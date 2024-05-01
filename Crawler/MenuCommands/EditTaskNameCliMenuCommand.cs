using System;
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
        taskName)
    {
        _parametersManager = parametersManager;
        _taskName = taskName;
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

            //ამოცანის სახელის რედაქტირება
            //TextDataInput nameInput = new TextDataInput("change  Task Name ", _taskName);
            //if (!nameInput.DoInput())
            //    return;
            var newTaskName = Inputer.InputText("change  Task Name ", _taskName);
            if (string.IsNullOrWhiteSpace(newTaskName))
                return;

            if (_taskName == newTaskName)
                return; //თუ ცვლილება მართლაც მოითხოვეს

            if (!parameters.CheckNewTaskNameValid(_taskName, newTaskName))
            {
                StShared.WriteErrorLine($"New Name For Task {newTaskName} is not valid", true);
                return;
            }

            if (!parameters.RemoveTask(_taskName))
            {
                StShared.WriteErrorLine(
                    $"Cannot change  Task with name {_taskName} to {newTaskName}, because cannot remove this  task",
                    true);
                return;
            }

            if (!parameters.AddTask(newTaskName, task))
            {
                StShared.WriteErrorLine(
                    $"Cannot change  Task with name {_taskName} to {newTaskName}, because cannot add this  task",
                    true);
                return;
            }

            _parametersManager.Save(parameters, $" Task Renamed from {_taskName} To {newTaskName}");

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

    protected override string GetStatus()
    {
        return _taskName;
    }
}