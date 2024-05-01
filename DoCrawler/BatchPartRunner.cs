using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using CrawlerDb.Models;
using DoCrawler.Domain;
using DoCrawler.Models;
using DoCrawler.States;
using LibCrawlerRepositories;
using LibDataInput;
using Microsoft.Extensions.Logging;
using RobotsTxt;
using SystemToolsShared;

namespace DoCrawler;

public sealed class BatchPartRunner
{
    private readonly BatchPart _batchPart;
    private readonly HttpClient _client = new();
    private readonly ConsoleFormatter _consoleFormatter = new();
    private readonly ILogger _logger;
    private readonly CrawlerParameters _par;
    private readonly ParseOnePageParameters _parseOnePageParameters;
    private readonly ICrawlerRepository _repository;
    private readonly UrlGraphDeDuplicator _urlGraphDeDuplicator;

    // ReSharper disable once ConvertToPrimaryConstructor
    public BatchPartRunner(ILogger logger, ICrawlerRepository repository, CrawlerParameters par,
        ParseOnePageParameters parseOnePageParameters, BatchPart batchPart)
    {
        _logger = logger;
        _repository = repository;
        _par = par;
        _parseOnePageParameters = parseOnePageParameters;
        _batchPart = batchPart;
        _urlGraphDeDuplicator = new UrlGraphDeDuplicator(repository);
    }

    public void InitBachPart(List<string> startPoints, Batch batch)
    {
        //_urlGraphNodes.Clear();
        var hostsByBatches = _repository.GetHostStartUrlNamesByBatch(batch);
        foreach (var hostName in hostsByBatches)
        {
            //შევამოწმოთ და თუ არ არსებობს შევქმნათ შემდეგი 2 ჩანაწერი მოსაქაჩი გვერდების სიაში:
            //1. {_hostName}
            //2. {_hostName}robots.txt
            TrySaveUrl($"{hostName}/", 0, _batchPart.BpId);
            //TrySaveUrl($"{host}/robots.txt", 0, _batchPart.BpId);
        }

        foreach (var uri in startPoints.Select(UriFabric.GetUri).Where(x => x is not null))
            TrySaveUrl(uri!.AbsoluteUri, 0, _batchPart.BpId);

        //_urlGraphDeDuplicator.CopyToRepository();

        SaveChangesAndReduceCache();

        //FIXME რაც არსებობს ამ პარტიის ფარგლებში ყველა Url დაკოპირდეს ახლად შექმნის ნაწილში.
        //ეს საშუალებას მოგვცემს დავადგინოთ საიტზე რომელიმე გვერდი მოკვდა თუ არა.
    }

    public void RunBatchPart()
    {
        //ჩაიტვირთოს ულუფა წინასწარ განსაზღვრული მაქსიმალური რაოდენობის მოუქაჩავი მისამართების
        //ეს მისამართები ჩადგეს რიგში და სათითაოდ თითოეულისთვის
        //აქედან უნდა დავიწყოთ პორციების ჩატვირთვის ციკლი
        while (LoadUrls(_batchPart))
        {
            while (ProcData.Instance.UrlsQueue.TryDequeue(out var urlModel))
                ProcessPage(urlModel, _batchPart);
            SaveChangesAndReduceCache();
        }
    }

    private bool LoadUrls(BatchPart batchPart)
    {
        StShared.ConsoleWriteInformationLine(_logger, true, "Loading next part Urls...");
        GetPagesState getPagesState = new(_logger, _repository, _par, batchPart);
        getPagesState.Execute();
        StShared.ConsoleWriteInformationLine(_logger, true,
            $"Loading Urls Finished. Urls count in queue is {ProcData.Instance.UrlsQueue.Count}");
        return getPagesState.UrlsLoaded;
    }

