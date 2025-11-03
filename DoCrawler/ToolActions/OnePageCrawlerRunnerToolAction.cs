using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DoCrawler.Domain;
using DoCrawler.Models;
using LibCrawlerRepositories;
using Microsoft.Extensions.Logging;

namespace DoCrawler.ToolActions;

public sealed class OnePageCrawlerRunnerToolAction : CrawlerToolAction
{
    private readonly ICrawlerRepositoryCreatorFactory _crawlerRepositoryCreatorFactory;
    private readonly string _strUrl;

    public OnePageCrawlerRunnerToolAction(ILogger logger, IHttpClientFactory httpClientFactory,
        ICrawlerRepositoryCreatorFactory crawlerRepositoryCreatorFactory, CrawlerParameters par,
        ParseOnePageParameters parseOnePageParameters, string taskName, TaskModel? task, string strUrl) : base(logger,
        par, taskName, task, crawlerRepositoryCreatorFactory, httpClientFactory, parseOnePageParameters)
    {
        _crawlerRepositoryCreatorFactory = crawlerRepositoryCreatorFactory;
        _strUrl = strUrl;
    }

    protected override async ValueTask<bool> RunAction(CancellationToken cancellationToken = default)
    {
        //1. start
        var (batch, batchPart) = PrepareBatchPart(_crawlerRepositoryCreatorFactory);

        if (batch is null)
            return false;
        //1. Finish

        //2. Start
        var batchPartRunner = CreateBatchPartRunner(batchPart, batch);
        //2. Finish
        return batchPartRunner is not null && await batchPartRunner.DoOnePage(_strUrl, cancellationToken);
    }
}