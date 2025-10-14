using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CrawlerDb.Models;
using DoCrawler.Domain;
using DoCrawler.Models;
using LibCrawlerRepositories;
using Microsoft.Extensions.Logging;

namespace DoCrawler.ToolActions;

public sealed class CrawlerRunnerToolAction : CrawlerToolAction
{
    private readonly Batch? _batch;
    private readonly ICrawlerRepositoryCreatorFactory _crawlerRepositoryCreatorFactory;

    public CrawlerRunnerToolAction(ILogger logger, IHttpClientFactory httpClientFactory,
        ICrawlerRepositoryCreatorFactory crawlerRepositoryCreatorFactory, CrawlerParameters par,
        ParseOnePageParameters parseOnePageParameters, string taskName, TaskModel? task, Batch? batch) : base(logger,
        par, taskName, task, crawlerRepositoryCreatorFactory, httpClientFactory, parseOnePageParameters)
    {
        _crawlerRepositoryCreatorFactory = crawlerRepositoryCreatorFactory;
        _batch = batch;
    }

    public CrawlerRunnerToolAction(ILogger logger, IHttpClientFactory httpClientFactory,
        ICrawlerRepositoryCreatorFactory crawlerRepositoryCreatorFactory, CrawlerParameters par,
        ParseOnePageParameters parseOnePageParameters, string taskName, Batch? batch) : base(logger, par, taskName,
        null, crawlerRepositoryCreatorFactory, httpClientFactory, parseOnePageParameters)
    {
        _crawlerRepositoryCreatorFactory = crawlerRepositoryCreatorFactory;
        _batch = batch;
    }

    protected override ValueTask<bool> RunAction(CancellationToken cancellationToken = default)
    {
        //1. start
        var (batch, batchPart) = PrepareBatchPart(_crawlerRepositoryCreatorFactory, _batch);

        if (batch is null)
            return ValueTask.FromResult(false);
        //1. Finish

        while (true)
        {
            //2. Start
            var batchPartRunner = CreateBatchPartRunner(batchPart, batch);
            //2. Finish
            if (batchPartRunner is null)
                return ValueTask.FromResult(false);

            batchPartRunner.InitBachPart(Task?.StartPoints ?? [], batch);

            batchPartRunner.RunBatchPart();

            batchPart = null;
        }
    }
}