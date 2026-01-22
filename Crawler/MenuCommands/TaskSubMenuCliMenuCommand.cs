using System;
using System.Linq;
using System.Net.Http;
using AppCliTools.CliMenu;
using AppCliTools.CliParameters.CliMenuCommands;
using AppCliTools.LibDataInput;
using DoCrawler.Models;
using LibCrawlerRepositories;
using Microsoft.Extensions.Logging;
using ParametersManagement.LibParameters;

namespace Crawler.MenuCommands;

public sealed class TaskSubMenuCliMenuCommand : CliMenuCommand
{
    private readonly ICrawlerRepositoryCreatorFactory _crawlerRepositoryCreatorFactory;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;
    private readonly ParametersManager _parametersManager;

    // ReSharper disable once ConvertToPrimaryConstructor
    public TaskSubMenuCliMenuCommand(ILogger logger, IHttpClientFactory httpClientFactory,
        ParametersManager parametersManager, ICrawlerRepositoryCreatorFactory crawlerRepositoryCreatorFactory,
        string taskName) : base(taskName, EMenuAction.LoadSubMenu)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _parametersManager = parametersManager;
        _crawlerRepositoryCreatorFactory = crawlerRepositoryCreatorFactory;
    }

    public override CliMenuSet GetSubMenu()
    {
        var taskSubMenuSet = new CliMenuSet($" Task => {Name}");

        var deleteTaskCommand = new DeleteTaskCliMenuCommand(_parametersManager, Name);
        taskSubMenuSet.AddMenuItem(deleteTaskCommand);

        taskSubMenuSet.AddMenuItem(new EditTaskNameCliMenuCommand(_parametersManager, Name));

        taskSubMenuSet.AddMenuItem(new TaskCliMenuCommand(_logger, _httpClientFactory, _crawlerRepositoryCreatorFactory,
            _parametersManager, Name));

        taskSubMenuSet.AddMenuItem(new TestOnePageCliMenuCommand(_logger, _httpClientFactory,
            _crawlerRepositoryCreatorFactory, _parametersManager, Name));

        var parameters = (CrawlerParameters)_parametersManager.Parameters;

        TaskModel? task = parameters.GetTask(Name);
        var newStartPointCommand = new NewStartPointCliMenuCommand(_parametersManager, Name);
        taskSubMenuSet.AddMenuItem(newStartPointCommand);

        if (task?.StartPoints != null)
        {
            foreach (string startPoint in task.StartPoints.OrderBy(o => o))
            {
                taskSubMenuSet.AddMenuItem(new StartPointSubMenuCliMenuCommand(_parametersManager, Name, startPoint));
            }
        }

        string key = ConsoleKey.Escape.Value().ToUpperInvariant();
        taskSubMenuSet.AddMenuItem(key, new ExitToMainMenuCliMenuCommand("Exit to Main menu", null), key.Length);

        return taskSubMenuSet;
    }
}
