using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using CrawlerDb.Models;
using DoCrawler.Domain;
using DoCrawler.Models;
using DoCrawler.ToolActions;
using LibCrawlerRepositories;
using Microsoft.Extensions.Logging;
using SystemTools.SystemToolsShared;

namespace Crawler.MenuCommands;

public sealed class BatchTaskCliMenuCommand : CliMenuCommand
{
    private readonly Batch _batch;
    private readonly ICrawlerRepositoryCreatorFactory _crawlerRepositoryCreatorFactory;
    private readonly IHttpClientFactory _httpClientFactory;

    private readonly ILogger _logger;
    private readonly CrawlerParameters _par;

    // ReSharper disable once ConvertToPrimaryConstructor
    public BatchTaskCliMenuCommand(ILogger logger, IHttpClientFactory httpClientFactory,
        ICrawlerRepositoryCreatorFactory crawlerRepositoryCreatorFactory, CrawlerParameters par, Batch batch) : base(
        "Run this batch", EMenuAction.Reload)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _crawlerRepositoryCreatorFactory = crawlerRepositoryCreatorFactory;
        _par = par;
        _batch = batch;
    }

    protected override ValueTask<bool> RunBody(CancellationToken cancellationToken = default)
    {
        var par = ParseOnePageParameters.Create(_par);
        if (par is null)
        {
            StShared.WriteErrorLine("ParseOnePageParameters does not created", true);
            return ValueTask.FromResult(false);
        }

        var crawlerRunnerToolAction = new CrawlerRunnerToolAction(_logger, _httpClientFactory,
            _crawlerRepositoryCreatorFactory, _par, par, Name, _batch);

        var crawlerRunner = new CrawlerRunner(crawlerRunnerToolAction, _logger);
        return ValueTask.FromResult(crawlerRunner.Run());
    }
}