    private (HttpStatusCode, string?) GetOnePageContent(Uri uri)
    {
        try
        {
            // ReSharper disable once using
            using var response = _client.GetAsync(uri).Result;

            return response.IsSuccessStatusCode
                ? (response.StatusCode, response.Content.ReadAsStringAsync().Result)
                : (response.StatusCode, null);
        }
        catch
        {
            StShared.WriteErrorLine($"Error when downloading {uri}", true, _logger, false);
            //StShared.WriteException(e, true);
        }

        return (HttpStatusCode.BadRequest, null);
    }

    private (HttpStatusCode, string?) GetSiteMapGzFileContent(Uri uri)
    {
        try
        {
            // ReSharper disable once using
            using var response = _client.GetAsync(uri).Result;
            if (!response.IsSuccessStatusCode)
                return (response.StatusCode, null);

            // ReSharper disable once using
            using var stream = response.Content.ReadAsStream();
            // ReSharper disable once using
            // ReSharper disable once DisposableConstructor
            using var gzStream = new GZipStream(stream, CompressionMode.Decompress);
            // ReSharper disable once using
            // ReSharper disable once DisposableConstructor
            using var reader = new StreamReader(gzStream);
            var text = reader.ReadToEnd();

            return (response.StatusCode, text);

        }
        catch
        {
            StShared.WriteErrorLine($"Error when downloading {uri}", true, _logger, false);
            //StShared.WriteException(e, true);
        }

        return (HttpStatusCode.BadRequest, null);
    }

    private void AnalyzeAsRobotsText(string content, int fromUrlPageId, int batchPartId, int schemeId, int hostId)
    {
        StShared.ConsoleWriteInformationLine(_logger, true, "Analyze as robots.txt");

        _repository.SaveRobotsTxtToBase(batchPartId, schemeId, hostId, content);

        var robots = RobotsFabric.AnaliseContentAndCreateRobots(content);

        if (robots is null)
            return;

        ProcData.Instance.SetRobotsCache(hostId, robots);

        foreach (var robotsSitemap in robots.Sitemaps)
            TrySaveUrl(robotsSitemap.Url.ToString(), fromUrlPageId, batchPartId, true);

        _urlGraphDeDuplicator.CopyToRepository();

        SaveChangesAndReduceCache();
    }

    private void AnalyzeAsSiteMap(string content, int urlId, int bpId)
    {
        if (TryParseXml(content, out var element) && element is not null)
            AnalyzeAsSiteMapXml(element, urlId, bpId);
        else
            AnalyzeAsSiteMapText(content, urlId, bpId);
    }

    private void AnalyzeAsHtml(string content, UrlModel url, BatchPart batchPart)
    {
        Uri uri = new(url.UrlName);

        _consoleFormatter.WriteInSameLine($"Parsing      {uri}");
        var parseOnePageState = new ParseOnePageState(_logger, _parseOnePageParameters, content, url);
        parseOnePageState.Execute();

        _consoleFormatter.WriteInSameLine($"Save URLs    {uri}");
        foreach (var childUri in parseOnePageState.ListOfUris)
            TrySaveUrl(childUri, url.UrlId, batchPart.BpId);
        var position = 0;

        _consoleFormatter.WriteInSameLine($"Save Terms   {uri}");
        foreach (var uriTerm in parseOnePageState.UriTerms)
            TrySaveTerm(uriTerm.TermType, uriTerm.Context, url.UrlId, batchPart.BpId, position++);

        _consoleFormatter.WriteInSameLine($"Clear Tail   {uri}");
        ClearTermsTail(batchPart.BpId, url.UrlId, position);

        _urlGraphDeDuplicator.CopyToRepository();
    }

    private static bool TryParseXml(string xml, out XElement? element)
    {
        element = null;
        try
        {
            element = XElement.Parse(xml);
            return true;
        }
        catch (XmlException e)
        {
            StShared.WriteWarningLine(e.Message, true);
            return false;
        }
        catch (Exception ex)
        {
            StShared.WriteException(ex, true);
            return false;
        }
    }

