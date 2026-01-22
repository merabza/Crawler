using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CrawlerDb.Models;
using DoCrawler.Domain;
using DoCrawler.Models;
using LibCrawlerRepositories;
using Microsoft.Extensions.Logging;

namespace DoCrawler.ToolActions;

public sealed class OnePageCrawlerRunnerToolAction : CrawlerToolAction
{
    private readonly ICrawlerRepositoryCreatorFactory _crawlerRepositoryCreatorFactory;
    private readonly string _strUrName;

    public OnePageCrawlerRunnerToolAction(ILogger logger, IHttpClientFactory httpClientFactory,
        ICrawlerRepositoryCreatorFactory crawlerRepositoryCreatorFactory, CrawlerParameters par,
        ParseOnePageParameters parseOnePageParameters, string taskName, TaskModel? task, string strUrName) : base(
        logger, par, taskName, task, crawlerRepositoryCreatorFactory, httpClientFactory, parseOnePageParameters)
    {
        _crawlerRepositoryCreatorFactory = crawlerRepositoryCreatorFactory;
        _strUrName = strUrName;
    }

    protected override async ValueTask<bool> RunAction(CancellationToken cancellationToken = default)
    {
        //1. start
        (Batch? batch, BatchPart? batchPart) = PrepareBatchPart(_crawlerRepositoryCreatorFactory);

        if (batch is null)
        {
            return false;
        }
        //1. Finish

        //2. Start
        BatchPartRunner? batchPartRunner = CreateBatchPartRunner(batchPart, batch);
        //2. Finish
        return batchPartRunner is not null && await batchPartRunner.DoOnePage(_strUrName, cancellationToken);
    }
}
