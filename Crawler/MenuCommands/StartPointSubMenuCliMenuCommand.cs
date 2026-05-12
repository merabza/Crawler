using System;
using System.Globalization;
using AppCliTools.CliMenu;
using AppCliTools.CliMenu.CliMenuCommands;
using AppCliTools.LibDataInput;
using ParametersManagement.LibParameters;

namespace Crawler.MenuCommands;

public sealed class StartPointSubMenuCliMenuCommand : CliMenuCommand
{
    private readonly IParametersManager _parametersManager;
    private readonly string _startPoint;
    private readonly string _taskName;

    // ReSharper disable once ConvertToPrimaryConstructor
    public StartPointSubMenuCliMenuCommand(IParametersManager parametersManager, string taskName, string startPoint) :
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

        string key = ConsoleKey.Escape.Value().ToLower(CultureInfo.CurrentCulture);
        taskSubMenuSet.AddMenuItem(key, new ExitToMainMenuCliMenuCommand("Exit to Main menu", null), key.Length);

        return taskSubMenuSet;
    }
}