    private void AnalyzeAsSiteMapXml(XElement element, int fromUrlPageId, int batchPartId)
    {
        //არსებობს Sitemap და Sitemap Index
        //ფორმატების აღწერა ვნახე მისამართზე
        //https://www.conductor.com/academy/xml-sitemap/

        //Sitemap Index-ის ნიმუშია
        /*
          <?xml version="1.0" encoding="UTF-8"?>
           <sitemapindex xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
            <sitemap>
                <loc>http://www.example.com/sitemap1.xml.gz</loc>
                <lastmod>2004-10-01T18:23:17+00:00</lastmod>
            </sitemap>
            <sitemap>
                <loc>http://www.example.com/sitemap2.xml.gz</loc>
                <lastmod>2005-01-01</lastmod>
            </sitemap>
           </sitemapindex>
        */

        //Sitemap-ის ნიმუშია
        /*
          <?xml version="1.0" encoding="UTF-8"?>
           <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
            <url>
                <loc>https://www.contentkingapp.com/</loc>
                <lastmod>2017-06-14T19:55:25+02:00</lastmod>
            </url>
            <url>
                <loc>https://www.contentkingapp.com/blog/</loc>
                <lastmod>2016-06-24T10:23:20+02:00</lastmod>
            </url>
           </urlset>
         */

        //პირველ რიგში უნდა დავადგინოთ Sitemap-ს ვაანალიზებთ თუ Sitemap Index-ს

        switch (element.Name.LocalName)
        {
            //თუ პირველი ტეგი არის sitemapindex, მაშინ საქმე გვაქვს Sitemap Index-თან
            case "sitemapindex":
                AnalyzeSiteMapXml(element, fromUrlPageId, batchPartId, true);
                break;
            //თუ პირველი ტეგი არის urlset, მაშინ საქმე გვაქვს Sitemap-თან
            case "urlset":
                AnalyzeSiteMapXml(element, fromUrlPageId, batchPartId, false);
                break;
            default:
                StShared.WriteErrorLine("Unknown XML Format", true, _logger, false);
                break;
        }
    }

    private void AnalyzeSiteMapXml(XContainer sitemapElement, int fromUrlPageId, int batchPartId, bool isIndex)
    {
        StShared.ConsoleWriteInformationLine(_logger, true, $"Analyze as Sitemap {(isIndex ? "Index " : " ")}XML");
        var sitemapNodeLocName = isIndex ? "sitemap" : "url";
        foreach (var smiNode in sitemapElement.Nodes())
        {
            var sitemapNode = smiNode as XElement;
            if (sitemapNode?.Name.LocalName != sitemapNodeLocName)
                continue;
            foreach (var smNode in sitemapNode.Nodes())
            {
                var locNode = smNode as XElement;
                if (locNode?.Name.LocalName == "loc")
                    TrySaveUrl(locNode.Value, fromUrlPageId, batchPartId, isIndex);
            }
        }

        _urlGraphDeDuplicator.CopyToRepository();
    }

    private void AnalyzeAsSiteMapText(string content, int fromUrlPageId, int batchPartId)
    {
        StShared.ConsoleWriteInformationLine(_logger, true, "Analyze as Sitemap Text");

        var lines = content.Split('\n');

        foreach (var line in lines)
            TrySaveUrl(line, fromUrlPageId, batchPartId);

        _urlGraphDeDuplicator.CopyToRepository();
    }

