using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using AppCliTools.LibDataInput;
using DoCrawler.Models;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared;

namespace Crawler.MenuCommands;

public sealed class DeleteStartPointCliMenuCommand : CliMenuCommand
{
    private readonly ParametersManager _parametersManager;
    private readonly string _startPoint;
    private readonly string _taskName;

    // ReSharper disable once ConvertToPrimaryConstructor
    public DeleteStartPointCliMenuCommand(ParametersManager parametersManager, string taskName, string startPoint) :
        base("Delete Start Point", EMenuAction.LevelUp)
    {
        _parametersManager = parametersManager;
        _taskName = taskName;
        _startPoint = startPoint;
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

        if (!task.StartPoints.Contains(_startPoint))
        {
            StShared.WriteErrorLine($"Start Point {_startPoint} in Task {_taskName} is not found", true);
            return false;
        }

        if (!Inputer.InputBool($"This will Delete Start Point {_startPoint}. are you sure?", false, false))
        {
            return false;
        }

        task.StartPoints.Remove(_startPoint);
        await _parametersManager.Save(parameters, $"Start Point {_startPoint} deleted.", null, cancellationToken);

        return true;
    }
}
