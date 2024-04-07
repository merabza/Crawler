using System;
using System.Diagnostics;
using CliMenu;
using DoCrawler;
using DoCrawler.Domain;
using DoCrawler.Models;
using LibCrawlerRepositories;
using LibDataInput;
using LibParameters;
using Microsoft.Extensions.Logging;
using SystemToolsShared;

namespace Crawler.MenuCommands;

public sealed class TestOnePageCommand : CliMenuCommand
{
    private readonly ICrawlerRepositoryCreatorFabric _crawlerRepositoryCreatorFabric;

    private readonly ILogger _logger;

    private readonly ParametersManager _parametersManager;
    private readonly string _taskName;

    // ReSharper disable once ConvertToPrimaryConstructor
    public TestOnePageCommand(ILogger logger, ICrawlerRepositoryCreatorFabric crawlerRepositoryCreatorFabric,
        ParametersManager parametersManager, string taskName)
    {
        _logger = logger;
        _crawlerRepositoryCreatorFabric = crawlerRepositoryCreatorFabric;
        _parametersManager = parametersManager;
        _taskName = taskName;
    }

    protected override void RunAction()
    {
        try
        {
            MenuAction = EMenuAction.Reload;
            var parameters = (CrawlerParameters)_parametersManager.Parameters;
            var task = parameters.GetTask(_taskName);
            if (task == null)
            {
                StShared.WriteErrorLine($"Task with name {_taskName} is not found", true);
                return;
            }

            var crawlerRepository = _crawlerRepositoryCreatorFabric.GetCrawlerRepository();

            var strUrl = Inputer.InputText("Page for Test", null);
            if (string.IsNullOrWhiteSpace(strUrl))
            {
                StShared.WriteErrorLine("Page for Test is empty", true);
                return;
            }

            var par = ParseOnePageParameters.Create(parameters);
            if (par is null)
            {
                StShared.WriteErrorLine("ParseOnePageParameters does not created", true);
                return;
            }

            CrawlerRunner crawlerRunner = new(_logger, crawlerRepository, parameters, par, _taskName, task);

            //დავინიშნოთ დრო
            var watch = Stopwatch.StartNew();
            Console.WriteLine("Crawler is running...");
            Console.WriteLine("---");
            crawlerRunner.RunOnePage(strUrl);
            watch.Stop();
            Console.WriteLine("---");
            Console.WriteLine($"Crawler Finished. Time taken: {watch.Elapsed.Seconds} second(s)");
            StShared.Pause();
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
            StShared.Pause();
        }
    }
}