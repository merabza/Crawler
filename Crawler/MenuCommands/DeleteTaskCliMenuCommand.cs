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
    public DeleteTaskCliMenuCommand(ParametersManager parametersManager, string taskName) : base("Delete Task",
        taskName)
    {
        _parametersManager = parametersManager;
        _taskName = taskName;
    }

    protected override void RunAction()
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
    }
}