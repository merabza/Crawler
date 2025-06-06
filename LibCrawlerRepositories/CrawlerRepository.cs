﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using CrawlerDb;
using CrawlerDb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace LibCrawlerRepositories;

public sealed class CrawlerRepository : ICrawlerRepository
{
    private readonly CrawlerDbContext _context;
    private readonly ILogger<CrawlerRepository> _logger;

    // ReSharper disable once ConvertToPrimaryConstructor
    public CrawlerRepository(CrawlerDbContext ctx, ILogger<CrawlerRepository> logger)
    {
        _context = ctx;
        _logger = logger;
    }

    public int SaveChanges()
    {
        try
        {
            return _context.SaveChanges();
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error occurred executing {nameof(SaveChanges)}.");
            throw;
        }
    }

    public int SaveChangesWithTransaction()
    {
        try
        {
            // ReSharper disable once using
            using var transaction = GetTransaction();
            try
            {
                var ret = _context.SaveChanges();
                transaction.Commit();
                return ret;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error occurred executing {nameof(SaveChangesWithTransaction)}.");
                transaction.Rollback();
                throw;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error occurred executing {nameof(SaveChangesWithTransaction)}.");
            throw;
        }
    }

    public IDbContextTransaction GetTransaction()
    {
        return _context.Database.BeginTransaction();
    }

    public HostModel CheckAddHostName(string hostName)
    {
        return _context.Hosts.SingleOrDefault(a => a.HostName == hostName) ??
               _context.Hosts.Add(new HostModel { HostName = hostName }).Entity;
    }

    public ExtensionModel CheckAddExtensionName(string extensionName)
    {
        return _context.Extensions.SingleOrDefault(a => a.ExtName == extensionName) ??
               _context.Extensions.Add(new ExtensionModel { ExtName = extensionName }).Entity;
    }

    public SchemeModel CheckAddSchemeName(string schemeName)
    {
        return _context.Schemes.SingleOrDefault(a => a.SchName == schemeName) ??
               _context.Schemes.Add(new SchemeModel { SchName = schemeName }).Entity;
    }

    public UrlModel? GetUrl(int hostId, int extId, int scmId, int urlHashCode, string strUrl)
    {
        var matchUrls = _context.Urls.Where(w =>
                w.UrlHashCode == urlHashCode && w.HostId == hostId && w.ExtensionId == extId && w.SchemeId == scmId)
            .ToList();
        return matchUrls.FirstOrDefault(url => url.UrlName == strUrl);
    }

    //public UrlModel AddUrl(string strUrl, int urlHashCode, HostModel host, ExtensionModel extension, SchemeModel scheme,
    //    bool isSiteMap)
    //{
    //    throw new NotImplementedException();
    //}

    //public UrlModel AddUrl(string strUrl, int urlHashCode, int hostId, int extensionId, int schemeId)
    public UrlModel AddUrl(string strUrl, int urlHashCode, HostModel host, ExtensionModel extension, SchemeModel scheme,
        bool isSiteMap, bool isAllowed)
    {
        return _context.Urls.Add(new UrlModel
        {
            UrlName = strUrl,
            HostNavigation = host,
            ExtensionNavigation = extension,
            SchemeNavigation = scheme,
            UrlHashCode = urlHashCode,
            IsSiteMap = isSiteMap,
            IsAllowed = isAllowed
        }).Entity;
    }

    public void AddUrlGraph(UrlGraphNode urlGraphNode)
    {
        _context.UrlGraphNodes.Add(urlGraphNode);
    }

    public void AddUrlGraph(int fromUrlPageId, UrlModel gotUrl, int batchPartId)
    {
        _context.UrlGraphNodes.Add(new UrlGraphNode
        {
            FromUrlId = fromUrlPageId, GotUrlNavigation = gotUrl, BatchPartId = batchPartId
        });
    }

    public List<string> GetHostStartUrlNamesByBatch(Batch batch)
    {
        return
        [
            .. _context.HostsByBatches.Where(w => w.BatchId == batch.BatchId).Include(i => i.SchemeNavigation)
                .Include(i => i.HostNavigation)
                .Select(s => $"{s.SchemeNavigation.SchName}://{s.HostNavigation.HostName}")
        ];
    }

    public void RemoveHostNamesByBatch(Batch batch, string schemeName, string hostName)
    {
        try
        {
            var hostByBatch = _context.HostsByBatches.SingleOrDefault(w =>
                w.BatchId == batch.BatchId && w.SchemeNavigation.SchName == schemeName &&
                w.HostNavigation.HostName == hostName);

            if (hostByBatch != null)
                _context.HostsByBatches.Remove(hostByBatch);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error occurred executing {nameof(RemoveHostNamesByBatch)}.");
            throw;
        }
    }

    public void AddHostNamesByBatch(Batch batch, string schemeName, string hostName)
    {
        try
        {
            var hostByBatch = _context.HostsByBatches.SingleOrDefault(w =>
                w.BatchId == batch.BatchId && w.SchemeNavigation.SchName == schemeName &&
                w.HostNavigation.HostName == hostName);

            if (hostByBatch != null)
                return;

            var scheme = _context.Schemes.SingleOrDefault(s => s.SchName == schemeName) ??
                         _context.Schemes.Add(new SchemeModel { SchName = schemeName }).Entity;

            var host = _context.Hosts.SingleOrDefault(s => s.HostName == hostName) ??
                       _context.Hosts.Add(new HostModel { HostName = hostName }).Entity;

            _context.HostsByBatches.Add(new HostByBatch
            {
                BatchId = batch.BatchId, SchemeNavigation = scheme, HostNavigation = host
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error occurred executing {nameof(CreateHost)}.");
            throw;
        }
    }

    public BatchPart? GetOpenedBatchPart(int batchId)
    {
        return _context.BatchParts.Include(i => i.BatchNavigation).ThenInclude(x => x.HostsByBatches)
            .SingleOrDefault(s => s.BatchId == batchId && s.Finished == null);
    }

    public BatchPart TryCreateNewPart(int batchId)
    {
        var newBatchPart = new BatchPart { BatchId = batchId, Created = DateTime.Now };
        return _context.BatchParts.Add(newBatchPart).Entity;
    }

    public void FinishBatchPart(BatchPart batchPart)
    {
        batchPart.Finished = DateTime.Now;
        _context.BatchParts.Update(batchPart);
    }

    public UrlGraphNode? GetUrlGraphEntry(int fromUrlPageId, int urlUrlId, int batchPartId)
    {
        return _context.UrlGraphNodes.Include(i => i.GotUrlNavigation).SingleOrDefault(s =>
            s.FromUrlId == fromUrlPageId && s.GotUrlId == urlUrlId && s.BatchPartId == batchPartId);
    }

    public void CreateContentAnalysisRecord(int batchPartBpId, int urlId, HttpStatusCode statusCode)
    {
        _context.ContentsAnalysis.Add(new ContentAnalysis
        {
            BatchPartId = batchPartBpId, UrlId = urlId, ResponseStatusCode = (int)statusCode, Finish = DateTime.Now
        });
    }

    public ContentAnalysis? GetContentAnalysis(int batchPartBpId, int urlId)
    {
        return _context.ContentsAnalysis.SingleOrDefault(s => s.BatchPartId == batchPartBpId && s.UrlId == urlId);
    }

    public void DeleteContentAnalysis(ContentAnalysis contentAnalysis)
    {
        _context.ContentsAnalysis.Remove(contentAnalysis);
    }

    public UrlModel UpdateUrlData(UrlModel urlModel)
    {
        try
        {
            return _context.Update(urlModel).Entity;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error occurred executing {nameof(UpdateUrlData)}.");
            throw;
        }
    }

    //public void ClearUrlAllows(int hostId)
    //{
    //    throw new NotImplementedException();
    //}

    //public void ClearUrlAllows(int hostId)
    //{
    //    _context.RemoveRange(_context.UrlAllows.Where(x => x.HostId == hostId));
    //}

    //public UrlAllowModel AddUrlAllow(int hostId, string patternText, bool isAllowed)
    //{
    //    return _context.UrlAllows.Add(new UrlAllowModel(hostId, patternText, isAllowed)).Entity;
    //}

    public string? LoadRobotsFromBase(int batchPartId, int schemeId, int hostId)
    {
        return _context.Robots
            .SingleOrDefault(x => x.BatchPartId == batchPartId && x.SchemeId == schemeId && x.HostId == hostId)
            ?.RobotsTxt;
    }

    public void SaveRobotsTxtToBase(int batchPartId, int schemeId, int hostId, string robotsTxt)
    {
        var robot = _context.Robots.SingleOrDefault(x =>
            x.BatchPartId == batchPartId && x.SchemeId == schemeId && x.HostId == hostId);
        if (robot is null)
        {
            robot = new Robot
            {
                BatchPartId = batchPartId, SchemeId = schemeId, HostId = hostId, RobotsTxt = robotsTxt
            };
            _context.Robots.Add(robot);
        }
        else
        {
            robot.RobotsTxt = robotsTxt;
            _context.Robots.Update(robot);
        }
    }

    public long GetUrlsCount(int batchPartId)
    {
        return (from bp in _context.BatchParts
            join hbb in _context.HostsByBatches on bp.BatchId equals hbb.BatchId
            join u in _context.Urls on new { hbb.HostId, hbb.SchemeId } equals new { u.HostId, u.SchemeId }
            where bp.BpId == batchPartId && u.IsAllowed
            select u).Count();
    }

    public long GetTermsCount()
    {
        return _context.Terms.Count();
    }

    public long GetLoadedUrlsCount(int batchPartId)
    {
        return (from bp in _context.BatchParts
            join hbb in _context.HostsByBatches on bp.BatchId equals hbb.BatchId
            join u in _context.Urls on new { hbb.HostId, hbb.SchemeId } equals new { u.HostId, u.SchemeId }
            join ca in _context.ContentsAnalysis on new { BatchPartId = bp.BpId, u.UrlId } equals new
            {
                ca.BatchPartId, ca.UrlId
            }
            where bp.BpId == batchPartId && u.IsAllowed
            select u).Count();
    }

    public TermType CheckAddTermType(string termTypeKey)
    {
        return _context.TermTypes.SingleOrDefault(a => a.TtKey == termTypeKey) ??
               _context.TermTypes.Add(new TermType { TtKey = termTypeKey }).Entity;
    }

    public Term? GetTerm(string termText)
    {
        return _context.Terms.FirstOrDefault(s => s.TermText == termText);
    }

    public Term AddTerm(string termText, TermType termTypeInBase)
    {
        return _context.Terms.Add(new Term { TermText = termText, TermTypeNavigation = termTypeInBase }).Entity;
    }

    public void AddTermByUrl(int batchPartId, int urlId, Term term, int position)
    {
        _context.TermsByUrls.Add(new TermByUrl
        {
            BatchPartId = batchPartId, UrlId = urlId, TermNavigation = term, Position = position
        });
    }

    public TermByUrl? GeTermByUrlEntry(int batchPartId, int urlId, int position)
    {
        return _context.TermsByUrls.SingleOrDefault(s =>
            s.BatchPartId == batchPartId && s.UrlId == urlId && s.Position == position);
    }

    public void ClearTermsTail(int batchPartId, int urlId, int position)
    {
        _context.TermsByUrls.RemoveRange(_context.TermsByUrls.Where(s =>
            s.BatchPartId == batchPartId && s.UrlId == urlId && s.Position >= position));
        //_context.SaveChanges();
    }

    public void EditTermByUrl(TermByUrl termByUrl, Term term)
    {
        termByUrl.TermNavigation = term;
        _context.TermsByUrls.Update(termByUrl);
    }

    public List<UrlModel> GetOnePortionUrls(int batchPartId, int maxCount)
    {
        return
        [
            .. (from bp in _context.BatchParts
                join hbb in _context.HostsByBatches on bp.BatchId equals hbb.BatchId
                join u in _context.Urls on new { hbb.HostId, hbb.SchemeId } equals new { u.HostId, u.SchemeId }
                join ca in _context.ContentsAnalysis on new { BatchPartId = bp.BpId, u.UrlId } equals new
                {
                    ca.BatchPartId, ca.UrlId
                } into gj
                from g in gj.DefaultIfEmpty()
                where g == null
                where bp.BpId == batchPartId && u.IsAllowed
                select u).Take(maxCount).Include(x => x.ExtensionNavigation)
        ];
    }

    #region Host cruder

    public List<HostModel> GetHostsList()
    {
        try
        {
            return _context.Hosts.ToList();
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error occurred executing {nameof(GetHostsList)}.");
            throw;
        }
    }

    public HostModel? GetHostByName(string hostName)
    {
        try
        {
            return _context.Hosts.SingleOrDefault(w => w.HostName == hostName);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error occurred executing {nameof(GetHostByName)}.");
            throw;
        }
    }

    public HostModel UpdateHost(HostModel host)
    {
        try
        {
            return _context.Update(host).Entity;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error occurred executing {nameof(UpdateHost)}.");
            throw;
        }
    }

    public HostModel CreateHost(HostModel newHost)
    {
        try
        {
            return _context.Hosts.Add(newHost).Entity;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error occurred executing {nameof(CreateHost)}.");
            throw;
        }
    }

    public HostModel DeleteHost(HostModel hostForDelete)
    {
        try
        {
            return _context.Hosts.Remove(hostForDelete).Entity;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error occurred executing {nameof(DeleteHost)}.");
            throw;
        }
    }

    #endregion

    #region Scheme cruder

    public List<SchemeModel> GetSchemesList()
    {
        try
        {
            return _context.Schemes.ToList();
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error occurred executing {nameof(GetSchemesList)}.");
            throw;
        }
    }

    public SchemeModel? GetSchemeByName(string schemeName)
    {
        try
        {
            return _context.Schemes.SingleOrDefault(w => w.SchName == schemeName);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error occurred executing {nameof(GetSchemeByName)}.");
            throw;
        }
    }

    public SchemeModel UpdateScheme(SchemeModel scheme)
    {
        try
        {
            return _context.Update(scheme).Entity;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error occurred executing {nameof(UpdateScheme)}.");
            throw;
        }
    }

    public SchemeModel CreateScheme(SchemeModel newScheme)
    {
        try
        {
            return _context.Schemes.Add(newScheme).Entity;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error occurred executing {nameof(CreateScheme)}.");
            throw;
        }
    }

    public SchemeModel DeleteScheme(SchemeModel schemeForDelete)
    {
        try
        {
            return _context.Schemes.Remove(schemeForDelete).Entity;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error occurred executing {nameof(DeleteScheme)}.");
            throw;
        }
    }

    #endregion

    #region Batch cruder

    public List<Batch> GetBatchesList()
    {
        try
        {
            return _context.Batches.ToList();
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error occurred executing {nameof(GetBatchesList)}.");
            throw;
        }
    }

    public Batch? GetBatchByName(string batchName)
    {
        try
        {
            return _context.Batches.SingleOrDefault(w => w.BatchName == batchName);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error occurred executing {nameof(GetBatchByName)}.");
            throw;
        }
    }

    public Batch UpdateBatch(Batch batch)
    {
        try
        {
            return _context.Update(batch).Entity;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error occurred executing {nameof(UpdateBatch)}.");
            throw;
        }
    }

    public Batch CreateBatch(Batch newBatch)
    {
        try
        {
            return _context.Batches.Add(newBatch).Entity;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error occurred executing {nameof(CreateBatch)}.");
            throw;
        }
    }

    public Batch DeleteBatch(Batch batchForDelete)
    {
        try
        {
            return _context.Batches.Remove(batchForDelete).Entity;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error occurred executing {nameof(DeleteBatch)}.");
            throw;
        }
    }

    #endregion
}