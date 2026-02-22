using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using DoCrawler.Domain;
using DoCrawler.Models;
using DoCrawler.ToolActions;
using LibCrawlerRepositories;
using Microsoft.Extensions.Logging;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared;

namespace Crawler.MenuCommands;

public sealed class TaskCliMenuCommand : CliMenuCommand
{
    private readonly ICrawlerRepositoryCreatorFactory _crawlerRepositoryCreatorFactory;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;
    private readonly IParametersManager _parametersManager;
    private readonly string _taskName;

    // ReSharper disable once ConvertToPrimaryConstructor
    public TaskCliMenuCommand(ILogger logger, IHttpClientFactory httpClientFactory,
        ICrawlerRepositoryCreatorFactory crawlerRepositoryCreatorFactory, IParametersManager parametersManager,
        string taskName) : base("Run this task", EMenuAction.Reload)
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

        //var crawlerRepository = _crawlerRepositoryCreatorFactory.GetCrawlerRepository();

        var par = ParseOnePageParameters.Create(parameters);
        if (par is null)
        {
            StShared.WriteErrorLine("ParseOnePageParameters does not created", true);
            return ValueTask.FromResult(false);
        }

        var crawlerRunnerToolAction = new CrawlerRunnerToolAction(_logger, _httpClientFactory,
            _crawlerRepositoryCreatorFactory, parameters, par, _taskName, task, null);

        var crawlerRunner = new CrawlerRunner(crawlerRunnerToolAction, _logger);
        return ValueTask.FromResult(crawlerRunner.Run());
    }
}
