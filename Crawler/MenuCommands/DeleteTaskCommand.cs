﻿using System;
using CliMenu;
using DoCrawler.Models;
using LibDataInput;
using LibParameters;
using SystemToolsShared;

namespace Crawler.MenuCommands;

public sealed class DeleteTaskCommand : CliMenuCommand
{
    private readonly ParametersManager _parametersManager;
    private readonly string _taskName;

    public DeleteTaskCommand(ParametersManager parametersManager, string taskName) : base("Delete Task",
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

            var tasks = parameters.Tasks;

            if (!tasks.ContainsKey(_taskName))
            {
                StShared.WriteErrorLine($"Task {_taskName} not found", true);
                return;
            }

            if (!Inputer.InputBool($"This will Delete  Task {_taskName}. are you sure?", false, false))
                return;

            tasks.Remove(_taskName);
            _parametersManager.Save(parameters, $"Task {_taskName} deleted.");

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