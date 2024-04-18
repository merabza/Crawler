using System;
using CliMenu;
using CliParameters.CliMenuCommands;
using LibDataInput;
using LibParameters;

namespace Crawler.MenuCommands;

public sealed class StartPointSubMenuCommand : CliMenuCommand
{
    private readonly ParametersManager _parametersManager;
    private readonly string _startPoint;
    private readonly string _taskName;

    // ReSharper disable once ConvertToPrimaryConstructor
    public StartPointSubMenuCommand(ParametersManager parametersManager, string taskName, string startPoint) :
        base(taskName)
    {
        _parametersManager = parametersManager;
        _taskName = taskName;
        _startPoint = startPoint;
    }

    protected override void RunAction()
    {
        MenuAction = EMenuAction.LoadSubMenu;
    }

    public override CliMenuSet GetSubmenu()
    {
        var taskSubMenuSet = new CliMenuSet($" Task => {_taskName},  Start Point => {_startPoint}");

        var deleteStartPointCommand =
            new DeleteStartPointCommand(_parametersManager, _taskName, _startPoint);
        taskSubMenuSet.AddMenuItem(deleteStartPointCommand);

        taskSubMenuSet.AddMenuItem(new EditStartPointCommand(_parametersManager, _taskName, _startPoint),
            "Edit Start Point");

        var key = ConsoleKey.Escape.Value().ToLower();
        taskSubMenuSet.AddMenuItem(key, "Exit to Main menu", new ExitToMainMenuCliMenuCommand(null, null), key.Length);

        return taskSubMenuSet;
    }
}