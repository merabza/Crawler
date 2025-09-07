using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CrawlerDb.Configurations;
using CrawlerDb.Models;
using DoCrawler.Domain;
using DoCrawler.Models;
using LibCrawlerRepositories;
using LibDataInput;
using LibToolActions;
using Microsoft.Extensions.Logging;
using RobotsTxt;
using SystemToolsShared;

namespace DoCrawler;

public sealed class CrawlerRunner : ToolAction
{
    private readonly Batch? _batch;
    private readonly ICrawlerRepositoryCreatorFactory _crawlerRepositoryCreatorFactory;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;
    private readonly CrawlerParameters _par;
    private readonly ParseOnePageParameters _parseOnePageParameters;
    private readonly TaskModel? _task;
    private readonly string? _taskName;

    public CrawlerRunner(ILogger logger, IHttpClientFactory httpClientFactory,
        ICrawlerRepositoryCreatorFactory crawlerRepositoryCreatorFactory, CrawlerParameters par,
        ParseOnePageParameters parseOnePageParameters, string taskName, TaskModel? task, Batch? batch) : base(logger,
        taskName, null, null, true)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _crawlerRepositoryCreatorFactory = crawlerRepositoryCreatorFactory;
        _par = par;
        _parseOnePageParameters = parseOnePageParameters;
        _taskName = taskName;
        _task = task;
        _batch = batch;
    }

    public CrawlerRunner(ILogger logger, IHttpClientFactory httpClientFactory,
        ICrawlerRepositoryCreatorFactory crawlerRepositoryCreatorFactory, CrawlerParameters par,
        ParseOnePageParameters parseOnePageParameters, string? taskName, Batch? batch) : base(logger,
        taskName ?? string.Empty, null, null, true)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _crawlerRepositoryCreatorFactory = crawlerRepositoryCreatorFactory;
        _par = par;
        _parseOnePageParameters = parseOnePageParameters;
        _taskName = null;
        _task = null;
        _batch = batch;
    }

    protected override ValueTask<bool> RunAction(CancellationToken cancellationToken = default)
    {
        var (batch, batchPart) = PrepareBatchPart(_crawlerRepositoryCreatorFactory, _batch);

        if (batch is null)
            return ValueTask.FromResult(false);

        BatchPartRunner? batchPartRunner = null;
        while (true)
        {
            var createNewPart = false;
            if (batchPart == null)
            {
                createNewPart = IsCreateNewPartAllowed(batch);
                if (!createNewPart)
                    return ValueTask.FromResult(false);
            }

            var crawlerRepository = _crawlerRepositoryCreatorFactory.GetCrawlerRepository();

            if (createNewPart)
            {
                batchPart = crawlerRepository.TryCreateNewPart(batch.BatchId);
                crawlerRepository.SaveChanges();
            }

            if (batchPart is not null)
                batchPartRunner = new BatchPartRunner(_logger, _httpClientFactory, _crawlerRepositoryCreatorFactory,
                    _par, _parseOnePageParameters, batchPart);

            if (batchPartRunner is null)
            {
                _logger.LogError("batchPartRunner is null");
                return ValueTask.FromResult(false);
            }

            if (createNewPart)
                batchPartRunner.InitBachPart(_task?.StartPoints ?? [], batch);

            batchPartRunner.RunBatchPart();

            batchPart = null;
        }
    }

    private Batch? GetBatchByTaskName(ICrawlerRepository crawlerRepository)
    {
        if (_taskName == null || _task == null || _task.StartPoints.Count == 0)
        {
            _logger.LogError("Not enough Information About Task");
            return null;
        }

        var newBatchName = _taskName.Truncate(BatchConfiguration.BatchNameLength);
        if (newBatchName is null)
        {
            _logger.LogError("Invalid task name for new batch");
            return null;
        }

        //მოიძებნოს ბაზაში Batch სახელით _taskName
        //თუ არ არსებობს Batch სახელით _taskName, შეიქმნას IsOpen=false, AutoCreateNextPart=false
        var batch = crawlerRepository.GetBatchByName(_taskName);
        if (batch == null)
        {
            batch = crawlerRepository.CreateBatch(new Batch
            {
                BatchName = newBatchName, IsOpen = false, AutoCreateNextPart = false
            });
            crawlerRepository.SaveChanges();
        }

        //მოხდეს _task.StartPoints-ების განხილვა. თითოეულისათვის:
        //გამოიყოს საწყისი მისამართიდან ჰოსტი და სქემა,
        //შემოწმდეს და თუ არ არსებობს ბაზაში ასეთი ჰოსტი ან სქემა, დარეგისტრირდეს თითოეული.
        //ამ სქემისა და ჰოსტის წყვილისთვის შემოწმდეს არის თუ არა დარეგისტრირებული HostByBatch ცხრილში
        //თუ არ არის დარეგისტრირებული, დარეგისტრირდეს.
        foreach (var myUri in _task.StartPoints.Select(UriFactory.GetUri).Where(myUri => myUri != null))
            crawlerRepository.AddHostNamesByBatch(batch, myUri!.Scheme, myUri.Host);

        crawlerRepository.SaveChanges();

        var batchName = batch.BatchName;
        _logger.LogInformation("Crawling for batch {batchName}", batchName);

        return batch;
    }

    public bool RunOnePage(string strUrl)
    {
        try
        {
            var (batch, batchPart) = PrepareBatchPart(_crawlerRepositoryCreatorFactory);

            if (batch is null)
                return false;

            var createNewPart = false;
            if (batchPart == null)
            {
                createNewPart = IsCreateNewPartAllowed(batch);
                if (!createNewPart)
                    return false;
            }

            var crawlerRepository = _crawlerRepositoryCreatorFactory.GetCrawlerRepository();
            if (createNewPart)
            {
                batchPart = crawlerRepository.TryCreateNewPart(batch.BatchId);
                crawlerRepository.SaveChanges();
            }

            BatchPartRunner? batchPartRunner = null;
            if (batchPart is not null)
                batchPartRunner = new BatchPartRunner(_logger, _httpClientFactory, _crawlerRepositoryCreatorFactory,
                    _par, _parseOnePageParameters, batchPart);

            if (batchPartRunner is null)
            {
                _logger.LogError("batchPartRunner is null");
                return false;
            }

            if (createNewPart)
                batchPartRunner.InitBachPart(_task?.StartPoints ?? [], batch);

            if (!batchPartRunner.DoOnePage(crawlerRepository, strUrl))
                return false;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred executing {0} for {1}", nameof(RunOnePage), strUrl);
            throw;
        }

        return true;
    }

    private static bool IsCreateNewPartAllowed(Batch batch)
    {
        if (batch.AutoCreateNextPart)
            return true;

        return Inputer.InputBool($"Opened part not found for bath {batch.BatchName}, Create new?", true, false);
    }

    private (Batch?, BatchPart?) PrepareBatchPart(ICrawlerRepositoryCreatorFactory crawlerRepositoryCreatorFactory,
        Batch? startBatch = null)
    {
        var par = ParseOnePageParameters.Create(_par);
        if (par is null)
        {
            StShared.WriteErrorLine("ParseOnePageParameters does not created", true);
            return (null, null);
        }

        var repository = crawlerRepositoryCreatorFactory.GetCrawlerRepository();
        var batch = startBatch ?? GetBatchByTaskName(repository);

        if (batch is not null)
            return (batch, repository.GetOpenedBatchPart(batch.BatchId));

        StShared.WriteErrorLine("batch is null", true);
        return (null, null);
    }
}