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
    private readonly string _strUrName;

    public OnePageCrawlerRunnerToolAction(ILogger logger, IHttpClientFactory httpClientFactory,
        ICrawlerRepository crawlerRepository, CrawlerParameters par, ParseOnePageParameters parseOnePageParameters,
        string taskName, TaskModel? task, string strUrName) : base(logger, par, taskName, task, crawlerRepository,
        httpClientFactory, parseOnePageParameters)
    {
        _strUrName = strUrName;
    }

    protected override async ValueTask<bool> RunAction(CancellationToken cancellationToken = default)
    {
        //1. start
        (Batch? batch, BatchPart? batchPart) = PrepareBatchPart();

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
