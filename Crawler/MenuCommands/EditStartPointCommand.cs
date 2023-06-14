using System;
using CliMenu;
using DoCrawler.Models;
using LibDataInput;
using LibParameters;
using SystemToolsShared;

namespace Crawler.MenuCommands;

public sealed class EditStartPointCommand : CliMenuCommand
{
    private readonly ParametersManager _parametersManager;
    private readonly string _startPoint;
    private readonly string _taskName;

    public EditStartPointCommand(ParametersManager parametersManager, string taskName, string startPoint)
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

            ////ამოცანის სახელის რედაქტირება
            //TextDataInput nameInput = new TextDataInput("change Start Point ", _startPoint);
            //if (!nameInput.DoInput())
            //    return;
            //string newStartPoint = nameInput.Text;

            var newStartPoint = Inputer.InputText("change Start Point ", _startPoint);

            if (string.IsNullOrWhiteSpace(newStartPoint))
                return;

            if (_startPoint == newStartPoint)
                return; //თუ ცვლილება მართლაც მოითხოვეს

            if (!task.CheckNewStartPointValid(_startPoint, newStartPoint))
            {
                StShared.WriteErrorLine($"New Start Point {newStartPoint} is not valid", true);
                return;
            }

            if (!task.RemoveStartPoint(_startPoint))
            {
                StShared.WriteErrorLine(
                    $"Cannot change Start Point {_startPoint} to {newStartPoint}, because cannot remove this Start Point",
                    true);
                return;
            }

            if (!task.AddStartPoint(newStartPoint))
            {
                StShared.WriteErrorLine(
                    $"Cannot change Start Point {_startPoint} to {newStartPoint}, because cannot add this Start Point",
                    true);
                return;
            }

            _parametersManager.Save(parameters, $" Start Point Changed from {_startPoint} To {newStartPoint}");

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