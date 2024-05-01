using System;
using CliMenu;
using DoCrawler.Models;
using LibDataInput;
using LibParameters;
using SystemToolsShared;

namespace Crawler.MenuCommands;

public sealed class DeleteStartPointCliMenuCommand : CliMenuCommand
{
    private readonly ParametersManager _parametersManager;
    private readonly string _startPoint;
    private readonly string _taskName;

    // ReSharper disable once ConvertToPrimaryConstructor
    public DeleteStartPointCliMenuCommand(ParametersManager parametersManager, string taskName, string startPoint) : base(
        "Delete Start Point", taskName)
    {
        _parametersManager = parametersManager;
        _taskName = taskName;
        _startPoint = startPoint;
    }

    protected override void RunAction()
    {
        try
        {
            var parameters = (CrawlerParameters)_parametersManager.Parameters;

            var task = parameters.GetTask(_taskName);
            if (task == null)
            {
                StShared.WriteErrorLine($"Task with name {_taskName} is not found", true);
                return;
            }

            if (!task.StartPoints.Contains(_startPoint))
            {
                StShared.WriteErrorLine($"Start Point {_startPoint} in Task {_taskName} is not found", true);
                return;
            }

            if (!Inputer.InputBool($"This will Delete Start Point {_startPoint}. are you sure?", false, false))
                return;

            task.StartPoints.Remove(_startPoint);
            _parametersManager.Save(parameters, $"Start Point {_startPoint} deleted.");

            MenuAction = EMenuAction.LevelUp;
            return;
        }
        catch (DataInputEscapeException)
        {
            Console.WriteLine();
            Console.WriteLine("Escape... ");
            StShared.Pause();
        }
        catch (Exception e)
        {
            StShared.WriteException(e, true);
        }

        MenuAction = EMenuAction.Reload;
    }
}