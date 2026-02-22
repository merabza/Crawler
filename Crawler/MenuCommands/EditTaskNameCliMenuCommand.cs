using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using AppCliTools.LibDataInput;
using DoCrawler.Models;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared;

namespace Crawler.MenuCommands;

public sealed class EditTaskNameCliMenuCommand : CliMenuCommand
{
    private readonly ParametersManager _parametersManager;
    private readonly string _taskName;

    // ReSharper disable once ConvertToPrimaryConstructor
    public EditTaskNameCliMenuCommand(ParametersManager parametersManager, string taskName) : base("Edit task Name",
        EMenuAction.LevelUp, EMenuAction.Reload, taskName)
    {
        _parametersManager = parametersManager;
        _taskName = taskName;
    }

    protected override async ValueTask<bool> RunBody(CancellationToken cancellationToken = default)
    {
        var parameters = (CrawlerParameters)_parametersManager.Parameters;

        TaskModel? task = parameters.GetTask(_taskName);
        if (task == null)
        {
            StShared.WriteErrorLine($"Task with name {_taskName} is not found", true);
            return false;
        }

        //ამოცანის სახელის რედაქტირება
        string? newTaskName = Inputer.InputText("change  Task Name ", _taskName);
        if (string.IsNullOrWhiteSpace(newTaskName))
        {
            return false;
        }

        if (_taskName == newTaskName)
        {
            return false; //თუ ცვლილება მართლაც მოითხოვეს
        }

        if (!parameters.CheckNewTaskNameValid(_taskName, newTaskName))
        {
            StShared.WriteErrorLine($"New Name For Task {newTaskName} is not valid", true);
            return false;
        }

        if (!parameters.RemoveTask(_taskName))
        {
            StShared.WriteErrorLine(
                $"Cannot change  Task with name {_taskName} to {newTaskName}, because cannot remove this  task", true);
            return false;
        }

        if (!parameters.AddTask(newTaskName, task))
        {
            StShared.WriteErrorLine(
                $"Cannot change  Task with name {_taskName} to {newTaskName}, because cannot add this  task", true);
            return false;
        }

        await _parametersManager.Save(parameters, $" Task Renamed from {_taskName} To {newTaskName}");

        return true;
    }

    protected override string GetStatus()
    {
        return _taskName;
    }
}
