using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using AppCliTools.LibDataInput;
using DoCrawler.Models;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared;

namespace Crawler.MenuCommands;

public sealed class DeleteTaskCliMenuCommand : CliMenuCommand
{
    private readonly ParametersManager _parametersManager;
    private readonly string _taskName;

    // ReSharper disable once ConvertToPrimaryConstructor
    public DeleteTaskCliMenuCommand(ParametersManager parametersManager, string taskName) : base("Delete Task",
        EMenuAction.LevelUp)
    {
        _parametersManager = parametersManager;
        _taskName = taskName;
    }

    protected override async ValueTask<bool> RunBody(CancellationToken cancellationToken = default)
    {
        var parameters = (CrawlerParameters)_parametersManager.Parameters;

        Dictionary<string, TaskModel> tasks = parameters.Tasks;

        if (!tasks.ContainsKey(_taskName))
        {
            StShared.WriteErrorLine($"Task {_taskName} not found", true);
            return false;
        }

        if (!Inputer.InputBool($"This will Delete  Task {_taskName}. are you sure?", false, false))
        {
            return false;
        }

        tasks.Remove(_taskName);
        await _parametersManager.Save(parameters, $"Task {_taskName} deleted.", null, cancellationToken);

        return true;
    }
}
