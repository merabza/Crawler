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
using CrawlerDb.Configurations;
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
    private readonly ConsoleFormatter _consoleFormatter = new();
    private readonly ICrawlerRepositoryCreatorFactory _crawlerRepositoryCreatorFactory;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;
    private readonly CrawlerParameters _par;

    private readonly ParseOnePageParameters _parseOnePageParameters;

    private ProcData _procData = new();
    //private readonly UrlGraphDeDuplicator _urlGraphDeDuplicator;

    // ReSharper disable once ConvertToPrimaryConstructor
    public BatchPartRunner(ILogger logger, IHttpClientFactory httpClientFactory,
        ICrawlerRepositoryCreatorFactory crawlerRepositoryCreatorFactory, CrawlerParameters par,
        ParseOnePageParameters parseOnePageParameters, BatchPart batchPart)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _crawlerRepositoryCreatorFactory = crawlerRepositoryCreatorFactory;
        _par = par;
        _parseOnePageParameters = parseOnePageParameters;
        _batchPart = batchPart;
        //_urlGraphDeDuplicator = new UrlGraphDeDuplicator(repository);
    }

    public void InitBachPart(List<string> startPoints, Batch batch)
    {
        var crawlerRepository = _crawlerRepositoryCreatorFactory.GetCrawlerRepository();

        //_urlGraphNodes.Clear();
        var hostsByBatches = crawlerRepository.GetHostStartUrlNamesByBatch(batch);
        foreach (var hostName in hostsByBatches)
        {
            //შევამოწმოთ და თუ არ არსებობს შევქმნათ შემდეგი 2 ჩანაწერი მოსაქაჩი გვერდების სიაში:
            //1. {_hostName}
            //2. {_hostName}robots.txt
            TrySaveUrl(crawlerRepository, $"{hostName}/", 0, _batchPart.BpId);
            TrySaveUrl(crawlerRepository, $"{hostName}/robots.txt", 0, _batchPart.BpId);
        }

        foreach (var uri in startPoints.Select(UriFactory.GetUri).Where(x => x is not null))
            TrySaveUrl(crawlerRepository, uri!.AbsoluteUri, 0, _batchPart.BpId);

        //_urlGraphDeDuplicator.CopyToRepository();

        SaveChangesAndReduceCache(crawlerRepository);

        //FIXME რაც არსებობს ამ პარტიის ფარგლებში ყველა Url დაკოპირდეს ახლად შექმნის ნაწილში.
        //ეს საშუალებას მოგვცემს დავადგინოთ საიტზე რომელიმე გვერდი მოკვდა თუ არა.
    }

    public void RunBatchPart()
    {
        //ჩაიტვირთოს ულუფა წინასწარ განსაზღვრული მაქსიმალური რაოდენობის მოუქაჩავი მისამართების
        //ეს მისამართები ჩადგეს რიგში და სათითაოდ თითოეულისთვის
        //აქედან უნდა დავიწყოთ პორციების ჩატვირთვის ციკლი
        while (true)
        {
            var crawlerRepository = _crawlerRepositoryCreatorFactory.GetCrawlerRepository();
            _procData = new ProcData();

            var loadedUrls = LoadUrls(crawlerRepository, _batchPart);

            if (loadedUrls.Count == 0)
                break;

            _consoleFormatter.UseCurrentLine();

            var analizedCount = 0;
            foreach (var urlModel in loadedUrls)
            {
                ProcessPage(crawlerRepository, urlModel, _batchPart);
                analizedCount++;
                if (!_procData.NeedsToReduceCache() && !crawlerRepository.NeedSaveChanges())
                    continue;

                SaveChangesAndReduceCache(crawlerRepository);
                crawlerRepository = _crawlerRepositoryCreatorFactory.GetCrawlerRepository();
                _procData = new ProcData();

                StShared.ConsoleWriteInformationLine(_logger, true,
                    $"Analized {analizedCount} from {loadedUrls.Count} loaded Urls");
                
                _consoleFormatter.UseCurrentLine();

            }

            SaveChangesAndReduceCache(crawlerRepository);
        }
    }

    private List<UrlModel> LoadUrls(ICrawlerRepository crawlerRepository, BatchPart batchPart)
    {
        StShared.ConsoleWriteInformationLine(_logger, true, "Loading next part Urls...");

        CountStatistics(crawlerRepository);

        var getPagesState = new GetPagesState(_logger, crawlerRepository, _par, batchPart);
        var urlsLoaded = getPagesState.GetPages();
        StShared.ConsoleWriteInformationLine(_logger, true,
            $"Loading Urls Finished. Urls count in queue is {urlsLoaded.Count}");
        return urlsLoaded;
    }

    private void CountStatistics(ICrawlerRepository crawlerRepository)
    {
        var urlsCount = crawlerRepository.GetUrlsCount(_batchPart.BpId);

        var loadedUrlsCount = crawlerRepository.GetLoadedUrlsCount(_batchPart.BpId);

        var termsCount = crawlerRepository.GetTermsCount();

        StShared.ConsoleWriteInformationLine(_logger, true,
            $"[{DateTime.Now}] Urls {loadedUrlsCount}-{urlsCount} terms {termsCount}");
    }

    private (HttpStatusCode, DateTime?, string?) GetOnePageContent(Uri uri)
    {
        try
        {
            // ReSharper disable once using
            var client = _httpClientFactory.CreateClient();
            // ReSharper disable once using
            using var response = client.GetAsync(uri).Result;

            //response.Headers.

            return response.IsSuccessStatusCode
                ? (response.StatusCode, GetPageLastModified(response), response.Content.ReadAsStringAsync().Result)
                : (response.StatusCode, null, null);
        }
        catch
        {
            StShared.WriteErrorLine($"Error when downloading {uri}", true, _logger, false);
            //StShared.WriteException(e, true);
        }

        return (HttpStatusCode.BadRequest, null, null);
    }

    private DateTime? GetPageLastModified(Uri uri)
    {
        // ReSharper disable once using
        var client = _httpClientFactory.CreateClient();
        // ReSharper disable once using
        // ReSharper disable once DisposableConstructor
        using var request = new HttpRequestMessage(HttpMethod.Head, uri);
        // ReSharper disable once using
        using var response = client.Send(request);

        return GetPageLastModified(response);
    }

    private static DateTime? GetPageLastModified(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Last-Modified", out var values))
            return null;
        var lastModifiedStr = values.FirstOrDefault();
        if (DateTime.TryParse(lastModifiedStr, out var lastModified))
            return lastModified;

        return null;
    }

    private (HttpStatusCode, DateTime?, string?) GetSiteMapGzFileContent(Uri uri)
    {
        try
        {
            // ReSharper disable once using
            var client = _httpClientFactory.CreateClient();
            // ReSharper disable once using
            using var response = client.GetAsync(uri).Result;
            if (!response.IsSuccessStatusCode)
                return (response.StatusCode, null, null);

            // ReSharper disable once using
            using var stream = response.Content.ReadAsStream();
            // ReSharper disable once using
            // ReSharper disable once DisposableConstructor
            using var gzStream = new GZipStream(stream, CompressionMode.Decompress);
            // ReSharper disable once using
            // ReSharper disable once DisposableConstructor
            using var reader = new StreamReader(gzStream);
            var text = reader.ReadToEnd();

            return (response.StatusCode, GetPageLastModified(response), text);
        }
        catch
        {
            StShared.WriteErrorLine($"Error when downloading {uri}", true, _logger, false);
            //StShared.WriteException(e, true);
        }

        return (HttpStatusCode.BadRequest, null, null);
    }

    private void AnalyzeAsRobotsText(ICrawlerRepository crawlerRepository, string content, int fromUrlPageId,
        int batchPartId, int schemeId, int hostId)
    {
        StShared.ConsoleWriteInformationLine(_logger, true, "Analyze as robots.txt");

        crawlerRepository.SaveRobotsTxtToBase(batchPartId, schemeId, hostId, content);

        var robots = RobotsFactory.AnaliseContentAndCreateRobots(content);

        if (robots is null)
            return;

        _procData.SetRobotsCache(hostId, robots);

        foreach (var robotsSitemap in robots.Sitemaps)
            TrySaveUrl(crawlerRepository, robotsSitemap.Url.ToString(), fromUrlPageId, batchPartId, true);

        var urlGraphDeDuplicator = new UrlGraphDeDuplicator(crawlerRepository);

        urlGraphDeDuplicator.CopyToRepository();

        SaveChangesAndReduceCache(crawlerRepository);
    }

    private void AnalyzeAsSiteMap(ICrawlerRepository crawlerRepository, string content, int urlId, int bpId)
    {
        if (TryParseXml(content, out var element) && element is not null)
            AnalyzeAsSiteMapXml(crawlerRepository, element, urlId, bpId);
        else
            AnalyzeAsSiteMapText(crawlerRepository, content, urlId, bpId);
    }

    private void AnalyzeAsHtml(ICrawlerRepository crawlerRepository, string content, UrlModel url, BatchPart batchPart)
    {
        var uri = new Uri(url.UrlName);

        _consoleFormatter.WriteInSameLine("Parsing", uri.ToString());
        var parseOnePageState = new ParseOnePageState(_logger, _parseOnePageParameters, content, url);
        parseOnePageState.Execute();

        _consoleFormatter.WriteInSameLine("Save URLs", uri.ToString());
        foreach (var childUri in parseOnePageState.ListOfUris)
            TrySaveUrl(crawlerRepository, childUri, url.UrlId, batchPart.BpId);
        var position = 0;

        _consoleFormatter.WriteInSameLine("Save Terms", uri.ToString());
        foreach (var uriTerm in parseOnePageState.UriTerms)
            TrySaveTerm(crawlerRepository, uriTerm.TermType, uriTerm.Context, url.UrlId, batchPart.BpId, position++);

        _consoleFormatter.WriteInSameLine("Clear Tail", uri.ToString());
        ClearTermsTail(crawlerRepository, batchPart.BpId, url.UrlId, position);

        var urlGraphDeDuplicator = new UrlGraphDeDuplicator(crawlerRepository);

        urlGraphDeDuplicator.CopyToRepository();
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

    private void AnalyzeAsSiteMapXml(ICrawlerRepository crawlerRepository, XElement element, int fromUrlPageId,
        int batchPartId)
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
                AnalyzeSiteMapXml(crawlerRepository, element, fromUrlPageId, batchPartId, true);
                break;
            //თუ პირველი ტეგი არის urlset, მაშინ საქმე გვაქვს Sitemap-თან
            case "urlset":
                AnalyzeSiteMapXml(crawlerRepository, element, fromUrlPageId, batchPartId, false);
                break;
            default:
                StShared.WriteErrorLine("Unknown XML Format", true, _logger, false);
                break;
        }
    }

    private void AnalyzeSiteMapXml(ICrawlerRepository crawlerRepository, XContainer sitemapElement, int fromUrlPageId,
        int batchPartId, bool isIndex)
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
                    TrySaveUrl(crawlerRepository, locNode.Value, fromUrlPageId, batchPartId, isIndex);
            }
        }

        var urlGraphDeDuplicator = new UrlGraphDeDuplicator(crawlerRepository);

        urlGraphDeDuplicator.CopyToRepository();
    }

    private void AnalyzeAsSiteMapText(ICrawlerRepository crawlerRepository, string content, int fromUrlPageId,
        int batchPartId)
    {
        StShared.ConsoleWriteInformationLine(_logger, true, "Analyze as Sitemap Text");

        var lines = content.Split('\n');

        foreach (var line in lines)
            TrySaveUrl(crawlerRepository, line, fromUrlPageId, batchPartId);

        var urlGraphDeDuplicator = new UrlGraphDeDuplicator(crawlerRepository);

        urlGraphDeDuplicator.CopyToRepository();
    }

    private void TrySaveTerm(ICrawlerRepository crawlerRepository, ETermType termType, string? termContext,
        int termUrlId, int termBatchPartId, int position)
    {
        try
        {
            var termTypeInBase = TrySaveTermType(crawlerRepository, termType);

            var termText = termType switch
            {
                ETermType.ParagraphStart => "<p>",
                ETermType.ParagraphFinish => "</p>",
                ETermType.StatementStart => "<s>",
                ETermType.StatementFinish => "</s>",
                ETermType.ForeignWord => termContext,
                ETermType.Word => termContext,
                ETermType.Punctuation => termContext,
                _ => throw new ArgumentOutOfRangeException(nameof(termType), termType, null)
            };

            if (string.IsNullOrWhiteSpace(termText))
            {
                StShared.WriteErrorLine("termText is empty", true);
                return;
            }

            var term = _procData.GetTermByName(termText);
            if (term == null && termTypeInBase.TtId != 0)
                term = crawlerRepository.GetTerm(termText);

            if (term == null)
            {
                term = crawlerRepository.AddTerm(termText, termTypeInBase);
                crawlerRepository.AddTermByUrl(termBatchPartId, termUrlId, term, position);
                _procData.AddTerm(term);
            }
            else
            {
                var termByUrl = crawlerRepository.GeTermByUrlEntry(termBatchPartId, termUrlId, position);

                if (termByUrl == null)
                    crawlerRepository.AddTermByUrl(termBatchPartId, termUrlId, term, position);
                else
                    crawlerRepository.EditTermByUrl(termByUrl, term);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred executing {0} for {1}", nameof(TrySaveTerm), termContext);
            throw;
        }
    }

    private void ClearTermsTail(ICrawlerRepository crawlerRepository, int batchPartId, int urlId, int position)
    {
        crawlerRepository.ClearTermsTail(batchPartId, urlId, position);
    }

    private TermType TrySaveTermType(ICrawlerRepository crawlerRepository, ETermType termType)
    {
        var termTypeName = termType.ToString();
        var termTypeInBase = _procData.GetTermTypeByKey(termTypeName);

        if (termTypeInBase != null)
            return termTypeInBase;

        termTypeInBase = crawlerRepository.CheckAddTermType(termTypeName);

        _procData.AddTermType(termTypeInBase);

        return termTypeInBase;
    }

    public UrlModel? TrySaveUrl(ICrawlerRepository crawlerRepository, string strUrl, int fromUrlPageId, int batchPartId,
        bool isSiteMap = false, bool isRobotsTxt = false)
    {
        try
        {
            var urlData = GetUrlData(crawlerRepository, strUrl);
            if (urlData == null)
                return null;

            if (urlData.Url is null)
            {
                //დადგინდეს Url შეესაბამება თუ არა robots.txt-ში მოცემულ წესებს
                var isAllowed = isRobotsTxt || IsUrlValidByRobotsRules(crawlerRepository, urlData);

                //ახალი url-ის დამატება
                urlData.Url = crawlerRepository.AddUrl(urlData.CheckedUri, urlData.UrlHashCode, urlData.Host,
                    urlData.Extension, urlData.Scheme, isSiteMap, isAllowed);

                var urlGraphDeDuplicator = new UrlGraphDeDuplicator(crawlerRepository);

                urlGraphDeDuplicator.AddUrlGraph(fromUrlPageId, urlData.Url, batchPartId);

                if (isAllowed && urlData.Url != null)
                    _procData.AddUrl(urlData.Url);
            }
            else
            {
                //url-ი ბაზაში უკვე ყოფილა. გრაფში არის თუ არა, უნდა შემოწმდეს დამატებით
                if (urlData.Url.UrlId == 0 || fromUrlPageId == 0 || batchPartId == 0)
                    return urlData.Url;
                var urlGraphNode = crawlerRepository.GetUrlGraphEntry(fromUrlPageId, urlData.Url.UrlId, batchPartId);
                if (urlGraphNode is not null)
                    return urlData.Url;
                var urlGraphDeDuplicator = new UrlGraphDeDuplicator(crawlerRepository);
                urlGraphDeDuplicator.AddUrlGraph(fromUrlPageId, urlData.Url, batchPartId);
            }

            return urlData.Url;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred executing {0} for {1}", nameof(TrySaveUrl), strUrl);
            throw;
        }
    }

    private bool IsUrlValidByRobotsRules(ICrawlerRepository crawlerRepository, UrlData urlData)
    {
        var hostId = urlData.Host.HostId;
        var schemeId = urlData.Scheme.SchId;

        if (!_batchPart.BatchNavigation.HostsByBatches.Any(x => x.SchemeId == schemeId && x.HostId == hostId))
            return true;

        var robots = _procData.GetRobots(hostId);

        if (robots is not null)
            return robots.IsPathAllowed("*", urlData.AbsolutePath);

        //დადგინდეს hostId-ისთვის ელემენტი არსებობს თუ არა რობოტების დიქშინარეში
        var isHostCachedInRobotsDictionary = _procData.IsHostCachedInRobotsDictionary(hostId);
        //თუ დაქეშილი არ არის
        if (isHostCachedInRobotsDictionary)
            return robots is null || robots.IsPathAllowed("*", urlData.AbsolutePath);

        //შევეცადოთ ჩავტვირთოთ ბაზიდან, თუ რა თქმა უნდა გადანახული არის
        robots = RobotsFromBase(crawlerRepository, schemeId, hostId);

        //if (robots is null)
        //    SaveUrlAndProcessOnePage(crawlerRepository,
        //        $"{urlData.Scheme.SchName}://{urlData.Host.HostName}/robots.txt", false, true);

        //robots = RobotsFromBase(crawlerRepository, schemeId, hostId);

        return robots is null || robots.IsPathAllowed("*", urlData.AbsolutePath);
    }

    private void SaveUrlAndProcessOnePage(ICrawlerRepository crawlerRepository, string strUrl, bool isSiteMap = false,
        bool isRobotsTxt = false)
    {
        var urlModel = TrySaveUrl(crawlerRepository, strUrl, 0, _batchPart.BpId, isSiteMap, isRobotsTxt);
        if (urlModel is null)
            return;
        SaveChangesAndReduceCache(crawlerRepository);
        ProcessPage(crawlerRepository, urlModel, _batchPart);
        SaveChangesAndReduceCache(crawlerRepository);
    }

    private Robots? RobotsFromBase(ICrawlerRepository crawlerRepository, int schemeId, int hostId)
    {
        var robotsTxt = crawlerRepository.LoadRobotsFromBase(_batchPart.BpId, schemeId, hostId);
        return robotsTxt is not null ? RobotsFactory.AnaliseContentAndCreateRobots(robotsTxt) : null;
    }

    private UrlData? GetUrlData(ICrawlerRepository crawlerRepository, string strUrl)
    {
        var myUri = UriFactory.GetUri(strUrl);
        if (myUri == null)
            return null;

        var host = myUri.Host.Truncate(HostModelConfiguration.HostNameLength);
        if (string.IsNullOrWhiteSpace(host))
            host = "InvalidHostName";

        var absolutePath = myUri.AbsolutePath;

        var extension = Path.GetExtension(absolutePath).Truncate(ExtensionModelConfiguration.ExtensionNameLength);
        if (string.IsNullOrWhiteSpace(extension))
            extension = "NoExtension";

        var scheme = myUri.Scheme.Truncate(SchemeModelConfiguration.SchemeNameLength);
        if (string.IsNullOrWhiteSpace(scheme))
            scheme = "InvalidSchemeName";

        var hostModel = TrySaveHostName(crawlerRepository, host);
        var extensionModel = TrySaveExtension(crawlerRepository, extension);
        var schemeModel = TrySaveScheme(crawlerRepository, scheme);

        var checkedUri = HttpUtility.UrlDecode(myUri.AbsoluteUri).Truncate(UrlModelConfiguration.TermTextLength);
        if (string.IsNullOrWhiteSpace(checkedUri))
            checkedUri = "InvalidUri";

        var urlHashCode = checkedUri.GetDeterministicHashCode();

        var url = _procData.GetUrlByHashCode(urlHashCode);

        if ((url is null || url.UrlName != checkedUri) && hostModel.HostId != 0 && extensionModel.ExtId != 0 &&
            schemeModel.SchId != 0)
            url = crawlerRepository.GetUrl(hostModel.HostId, extensionModel.ExtId, schemeModel.SchId, urlHashCode,
                checkedUri);

        var urlData = new UrlData(hostModel, extensionModel, schemeModel, checkedUri, absolutePath, urlHashCode, url);

        return urlData;
    }

    private HostModel TrySaveHostName(ICrawlerRepository crawlerRepository, string hostName)
    {
        var host = _procData.GetHostByName(hostName);

        if (host != null)
            return host;

        host = crawlerRepository.CheckAddHostName(hostName);

        _procData.AddHost(host);

        return host;
    }

    private ExtensionModel TrySaveExtension(ICrawlerRepository crawlerRepository, string extensionName)
    {
        var extension = _procData.GetExtensionByName(extensionName);

        if (extension != null)
            return extension;

        extension = crawlerRepository.CheckAddExtensionName(extensionName);

        _procData.AddExtension(extension);

        return extension;
    }

    private SchemeModel TrySaveScheme(ICrawlerRepository crawlerRepository, string schemeName)
    {
        var scheme = _procData.GetSchemeByName(schemeName);

        if (scheme != null)
            return scheme;

        scheme = crawlerRepository.CheckAddSchemeName(schemeName);

        _procData.AddScheme(scheme);

        return scheme;
    }

    private void SaveChangesAndReduceCache(ICrawlerRepository crawlerRepository)
    {
        StShared.ConsoleWriteInformationLine(_logger, true, $"[{DateTime.Now}] Save Changes");

        crawlerRepository.SaveChangesWithTransaction();

        StShared.ConsoleWriteInformationLine(_logger, true, $"[{DateTime.Now}] CountStatistics");

        CountStatistics(crawlerRepository);
    }

    private void ProcessPage(ICrawlerRepository crawlerRepository, UrlModel urlForProcess, BatchPart batchPart)
    {
        try
        {
            var uri = new Uri(urlForProcess.UrlName);

            var startedAt = DateTime.Now;
            _consoleFormatter.WriteInSameLine("Downloading", uri.ToString());


            HttpStatusCode statusCode;
            DateTime? lastModifiedDate;
            string? content;

            if (urlForProcess.IsSiteMap && string.Equals(urlForProcess.ExtensionNavigation.ExtName, ".gz",
                    StringComparison.CurrentCultureIgnoreCase))
                //მოიქაჩოს მისამართის მიხედვით Gz კონტენტი გახსნით
                (statusCode, lastModifiedDate, content) = GetSiteMapGzFileContent(uri);
            else
                //მოიქაჩოს მისამართის მიხედვით კონტენტი
                (statusCode, lastModifiedDate, content) = GetOnePageContent(uri);

            if (content == null)
            {
                //დავადასტუროთ, რომ ამ გვერდის გაანალიზება ვერ მოხდა.
                crawlerRepository.CreateContentAnalysisRecord(batchPart.BpId, urlForProcess.UrlId, statusCode,
                    lastModifiedDate);

                //StShared.WriteWarningLine($"Page is not Loaded: {uri}", true);
                _consoleFormatter.WriteInSameLine("Page is not Loaded", uri.ToString());
                _consoleFormatter.UseCurrentLine();

                return;
            }

            //urlForProcess.LastDownloaded = DateTime.Now;
            //urlForProcess.DownloadTryCount++;
            //_repository.UpdateUrlData(urlForProcess);

            //გაანალიზდეს კონტენტი კონტენტის ტიპის მიხედვით
            _consoleFormatter.WriteInSameLine("Analyze content of", uri.ToString());

            //robots.txt, sitemap, html
            if (uri.LocalPath == "/robots.txt")
                //გავაანალიზოთ როგორც robots.txt
                AnalyzeAsRobotsText(crawlerRepository, content, urlForProcess.UrlId, batchPart.BpId,
                    urlForProcess.SchemeId, urlForProcess.HostId);
            else if (urlForProcess.IsSiteMap)
                //გავაანალიზოთ როგორც SiteMap
                AnalyzeAsSiteMap(crawlerRepository, content, urlForProcess.UrlId, batchPart.BpId);
            else
                //გავაანალიზოთ როგორც Html
                AnalyzeAsHtml(crawlerRepository, content, urlForProcess, batchPart);

            //გაანალიზების შედეგად ნაპოვნი მისამართები დარეგისტრირდეს ბაზაში TrySaveUrl მეთოდის გამოყენებით

            //გაანალიზების შედეგად ნაპოვნი ქართული სიტყვები დარეგისტრირდეს ბაზაში

            //ასევე უნდა დარეგისტრირდეს სიტყვისა და მისამართის თანაკვეთა (ანუ სადაც ვიპოვეთ ეს სიტყვა)1

            _consoleFormatter.WriteInSameLine("Copy Graph", uri.ToString());

            var urlGraphDeDuplicator = new UrlGraphDeDuplicator(crawlerRepository);
            urlGraphDeDuplicator.CopyToRepository();

            //დავადასტუროთ, რომ ამ გვერდის გაანალიზება მოხდა.
            crawlerRepository.CreateContentAnalysisRecord(batchPart.BpId, urlForProcess.UrlId, statusCode,
                lastModifiedDate);

            _consoleFormatter.WriteInSameLine("Finished",
                $"{uri} ({DateTime.Now.MillisecondsDifference(startedAt)}ms)");
        }
        catch (Exception e)
        {
            StShared.WriteErrorLine($"Error when working on {urlForProcess.UrlName}", true);
            StShared.WriteException(e, true);
        }
    }

    public bool DoOnePage(string strUrl)
    {
        var crawlerRepository = _crawlerRepositoryCreatorFactory.GetCrawlerRepository();
        _procData = new ProcData();

        var urlData = GetUrlData(crawlerRepository, strUrl);
        if (urlData == null)
        {
            StShared.WriteErrorLine($"Cannot prepare data for uri {strUrl}", true);
            return false;
        }

        var contentAnalysis = urlData.Url is not null
            ? crawlerRepository.GetContentAnalysis(_batchPart.BpId, urlData.Url.UrlId)
            : null;
        if (contentAnalysis != null)
        {
            if (!Inputer.InputBool(
                    $"The page {strUrl} already analyzed. Do you wont to delete Content data for reanalyze", true,
                    false))
                return false;
            crawlerRepository.DeleteContentAnalysis(contentAnalysis);
        }

        SaveUrlAndProcessOnePage(crawlerRepository, urlData.CheckedUri);

        return true;
    }
}