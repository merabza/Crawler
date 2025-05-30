using System;
using System.Linq;
using System.Net.Http;
using CliMenu;
using CliParameters.CliMenuCommands;
using DoCrawler.Models;
using LibCrawlerRepositories;
using LibDataInput;
using LibParameters;
using Microsoft.Extensions.Logging;

namespace Crawler.MenuCommands;

public sealed class TaskSubMenuCliMenuCommand : CliMenuCommand
{
    private readonly ICrawlerRepositoryCreatorFabric _crawlerRepositoryCreatorFabric;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;
    private readonly ParametersManager _parametersManager;

    // ReSharper disable once ConvertToPrimaryConstructor
    public TaskSubMenuCliMenuCommand(ILogger logger, IHttpClientFactory httpClientFactory,
        ParametersManager parametersManager, ICrawlerRepositoryCreatorFabric crawlerRepositoryCreatorFabric,
        string taskName) : base(taskName, EMenuAction.LoadSubMenu)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _parametersManager = parametersManager;
        _crawlerRepositoryCreatorFabric = crawlerRepositoryCreatorFabric;
    }

    public override CliMenuSet GetSubMenu()
    {
        CliMenuSet taskSubMenuSet = new($" Task => {Name}");

        DeleteTaskCliMenuCommand deleteTaskCommand = new(_parametersManager, Name);
        taskSubMenuSet.AddMenuItem(deleteTaskCommand);

        taskSubMenuSet.AddMenuItem(new EditTaskNameCliMenuCommand(_parametersManager, Name));

        taskSubMenuSet.AddMenuItem(new TaskCliMenuCommand(_logger, _httpClientFactory, _crawlerRepositoryCreatorFabric,
            _parametersManager, Name));

        taskSubMenuSet.AddMenuItem(new TestOnePageCliMenuCommand(_logger, _httpClientFactory,
            _crawlerRepositoryCreatorFabric, _parametersManager, Name));

        var parameters = (CrawlerParameters)_parametersManager.Parameters;

        var task = parameters.GetTask(Name);
        NewStartPointCliMenuCommand newStartPointCommand = new(_parametersManager, Name);
        taskSubMenuSet.AddMenuItem(newStartPointCommand);

        if (task?.StartPoints != null)
            foreach (var startPoint in task.StartPoints.OrderBy(o => o))
                taskSubMenuSet.AddMenuItem(new StartPointSubMenuCliMenuCommand(_parametersManager, Name, startPoint));

        var key = ConsoleKey.Escape.Value().ToLower();
        taskSubMenuSet.AddMenuItem(key, new ExitToMainMenuCliMenuCommand("Exit to Main menu", null), key.Length);

        return taskSubMenuSet;
    }
}