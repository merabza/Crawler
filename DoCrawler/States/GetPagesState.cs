using CrawlerDb.Models;
using DoCrawler.Models;
using LibCrawlerRepositories;
using Microsoft.Extensions.Logging;

namespace DoCrawler.States;

public sealed class GetPagesState : State
{
    private readonly BatchPart _batchPart;
    private readonly CrawlerParameters _par;
    private readonly ICrawlerRepository _repository;

    // ReSharper disable once ConvertToPrimaryConstructor
    public GetPagesState(ILogger logger, ICrawlerRepository repository, CrawlerParameters par, BatchPart batchPart) :
        base(logger, "Get Pages")
    {
        _repository = repository;
        _par = par;
        _batchPart = batchPart;
    }

    public bool UrlsLoaded { get; private set; }

    //BackProcessor bp
    public override void Execute()
    {
        UrlsLoaded = GetPages();
    }

    //BackProcessor bp
    //public override void GoNext()
    //{
    //  Processes.Instance.GetPagesStateFinished();
    //}

    //BackProcessor bp
    private bool GetPages()
    {
        Logger.LogInformation("Loading Urls");
        var urls = _repository.GetOnePortionUrls(_batchPart.BpId, _par.LoadPagesMaxCount);
        var urlsCount = urls.Count;
        Logger.LogInformation("Loaded {urlsCount} Urls", urlsCount);
        if (urls.Count > 0)
        {
            Logger.LogInformation("Add urls to Queue");
            foreach (var urlModel in urls)
                ProcData.Instance.UrlsQueue.Enqueue(urlModel);
            return true;
        }

        Logger.LogInformation("Finish Batch Part");
        _repository.FinishBatchPart(_batchPart);
        _repository.SaveChanges();
        if (!_batchPart.BatchNavigation.AutoCreateNextPart)
            return false;

        _repository.TryCreateNewPart(_batchPart.BatchId);
        _repository.SaveChanges();

        return false;
    }
}