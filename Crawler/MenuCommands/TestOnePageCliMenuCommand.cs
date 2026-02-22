using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using AppCliTools.LibDataInput;
using DoCrawler.Domain;
using DoCrawler.Models;
using DoCrawler.ToolActions;
using LibCrawlerRepositories;
using Microsoft.Extensions.Logging;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared;

namespace Crawler.MenuCommands;

public sealed class TestOnePageCliMenuCommand : CliMenuCommand
{
    private readonly ICrawlerRepositoryCreatorFactory _crawlerRepositoryCreatorFactory;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;
    private readonly ParametersManager _parametersManager;
    private readonly string _taskName;

    // ReSharper disable once ConvertToPrimaryConstructor
    public TestOnePageCliMenuCommand(ILogger logger, IHttpClientFactory httpClientFactory,
        ICrawlerRepositoryCreatorFactory crawlerRepositoryCreatorFactory, ParametersManager parametersManager,
        string taskName) : base("Test One Page", EMenuAction.Reload)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _crawlerRepositoryCreatorFactory = crawlerRepositoryCreatorFactory;
        _parametersManager = parametersManager;
        _taskName = taskName;
    }

    protected override ValueTask<bool> RunBody(CancellationToken cancellationToken = default)
    {
        var parameters = (CrawlerParameters)_parametersManager.Parameters;
        TaskModel? task = parameters.GetTask(_taskName);
        if (task == null)
        {
            StShared.WriteErrorLine($"Task with name {_taskName} is not found", true);
            return ValueTask.FromResult(false);
        }

        string? strUrl = Inputer.InputText("Page for Test", null);
        if (string.IsNullOrWhiteSpace(strUrl))
        {
            StShared.WriteErrorLine("Page for Test is empty", true);
            return ValueTask.FromResult(false);
        }

        var par = ParseOnePageParameters.Create(parameters);
        if (par is null)
        {
            StShared.WriteErrorLine("ParseOnePageParameters does not created", true);
            return ValueTask.FromResult(false);
        }

        var crawlerRunnerToolAction = new OnePageCrawlerRunnerToolAction(_logger, _httpClientFactory,
            _crawlerRepositoryCreatorFactory, parameters, par, _taskName, task, strUrl);

        var crawlerRunner = new CrawlerRunner(crawlerRunnerToolAction, _logger);
        return ValueTask.FromResult(crawlerRunner.Run());
    }
}
