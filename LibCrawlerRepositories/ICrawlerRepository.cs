using System.Collections.Generic;
using System.Net;
using CrawlerDb.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace LibCrawlerRepositories;

public interface ICrawlerRepository
{
    bool NeedSaveChanges();
    int SaveChanges();
    int SaveChangesWithTransaction();
    IDbContextTransaction GetTransaction();

    List<HostModel> GetHostsList();
    HostModel? GetHostByName(string recordKey);
    HostModel UpdateHost(HostModel host);
    HostModel CreateHost(HostModel newHost);
    HostModel DeleteHost(HostModel hostForDelete);

    List<SchemeModel> GetSchemesList();
    SchemeModel? GetSchemeByName(string schemeName);
    SchemeModel UpdateScheme(SchemeModel scheme);
    SchemeModel CreateScheme(SchemeModel newScheme);
    SchemeModel DeleteScheme(SchemeModel schemeForDelete);

    List<Batch> GetBatchesList();
    Batch? GetBatchByName(string batchName);
    Batch UpdateBatch(Batch batch);
    Batch CreateBatch(Batch newBatch);
    Batch DeleteBatch(Batch batchForDelete);

    HostModel CheckAddHostName(string hostName);
    ExtensionModel CheckAddExtensionName(string extensionName);
    SchemeModel CheckAddSchemeName(string schemeName);
    UrlModel? GetUrl(int hostId, int extId, int scmId, int urlHashCode, string strUrl);

    UrlModel AddUrl(string strUrl, int urlHashCode, HostModel host, ExtensionModel extension, SchemeModel scheme,
        bool isSiteMap, bool isAllowed);

    void AddUrlGraph(UrlGraphNode urlGraphNode);
    void AddUrlGraph(int fromUrlPageId, UrlModel gotUrl, int batchId);
    List<string> GetHostStartUrlNamesByBatch(Batch batch);
    void RemoveHostNamesByBatch(Batch batch, string scheme, string host);
    void AddHostNamesByBatch(Batch batch, string scheme, string host);
    BatchPart? GetOpenedBatchPart(int batchId);
    BatchPart TryCreateNewPart(int batchId);
    void FinishBatchPart(BatchPart batchPart);
    List<UrlModel> GetOnePortionUrls(int batchPartBpId, int maxCount);
    UrlGraphNode? GetUrlGraphEntry(int fromUrlPageId, int urlUrlId, int batchPartId);
    void CreateContentAnalysisRecord(int batchPartBpId, int resultUrlId, HttpStatusCode statusCode);
    TermType CheckAddTermType(string termTypeName);
    Term? GetTerm(string termText);
    Term AddTerm(string termText, TermType termTypeInBase);
    void AddTermByUrl(int termBatchPartId, int termUrlId, Term term, int position);
    TermByUrl? GeTermByUrlEntry(int batchPartId, int urlId, int position);
    void ClearTermsTail(int batchPartId, int urlId, int position);
    void EditTermByUrl(TermByUrl termByUrl, Term term);
    ContentAnalysis? GetContentAnalysis(int batchPartBpId, int urlId);
    void DeleteContentAnalysis(ContentAnalysis contentAnalysis);

    UrlModel UpdateUrlData(UrlModel urlForProcess);

    //void ClearUrlAllows(int hostId);
    //UrlAllowModel AddUrlAllow(int hostId, string patternText, bool isAllowed);
    string? LoadRobotsFromBase(int batchPartId, int schemeId, int hostId);
    void SaveRobotsTxtToBase(int batchPartId, int schemeId, int hostId, string robotsTxt);
    long GetUrlsCount(int batchPartId);
    long GetTermsCount();
    long GetLoadedUrlsCount(int batchPartId);
}