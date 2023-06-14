using System;
using System.Diagnostics;
using CliMenu;
using CrawlerDb.Models;
using DoCrawler;
using DoCrawler.Domain;
using DoCrawler.Models;
using LibCrawlerRepositories;
using Microsoft.Extensions.Logging;
using SystemToolsShared;

namespace Crawler.MenuCommands;

public sealed class BatchTaskCommand : CliMenuCommand
{
    private readonly Batch _batch;
    private readonly ICrawlerRepositoryCreatorFabric _crawlerRepositoryCreatorFabric;

    private readonly ILogger _logger;
    private readonly CrawlerParameters _par;

    public BatchTaskCommand(ILogger logger, ICrawlerRepositoryCreatorFabric crawlerRepositoryCreatorFabric,
        CrawlerParameters par, Batch batch)
    {
        _logger = logger;
        _crawlerRepositoryCreatorFabric = crawlerRepositoryCreatorFabric;
        _par = par;
        _batch = batch;
    }

    protected override void RunAction()
    {
        MenuAction = EMenuAction.Reload;
        //CrawlerParameters parameters = (CrawlerParameters)_parametersManager.Parameters;
        //TaskModel task = parameters.GetTask(_taskName);
        //if (task == null)
        //{
        //  StShared.ConsoleWriteErrorLine($" ask with name {_taskName} is not found");
        //  return false;
        //}

        var crawlerRepository = _crawlerRepositoryCreatorFabric.GetCrawlerRepository();

        var par = ParseOnePageParameters.Create(_par);
        if (par is null)
        {
            StShared.WriteErrorLine("ParseOnePageParameters does not created", true);
            return;
        }

        CrawlerRunner crawlerRunner = new(_logger, crawlerRepository, _par, par);

        //დავინიშნოთ დრო
        var watch = Stopwatch.StartNew();
        Console.WriteLine("Crawler is running...");
        Console.WriteLine("---");
        crawlerRunner.Run(_batch);
        watch.Stop();
        Console.WriteLine("---");
        Console.WriteLine($"Crawler Finished. Time taken: {watch.Elapsed.Seconds} second(s)");
        StShared.Pause();
    }
}