using System.Linq;
using System.Net.Http;
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

namespace DoCrawler.ToolActions;

public class CrawlerToolAction : ToolAction
{
    private readonly ICrawlerRepositoryCreatorFactory _crawlerRepositoryCreatorFactory;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ParseOnePageParameters _parseOnePageParameters;
    private readonly string? _taskName;
    protected readonly ILogger CrLogger;
    protected readonly CrawlerParameters Par;
    protected readonly TaskModel? Task;

    protected CrawlerToolAction(ILogger logger, CrawlerParameters par, string taskName, TaskModel? task,
        ICrawlerRepositoryCreatorFactory crawlerRepositoryCreatorFactory, IHttpClientFactory httpClientFactory,
        ParseOnePageParameters parseOnePageParameters) : base(logger, taskName, null, null, true)
    {
        CrLogger = logger;
        Par = par;
        _taskName = taskName;
        Task = task;
        _crawlerRepositoryCreatorFactory = crawlerRepositoryCreatorFactory;
        _httpClientFactory = httpClientFactory;
        _parseOnePageParameters = parseOnePageParameters;
    }

    protected BatchPartRunner? CreateBatchPartRunner(BatchPart? batchPart, Batch batch)
    {
        BatchPartRunner? batchPartRunner = null;

        var createNewPart = false;
        if (batchPart == null)
        {
            createNewPart = IsCreateNewPartAllowed(batch);
            if (!createNewPart)
                return null;
        }

        if (createNewPart)
        {
            var crawlerRepository = _crawlerRepositoryCreatorFactory.GetCrawlerRepository();
            batchPart = crawlerRepository.TryCreateNewPart(batch.BatchId);
            crawlerRepository.SaveChanges();
        }

        if (batchPart is not null)
            batchPartRunner = new BatchPartRunner(CrLogger, _httpClientFactory, _crawlerRepositoryCreatorFactory, Par,
                _parseOnePageParameters, batchPart);

        if (batchPartRunner is null)
        {
            CrLogger.LogError("batchPartRunner is null");
            return null;
        }

        if (createNewPart)
            batchPartRunner.InitBachPart(Task?.StartPoints ?? [], batch);
        return batchPartRunner;
    }

    private Batch? GetBatchByTaskName(ICrawlerRepository crawlerRepository)
    {
        if (_taskName == null || Task == null || Task.StartPoints.Count == 0)
        {
            CrLogger.LogError("Not enough Information About Task");
            return null;
        }

        var newBatchName = _taskName.Truncate(BatchConfiguration.BatchNameLength);
        if (newBatchName is null)
        {
            CrLogger.LogError("Invalid task name for new batch");
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
        foreach (var myUri in Task.StartPoints.Select(UriFactory.GetUri).Where(myUri => myUri != null))
            crawlerRepository.AddHostNamesByBatch(batch, myUri!.Scheme, myUri.Host);

        crawlerRepository.SaveChanges();

        var batchName = batch.BatchName;
        CrLogger.LogInformation("Crawling for batch {batchName}", batchName);

        return batch;
    }

    protected static bool IsCreateNewPartAllowed(Batch batch)
    {
        return batch.AutoCreateNextPart ||
               Inputer.InputBool($"Opened part not found for bath {batch.BatchName}, Create new?", true, false);
    }

    protected (Batch?, BatchPart?) PrepareBatchPart(ICrawlerRepositoryCreatorFactory crawlerRepositoryCreatorFactory,
        Batch? startBatch = null)
    {
        var par = ParseOnePageParameters.Create(Par);
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