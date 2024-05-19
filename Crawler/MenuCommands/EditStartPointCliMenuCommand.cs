using CliMenu;
using DoCrawler.Models;
using LibDataInput;
using LibParameters;
using SystemToolsShared;

namespace Crawler.MenuCommands;

public sealed class EditStartPointCliMenuCommand : CliMenuCommand
{
    private readonly ParametersManager _parametersManager;
    private readonly string _startPoint;
    private readonly string _taskName;

    // ReSharper disable once ConvertToPrimaryConstructor
    public EditStartPointCliMenuCommand(ParametersManager parametersManager, string taskName, string startPoint) : base(
        null, EMenuAction.LevelUp, EMenuAction.Reload, taskName)
    {
        _parametersManager = parametersManager;
        _taskName = taskName;
        _startPoint = startPoint;
    }

    protected override bool RunBody()
    {
        var parameters = (CrawlerParameters)_parametersManager.Parameters;

        var task = parameters.GetTask(_taskName);
        if (task == null)
        {
            StShared.WriteErrorLine($"Task with name {_taskName} is not found", true);
            return false;
        }

        if (!task.StartPoints.Contains(_startPoint))
        {
            StShared.WriteErrorLine($"Start Point {_startPoint} in Task {_taskName} is not found", true);
            return false;
        }

        ////ამოცანის სახელის რედაქტირება
        //TextDataInput nameInput = new TextDataInput("change Start Point ", _startPoint);
        //if (!nameInput.DoInput())
        //    return;
        //string newStartPoint = nameInput.Text;

        var newStartPoint = Inputer.InputText("change Start Point ", _startPoint);

        if (string.IsNullOrWhiteSpace(newStartPoint))
            return false;

        if (_startPoint == newStartPoint)
            return false; //თუ ცვლილება მართლაც მოითხოვეს

        if (!task.CheckNewStartPointValid(_startPoint, newStartPoint))
        {
            StShared.WriteErrorLine($"New Start Point {newStartPoint} is not valid", true);
            return false;
        }

        if (!task.RemoveStartPoint(_startPoint))
        {
            StShared.WriteErrorLine(
                $"Cannot change Start Point {_startPoint} to {newStartPoint}, because cannot remove this Start Point",
                true);
            return false;
        }

        if (!task.AddStartPoint(newStartPoint))
        {
            StShared.WriteErrorLine(
                $"Cannot change Start Point {_startPoint} to {newStartPoint}, because cannot add this Start Point",
                true);
            return false;
        }

        _parametersManager.Save(parameters, $" Start Point Changed from {_startPoint} To {newStartPoint}");

        return true;
    }
}