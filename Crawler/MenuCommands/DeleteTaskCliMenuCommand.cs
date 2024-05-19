using CliMenu;
using DoCrawler.Models;
using LibDataInput;
using LibParameters;
using SystemToolsShared;

namespace Crawler.MenuCommands;

public sealed class DeleteTaskCliMenuCommand : CliMenuCommand
{
    private readonly ParametersManager _parametersManager;
    private readonly string _taskName;

    // ReSharper disable once ConvertToPrimaryConstructor
    public DeleteTaskCliMenuCommand(ParametersManager parametersManager, string taskName) : base("Delete Task", EMenuAction.LevelUp)
    {
        _parametersManager = parametersManager;
        _taskName = taskName;
    }

    protected override bool RunBody()
    {
        var parameters = (CrawlerParameters)_parametersManager.Parameters;

        var tasks = parameters.Tasks;

        if (!tasks.ContainsKey(_taskName))
        {
            StShared.WriteErrorLine($"Task {_taskName} not found", true);
            return false;
        }

        if (!Inputer.InputBool($"This will Delete  Task {_taskName}. are you sure?", false, false))
            return false;

        tasks.Remove(_taskName);
        _parametersManager.Save(parameters, $"Task {_taskName} deleted.");

        return true;
    }
}