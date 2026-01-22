using System.Collections.Generic;
using CrawlerDb.Models;
using DoCrawler.Models;
using LibCrawlerRepositories;
using Microsoft.Extensions.Logging;

namespace DoCrawler.States;

public sealed class GetPagesState // : State
{
    private readonly BatchPart _batchPart;
    private readonly ILogger _logger;
    private readonly CrawlerParameters _par;
    private readonly ICrawlerRepository _repository;

    // ReSharper disable once ConvertToPrimaryConstructor
    public GetPagesState(ILogger logger, ICrawlerRepository repository, CrawlerParameters par, BatchPart batchPart)
        //:
        //base(logger, "Get Pages")
    {
        _logger = logger;
        _repository = repository;
        _par = par;
        _batchPart = batchPart;
    }

    //public bool UrlsLoaded { get; private set; }

    //BackProcessor bp
    //public override void Execute()
    //{
    //    UrlsLoaded = GetPages();
    //}

    //BackProcessor bp
    //public override void GoNext()
    //{
    //  Processes.Instance.GetPagesStateFinished();
    //}

    //BackProcessor bp
    public List<UrlModel> GetPages()
    {
        _logger.LogInformation("Loading Urls");
        var urls = _repository.GetOnePortionUrls(_batchPart.BpId, _par.LoadPagesMaxCount);
        var urlsCount = urls.Count;
        _logger.LogInformation("Loaded {UrlsCount} Urls", urlsCount);
        if (urls.Count > 0)
        {
            _logger.LogInformation("Add urls to Queue");
            //foreach (var urlModel in urls)
            //    ProcData.Instance.UrlsQueue.Enqueue(urlModel);
            return urls;
        }

        _logger.LogInformation("Finish Batch Part");
        _repository.FinishBatchPart(_batchPart);
        _repository.SaveChanges();
        if (!_batchPart.BatchNavigation.AutoCreateNextPart)
        {
            return [];
        }

        _repository.TryCreateNewPart(_batchPart.BatchId);
        _repository.SaveChanges();

        return [];
    }
}
