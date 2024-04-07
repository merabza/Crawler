using System;
using System.Linq;
using CliMenu;
using CliParameters.MenuCommands;
using DoCrawler.Models;
using LibCrawlerRepositories;
using LibDataInput;
using LibParameters;
using Microsoft.Extensions.Logging;

namespace Crawler.MenuCommands;

public sealed class TaskSubMenuCommand : CliMenuCommand
{
    private readonly ICrawlerRepositoryCreatorFabric _crawlerRepositoryCreatorFabric;
    private readonly ILogger _logger;
    private readonly ParametersManager _parametersManager;

    // ReSharper disable once ConvertToPrimaryConstructor
    public TaskSubMenuCommand(ILogger logger, ParametersManager parametersManager,
        ICrawlerRepositoryCreatorFabric crawlerRepositoryCreatorFabric, string taskName) : base(taskName)
    {
        _logger = logger;
        _parametersManager = parametersManager;
        _crawlerRepositoryCreatorFabric = crawlerRepositoryCreatorFabric;
    }

    protected override void RunAction()
    {
        MenuAction = EMenuAction.LoadSubMenu;
    }

    public override CliMenuSet GetSubmenu()
    {
        CliMenuSet taskSubMenuSet = new($" Task => {Name}");

        if (Name is not null)
        {
            DeleteTaskCommand deleteTaskCommand = new(_parametersManager, Name);
            taskSubMenuSet.AddMenuItem(deleteTaskCommand);


            taskSubMenuSet.AddMenuItem(new EditTaskNameCommand(_parametersManager, Name), "Edit  task Name");

            taskSubMenuSet.AddMenuItem(
                new TaskCommand(_logger, _crawlerRepositoryCreatorFabric, _parametersManager, Name),
                "Run this task");

            taskSubMenuSet.AddMenuItem(
                new TestOnePageCommand(_logger, _crawlerRepositoryCreatorFabric, _parametersManager, Name),
                "Test One Page");

            var parameters = (CrawlerParameters)_parametersManager.Parameters;

            var task = parameters.GetTask(Name);
            NewStartPointCommand newStartPointCommand = new(_parametersManager, Name);
            taskSubMenuSet.AddMenuItem(newStartPointCommand);


            if (task?.StartPoints != null)
                foreach (var startPoint in task.StartPoints.OrderBy(o => o))
                    taskSubMenuSet.AddMenuItem(new StartPointSubMenuCommand(_parametersManager, Name, startPoint));
        }

        var key = ConsoleKey.Escape.Value().ToLower();
        taskSubMenuSet.AddMenuItem(key, "Exit to Main menu", new ExitToMainMenuCommand(null, null), key.Length);

        return taskSubMenuSet;
    }
}