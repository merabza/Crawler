using CliMenu;
using DoCrawler.Models;
using LibDataInput;
using LibParameters;
using SystemToolsShared;

namespace Crawler.MenuCommands;

public sealed class EditTaskNameCliMenuCommand : CliMenuCommand
{
    private readonly ParametersManager _parametersManager;
    private readonly string _taskName;

    // ReSharper disable once ConvertToPrimaryConstructor
    public EditTaskNameCliMenuCommand(ParametersManager parametersManager, string taskName) : base("Edit Task",
        EMenuAction.LevelUp, EMenuAction.Reload,
        taskName)
    {
        _parametersManager = parametersManager;
        _taskName = taskName;
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

        //ამოცანის სახელის რედაქტირება
        var newTaskName = Inputer.InputText("change  Task Name ", _taskName);
        if (string.IsNullOrWhiteSpace(newTaskName))
            return false;

        if (_taskName == newTaskName)
            return false; //თუ ცვლილება მართლაც მოითხოვეს

        if (!parameters.CheckNewTaskNameValid(_taskName, newTaskName))
        {
            StShared.WriteErrorLine($"New Name For Task {newTaskName} is not valid", true);
            return false;
        }

        if (!parameters.RemoveTask(_taskName))
        {
            StShared.WriteErrorLine(
                $"Cannot change  Task with name {_taskName} to {newTaskName}, because cannot remove this  task",
                true);
            return false;
        }

        if (!parameters.AddTask(newTaskName, task))
        {
            StShared.WriteErrorLine(
                $"Cannot change  Task with name {_taskName} to {newTaskName}, because cannot add this  task",
                true);
            return false;
        }

        _parametersManager.Save(parameters, $" Task Renamed from {_taskName} To {newTaskName}");

        return true;
    }

    protected override string GetStatus()
    {
        return _taskName;
    }
}