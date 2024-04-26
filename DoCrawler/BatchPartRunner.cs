using System;
using System.Collections.Generic;
using System.IO;
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
        var hostsByBatches = _repository.GetHostNamesByBatch(batch);
        foreach (var host in hostsByBatches)
        {
            //შევამოწმოთ და თუ არ არსებობს შევქმნათ შემდეგი 2 ჩანაწერი მოსაქაჩი გვერდების სიაში:
            //1. {_hostName}
            //2. {_hostName}robots.txt
            TrySaveUrl($"{host}/", 0, _batchPart.BpId);
            TrySaveUrl($"{host}/robots.txt", 0, _batchPart.BpId);
        }

        foreach (var uri in startPoints.Select(UriFabric.GetUri).Where(x => x is not null))
            TrySaveUrl(uri!.AbsoluteUri, 0, _batchPart.BpId);

        _urlGraphDeDuplicator.CopyToRepository();

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
            while (ProcData.Instance.UrlsQueue.TryDequeue(out var result)) ProcessPage(result, _batchPart);

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
        catch (Exception e)
        {
            StShared.WriteErrorLine($"Error when downloading {uri}", true, _logger, false);
            //StShared.WriteException(e, true);
        }

        return (HttpStatusCode.BadRequest, null);
    }

    private void AnalyzeAsRobotsText(string content, int fromUrlPageId, int batchPartId)
    {
        StShared.ConsoleWriteInformationLine(_logger, true, "Analyze as robots.txt");

        var lines = content.Split(Environment.NewLine);

        foreach (var line in lines)
        {
            var colonIndex = line.IndexOf(':');
            if (colonIndex <= -1)
                continue;
            var command = line[..colonIndex].Trim();
            if (command.ToLower() != "sitemap")
                continue;
            var siteMapUrl = line[(colonIndex + 1)..].Trim();
            TrySaveUrl(siteMapUrl, fromUrlPageId, batchPartId, true);
        }
        //SaveChangesAndReduceCache();
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
        //StShared.ConsoleWriteInformationLine($"Parsing content from Url {uri}");
        _consoleFormatter.WriteInSameLine($"Parsing      {uri}");
        ParseOnePageState parseOnePageState = new(_logger, _parseOnePageParameters, content, url);
        parseOnePageState.Execute();
        //StShared.ConsoleWriteInformationLine($"Save URLs from content of Url {uri}");
        _consoleFormatter.WriteInSameLine($"Save URLs    {uri}");
        foreach (var childUri in parseOnePageState.ListOfUris)
            TrySaveUrl(childUri, url.UrlId, batchPart.BpId);
        var position = 0;
        //StShared.ConsoleWriteInformationLine($"Save Terms from content of Url {uri}");
        _consoleFormatter.WriteInSameLine($"Save Terms   {uri}");
        foreach (var uriTerm in parseOnePageState.UriTerms)
            TrySaveTerm(uriTerm.TermType, uriTerm.Context, url.UrlId, batchPart.BpId, position++);
        //StShared.ConsoleWriteInformationLine($"Clear Tail for Url {uri}");
        _consoleFormatter.WriteInSameLine($"Clear Tail   {uri}");
        ClearTermsTail(batchPart.BpId, url.UrlId, position);
        //StShared.ConsoleWriteInformationLine("Save Changes And Reduce Cache");
        //_consoleFormatter.WriteInSameLine($"Save Changes {uri}");
        //SaveChangesAndReduceCache();
        //StShared.ConsoleWriteInformationLine($"Finished Parsing content from Url {uri}");
        //_consoleFormatter.WriteInSameLine($"Finished     {uri}");
        //_consoleFormatter.Clear();
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
                AnalyzeSiteMapIndexXml(element.Name.NamespaceName, element, fromUrlPageId, batchPartId);
                break;
            //თუ პირველი ტეგი არის urlset, მაშინ საქმე გვაქვს Sitemap-თან
            case "urlset":
                AnalyzeSiteMapXml(element.Name.NamespaceName, element, fromUrlPageId, batchPartId);
                break;
            default:
                StShared.WriteErrorLine("Unknown XML Format", true,_logger, false);
                break;
        }
    }

    private void AnalyzeSiteMapXml(string namespaceName, XElement urlsetElement, int fromUrlPageId, int batchPartId)
    {
        StShared.ConsoleWriteInformationLine(_logger, true, "Analyze as Sitemap Index XML");
    }

    private void AnalyzeSiteMapIndexXml(string namespaceName, XContainer sitemapindexElement, int fromUrlPageId, int batchPartId)
    {
        StShared.ConsoleWriteInformationLine(_logger, true, "Analyze as Sitemap Index XML");
        foreach (var smiNode in sitemapindexElement.Nodes())
        {
            var sitemapNode = smiNode as XElement;
            if (sitemapNode?.Name.LocalName != "sitemap") 
                continue;
            foreach (var smNode in sitemapNode.Nodes())
            {
                var locNode = smNode as XElement;
                if (locNode?.Name.LocalName == "loc") 
                    TrySaveUrl(locNode.Value, fromUrlPageId, batchPartId);
            }
        }
    }

    private void AnalyzeAsSiteMapText(string content, int fromUrlPageId, int batchPartId)
    {
        StShared.ConsoleWriteInformationLine(_logger, true, "Analyze as Sitemap Text");

        var lines = content.Split('\n');

        foreach (var line in lines) TrySaveUrl(line, fromUrlPageId, batchPartId);
        //SaveChangesAndReduceCache();
    }

    private void TrySaveTerm(ETermType termType, string? termContext, int termUrlId, int termBatchPartId,
        int position)
    {
        try
        {
            var termTypeInBase = TrySaveTermType(termType);

            //if (termTypeInBase == null)
            //    return;

            //using var transaction = _repository.GetTransaction();
            //try
            //{
            var termText = termType switch
            {
                ETermType.ParagraphStart => "<p>",
                ETermType.ParagraphFinish => "</p>",
                ETermType.StatementStart => "<s>",
                ETermType.StatementFinish => "</s>",
                ETermType.Word => termContext?.ToLower(),
                ETermType.Punctuation => termContext,
                _ => throw new ArgumentOutOfRangeException(nameof(termType), termType, null)
            };
            //bool foundInCache = true;

            if (string.IsNullOrWhiteSpace(termText))
            {
                StShared.WriteErrorLine("termText is empty", true);
                return;
            }

            var term = ProcData.Instance.GetTermByName(termText);
            if (term == null && termTypeInBase.TtId != 0)
                //foundInCache = false;
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
            //if (!foundInCache)
            //{
            //}

            //  _repository.SaveChanges();
            //  transaction.Commit();
            //}
            //catch (Exception e)
            //{
            //  transaction.Rollback();
            //  _logger.LogError(e, $"Error occurred executing {nameof(TrySaveTerm)} for {termContext}");
            //  throw;
            //}
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

        //if (termTypeInBase != null)
        ProcData.Instance.AddTermType(termTypeInBase);

        return termTypeInBase;
    }

    public void TrySaveUrl(string strUrl, int fromUrlPageId, int batchPartId, bool isSiteMap = false)
    {
        try
        {
            var urlData = GetUrl(strUrl);
            if (urlData == null)
                return;

            UrlGraphNode? urlGraphNode = null;

            if (urlData.Url == null)
            {
                //ახალი url-ის დამატება
                urlData.Url = _repository.AddUrl(urlData.CheckedUri, urlData.UrlHashCode, urlData.Host,
                    urlData.Extension, urlData.Scheme, isSiteMap);
                _urlGraphDeDuplicator.AddUrlGraph(fromUrlPageId, urlData.Url, batchPartId);

                if (urlData.Url != null) ProcData.Instance.AddUrl(urlData.Url);
            }
            else
            {
                //url-ი ბაზაში უკვე ყოფილა. გრაფში არის თუ არა, უნდა შემოწმდეს დამატებით
                if (urlData.Url.UrlId != 0)
                    urlGraphNode = _repository.GetUrlGraphEntry(fromUrlPageId, urlData.Url.UrlId, batchPartId);
            }

            if (urlGraphNode == null && fromUrlPageId != 0 && batchPartId != 0)
            {
                if (urlData.Url is null)
                    throw new Exception("urlData.Url is null");
                _urlGraphDeDuplicator.AddUrlGraph(fromUrlPageId, urlData.Url, batchPartId);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred executing {0} for {1}", nameof(TrySaveUrl), strUrl);
            throw;
        }
    }

    private UrlData? GetUrl(string strUrl)
    {
        var myUri = UriFabric.GetUri(strUrl);
        if (myUri == null)
            return null;

        var host = myUri.Host;
        var extension = Path.GetExtension(myUri.AbsolutePath);
        var scheme = myUri.Scheme;
        if (extension == "")
            extension = "NoExtension";

        var hostModel = TrySaveHostName(host);
        var extensionModel = TrySaveExtension(extension);
        var schemeModel = TrySaveScheme(scheme);

        //if (hostModel == null || extensionModel == null || schemeModel == null)
        //    return null;

        var checkedUri = HttpUtility.UrlDecode(myUri.AbsoluteUri);

        var urlHashCode = checkedUri.GetDeterministicHashCode();

        var url = ProcData.Instance.GetUrlByHashCode(urlHashCode);

        if ((url == null || url.UrlName != checkedUri) && hostModel.HostId != 0 && extensionModel.ExtId != 0 &&
            schemeModel.SchId != 0)
            url = _repository.GetUrl(hostModel.HostId, extensionModel.ExtId, schemeModel.SchId,
                urlHashCode, checkedUri);

        UrlData urlData = new(hostModel, extensionModel, schemeModel, checkedUri, urlHashCode, url);

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
            //string siteMapUrl =  result.HostId 
            Uri uri = new(urlForProcess.UrlName);

            var startedAt = DateTime.Now;
            _consoleFormatter.WriteFirstLine($"Downloading  {uri}");

            //მოიქაჩოს მისამართის მიხედვით კონტენტი
            var (statusCode, content) = GetOnePageContent(uri);

            if (content == null)
            {
                StShared.WriteWarningLine($"Not Loaded page: {uri}", true);
                return;
            }

            //გაანალიზდეს კონტენტი კონტენტის ტიპის მიხედვით
            //StShared.ConsoleWriteInformationLine($"Analyze content of {uri}");
            _consoleFormatter.WriteInSameLine($"Analyze content of {uri}");

            //robots.txt, sitemap, html
            if (uri.LocalPath == "/robots.txt")
                //გავაანალიზოთ როგორც robots.txt
                AnalyzeAsRobotsText(content, urlForProcess.UrlId, batchPart.BpId);
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
            //_repository.SaveChanges();


            _consoleFormatter.WriteInSameLine(
                $"Finished     {uri} ({DateTime.Now.MillisecondsDifference(startedAt)}ms)");

            if (ProcData.Instance.NeedsToReduceCache())
                //_consoleFormatter.WriteInSameLine($"Save Changes {uri}");
                SaveChangesAndReduceCache();
        }
        catch (Exception e)
        {
            StShared.WriteErrorLine($"Error when working on {urlForProcess.UrlName}", true);
            StShared.WriteException(e, true);
        }
    }

    public bool DoOnePage(string strUrl, BatchPart batchPart)
    {
        var urlData = GetUrl(strUrl);
        if (urlData == null)
        {
            StShared.WriteErrorLine($"Cannot prepare data for uri {strUrl}", true);
            return false;
        }

        if (urlData.Url is null)
            throw new Exception("urlData.Url is null");

        var contentAnalysis = _repository.GetContentAnalysis(batchPart.BpId, urlData.Url.UrlId);
        if (contentAnalysis != null)
        {
            if (!Inputer.InputBool(
                    $"The page {strUrl} already analyzed. Do you wont to delete Content data for reanalyze", true,
                    false))
                return false;
            _repository.DeleteContentAnalysis(contentAnalysis);
        }

        ProcessPage(urlData.Url, batchPart);
        SaveChangesAndReduceCache();
        return true;
    }
}