    private void TrySaveTerm(ETermType termType, string? termContext, int termUrlId, int termBatchPartId,
        int position)
    {
        try
        {
            var termTypeInBase = TrySaveTermType(termType);

            var termText = termType switch
            {
                ETermType.ParagraphStart => "<p>",
                ETermType.ParagraphFinish => "</p>",
                ETermType.StatementStart => "<s>",
                ETermType.StatementFinish => "</s>",
                ETermType.Word => termContext, //?.ToLower(),
                ETermType.Punctuation => termContext,
                _ => throw new ArgumentOutOfRangeException(nameof(termType), termType, null)
            };

            if (string.IsNullOrWhiteSpace(termText))
            {
                StShared.WriteErrorLine("termText is empty", true);
                return;
            }

            var term = ProcData.Instance.GetTermByName(termText);
            if (term == null && termTypeInBase.TtId != 0)
                term = _repository.GetTerm(termText);

            if (term == null)
            {
                term = _repository.AddTerm(termText, termTypeInBase);
                _repository.AddTermByUrl(termBatchPartId, termUrlId, term, position);
                ProcData.Instance.AddTerm(term);
            }
            else
            {
                var termByUrl = _repository.GeTermByUrlEntry(termBatchPartId, termUrlId, position);

                if (termByUrl == null)
                    _repository.AddTermByUrl(termBatchPartId, termUrlId, term, position);
                else
                    _repository.EditTermByUrl(termByUrl, term);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred executing {0} for {1}", nameof(TrySaveTerm), termContext);
            throw;
        }
    }

    private void ClearTermsTail(int batchPartId, int urlId, int position)
    {
        _repository.ClearTermsTail(batchPartId, urlId, position);
    }

    private TermType TrySaveTermType(ETermType termType)
    {
        var termTypeName = termType.ToString();
        var termTypeInBase = ProcData.Instance.GetTermTypeByKey(termTypeName);

        if (termTypeInBase != null)
            return termTypeInBase;

        termTypeInBase = _repository.CheckAddTermType(termTypeName);

        ProcData.Instance.AddTermType(termTypeInBase);

        return termTypeInBase;
    }

    public UrlModel? TrySaveUrl(string strUrl, int fromUrlPageId, int batchPartId, bool isSiteMap = false,
        bool isRobotsTxt = false)
    {
        try
        {
            var urlData = GetUrlData(strUrl);
            if (urlData == null)
                return null;

            if (urlData.Url is null)
            {
                //დადგინდეს Url შეესაბამება თუ არა robots.txt-ში მოცემულ წესებს
                var isAllowed = isRobotsTxt || IsUrlValidByRobotsRules(urlData);

                //ახალი url-ის დამატება
                urlData.Url = _repository.AddUrl(urlData.CheckedUri, urlData.UrlHashCode, urlData.Host,
                    urlData.Extension, urlData.Scheme, isSiteMap, isAllowed);
                _urlGraphDeDuplicator.AddUrlGraph(fromUrlPageId, urlData.Url, batchPartId);

                if (isAllowed && urlData.Url != null)
                    ProcData.Instance.AddUrl(urlData.Url);
            }
            else
            {
                //url-ი ბაზაში უკვე ყოფილა. გრაფში არის თუ არა, უნდა შემოწმდეს დამატებით
                if (urlData.Url.UrlId == 0 || fromUrlPageId == 0 || batchPartId == 0)
                    return urlData.Url;
                var urlGraphNode = _repository.GetUrlGraphEntry(fromUrlPageId, urlData.Url.UrlId, batchPartId);
                if (urlGraphNode is null)
                    _urlGraphDeDuplicator.AddUrlGraph(fromUrlPageId, urlData.Url, batchPartId);

            }

            return urlData.Url;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred executing {0} for {1}", nameof(TrySaveUrl), strUrl);
            throw;
        }
    }

    private bool IsUrlValidByRobotsRules(UrlData urlData)
    {
        var hostId = urlData.Host.HostId;
        var schemeId = urlData.Scheme.SchId;

        if (!_batchPart.BatchNavigation.HostsByBatches.Any(x => x.SchemeId == schemeId && x.HostId == hostId))
            return true;

        var robots = ProcData.Instance.GetRobots(hostId);

        if (robots is not null)
            return robots.IsPathAllowed("*", urlData.AbsolutePath);

        //დადგინდეს hostId-ისთვის ელემენტი არსებობს თუ არა რობოტების დიქშინარეში
        var isHostCachedInRobotsDictionary = ProcData.Instance.IsHostCachedInRobotsDictionary(hostId);
        //თუ დაქეშილი არ არის
        if (isHostCachedInRobotsDictionary)
            return robots is null || robots.IsPathAllowed("*", urlData.AbsolutePath);

        //შევეცადოთ ჩავტვირთოთ ბაზიდან, თუ რა თქმა უნდა გადანახული არის
        robots = RobotsFromBase(schemeId, hostId);

        if (robots is null)
            SaveUrlAndProcessOnePage($"{urlData.Scheme.SchName}://{urlData.Host.HostName}/robots.txt", false, true);

        robots = RobotsFromBase(schemeId, hostId);

        return robots is null || robots.IsPathAllowed("*", urlData.AbsolutePath);
    }

    private void SaveUrlAndProcessOnePage(string strUrl, bool isSiteMap = false, bool isRobotsTxt = false)
    {
        var urlModel = TrySaveUrl(strUrl, 0, _batchPart.BpId, isSiteMap, isRobotsTxt);
        if (urlModel is null)
            return;
        SaveChangesAndReduceCache();
        ProcessPage(urlModel, _batchPart);
        SaveChangesAndReduceCache();
    }

    private Robots? RobotsFromBase(int schemeId, int hostId)
    {
        var robotsTxt = _repository.LoadRobotsFromBase(_batchPart.BpId, schemeId, hostId);
        return robotsTxt is not null ? RobotsFabric.AnaliseContentAndCreateRobots(robotsTxt) : null;
    }

    private UrlData? GetUrlData(string strUrl)
    {
        var myUri = UriFabric.GetUri(strUrl);
        if (myUri == null)
            return null;

        var host = myUri.Host;
        var absolutePath = myUri.AbsolutePath;
        var extension = Path.GetExtension(absolutePath);
        var scheme = myUri.Scheme;
        if (extension == "")
            extension = "NoExtension";

        var hostModel = TrySaveHostName(host);
        var extensionModel = TrySaveExtension(extension);
        var schemeModel = TrySaveScheme(scheme);

        var checkedUri = HttpUtility.UrlDecode(myUri.AbsoluteUri);

        var urlHashCode = checkedUri.GetDeterministicHashCode();

        var url = ProcData.Instance.GetUrlByHashCode(urlHashCode);

        if ((url is null || url.UrlName != checkedUri) && hostModel.HostId != 0 && extensionModel.ExtId != 0 &&
            schemeModel.SchId != 0)
            url = _repository.GetUrl(hostModel.HostId, extensionModel.ExtId, schemeModel.SchId, urlHashCode,
                checkedUri);

        UrlData urlData = new(hostModel, extensionModel, schemeModel, checkedUri, absolutePath, urlHashCode, url);

        return urlData;
    }

    private HostModel TrySaveHostName(string hostName)
    {
        var host = ProcData.Instance.GetHostByName(hostName);

        if (host != null)
            return host;

        host = _repository.CheckAddHostName(hostName);

        ProcData.Instance.AddHost(host);

        return host;
    }

    private ExtensionModel TrySaveExtension(string extensionName)
    {
        var extension = ProcData.Instance.GetExtensionByName(extensionName);

        if (extension != null)
            return extension;

        extension = _repository.CheckAddExtensionName(extensionName);

        ProcData.Instance.AddExtension(extension);

        return extension;
    }

    private SchemeModel TrySaveScheme(string schemeName)
    {
        var scheme = ProcData.Instance.GetSchemeByName(schemeName);

        if (scheme != null)
            return scheme;

        scheme = _repository.CheckAddSchemeName(schemeName);

        ProcData.Instance.AddScheme(scheme);

        return scheme;
    }

    private void SaveChangesAndReduceCache()
    {
        StShared.ConsoleWriteInformationLine(_logger, true, "Save Changes");

        _repository.SaveChangesWithTransaction();

        ProcData.Instance.ReduceCache();
    }

    private void ProcessPage(UrlModel urlForProcess, BatchPart batchPart)
    {
        try
        {
            Uri uri = new(urlForProcess.UrlName);

            var startedAt = DateTime.Now;
            _consoleFormatter.WriteFirstLine($"Downloading  {uri}");

            HttpStatusCode statusCode;
            string? content;

            if (urlForProcess.IsSiteMap && string.Equals(urlForProcess.ExtensionNavigation.ExtName, ".gz",
                    StringComparison.CurrentCultureIgnoreCase))
                //მოიქაჩოს მისამართის მიხედვით Gz კონტენტი გახსნით
                (statusCode, content) = GetSiteMapGzFileContent(uri);
            else
                //მოიქაჩოს მისამართის მიხედვით კონტენტი
                (statusCode, content) = GetOnePageContent(uri);

            if (content == null)
            {
                StShared.WriteWarningLine($"Page is not Loaded: {uri}", true);
                return;
            }

            //urlForProcess.LastDownloaded = DateTime.Now;
            //urlForProcess.DownloadTryCount++;
            //_repository.UpdateUrlData(urlForProcess);

            //გაანალიზდეს კონტენტი კონტენტის ტიპის მიხედვით
            _consoleFormatter.WriteInSameLine($"Analyze content of {uri}");

            //robots.txt, sitemap, html
            if (uri.LocalPath == "/robots.txt")
                //გავაანალიზოთ როგორც robots.txt
                AnalyzeAsRobotsText(content, urlForProcess.UrlId, batchPart.BpId, urlForProcess.SchemeId,
                    urlForProcess.HostId);
            else if (urlForProcess.IsSiteMap)
                //გავაანალიზოთ როგორც SiteMap
                AnalyzeAsSiteMap(content, urlForProcess.UrlId, batchPart.BpId);
            else
                //გავაანალიზოთ როგორც Html
                AnalyzeAsHtml(content, urlForProcess, batchPart);

            //გაანალიზების შედეგად ნაპოვნი მისამართები დარეგისტრირდეს ბაზაში TrySaveUrl მეთოდის გამოყენებით

            //გაანალიზების შედეგად ნაპოვნი ქართული სიტყვები დარეგისტრირდეს ბაზაში

            //ასევე უნდა დარეგისტრირდეს სიტყვისა და მისამართის თანაკვეთა (ანუ სადაც ვიპოვეთ ეს სიტყვა)1

            _consoleFormatter.WriteInSameLine($"Copy Graph   {uri}");

            _urlGraphDeDuplicator.CopyToRepository();

            //დავადასტუროთ, რომ ამ გვერდის გაანალიზება მოხდა.
            _repository.CreateContentAnalysisRecord(batchPart.BpId, urlForProcess.UrlId, statusCode);

            _consoleFormatter.WriteInSameLine(
                $"Finished     {uri} ({DateTime.Now.MillisecondsDifference(startedAt)}ms)");

            if (ProcData.Instance.NeedsToReduceCache())
                SaveChangesAndReduceCache();
        }
        catch (Exception e)
        {
            StShared.WriteErrorLine($"Error when working on {urlForProcess.UrlName}", true);
            StShared.WriteException(e, true);
        }
    }

    public bool DoOnePage(string strUrl)
    {
        var urlData = GetUrlData(strUrl);
        if (urlData == null)
        {
            StShared.WriteErrorLine($"Cannot prepare data for uri {strUrl}", true);
            return false;
        }

        var contentAnalysis = urlData.Url is not null
            ? _repository.GetContentAnalysis(_batchPart.BpId, urlData.Url.UrlId)
            : null;
        if (contentAnalysis != null)
        {
            if (!Inputer.InputBool(
                    $"The page {strUrl} already analyzed. Do you wont to delete Content data for reanalyze", true,
                    false))
                return false;
            _repository.DeleteContentAnalysis(contentAnalysis);
        }

        SaveUrlAndProcessOnePage(urlData.CheckedUri);

        return true;
    }
}