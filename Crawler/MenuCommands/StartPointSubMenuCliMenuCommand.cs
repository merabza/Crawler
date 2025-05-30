using System;
using CliMenu;
using CliParameters.CliMenuCommands;
using LibDataInput;
using LibParameters;

namespace Crawler.MenuCommands;

public sealed class StartPointSubMenuCliMenuCommand : CliMenuCommand
{
    private readonly ParametersManager _parametersManager;
    private readonly string _startPoint;
    private readonly string _taskName;

    // ReSharper disable once ConvertToPrimaryConstructor
    public StartPointSubMenuCliMenuCommand(ParametersManager parametersManager, string taskName, string startPoint) :
        base(taskName, EMenuAction.LoadSubMenu)
    {
        _parametersManager = parametersManager;
        _taskName = taskName;
        _startPoint = startPoint;
    }

    public override CliMenuSet GetSubMenu()
    {
        var taskSubMenuSet = new CliMenuSet($" Task => {_taskName},  Start Point => {_startPoint}");

        var deleteStartPointCommand = new DeleteStartPointCliMenuCommand(_parametersManager, _taskName, _startPoint);
        taskSubMenuSet.AddMenuItem(deleteStartPointCommand);

        taskSubMenuSet.AddMenuItem(new EditStartPointCliMenuCommand(_parametersManager, _taskName, _startPoint));

        var key = ConsoleKey.Escape.Value().ToLower();
        taskSubMenuSet.AddMenuItem(key, new ExitToMainMenuCliMenuCommand("Exit to Main menu", null), key.Length);

        return taskSubMenuSet;
    }
}