using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using CliMenu;
using DoCrawler;
using DoCrawler.Domain;
using DoCrawler.Models;
using LibCrawlerRepositories;
using LibParameters;
using Microsoft.Extensions.Logging;
using SystemToolsShared;

namespace Crawler.MenuCommands;

public sealed class TaskCliMenuCommand : CliMenuCommand
{
    private readonly ICrawlerRepositoryCreatorFactory _crawlerRepositoryCreatorFactory;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;
    private readonly IParametersManager _parametersManager;
    private readonly string _taskName;

    // ReSharper disable once ConvertToPrimaryConstructor
    public TaskCliMenuCommand(ILogger logger, IHttpClientFactory httpClientFactory,
        ICrawlerRepositoryCreatorFactory crawlerRepositoryCreatorFactory, IParametersManager parametersManager,
        string taskName) : base("Run this task", EMenuAction.Reload)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _crawlerRepositoryCreatorFactory = crawlerRepositoryCreatorFactory;
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

        var crawlerRepository = _crawlerRepositoryCreatorFactory.GetCrawlerRepository();

        var par = ParseOnePageParameters.Create(parameters);
        if (par is null)
        {
            StShared.WriteErrorLine("ParseOnePageParameters does not created", true);
            return false;
        }

        CrawlerRunner crawlerRunner = new(_logger, _httpClientFactory, crawlerRepository, parameters, par, _taskName,
            task, null);

        //დავინიშნოთ დრო
        var watch = Stopwatch.StartNew();
        Console.WriteLine("Crawler is running...");
        Console.WriteLine("---");
        var result = crawlerRunner.Run(CancellationToken.None).Result;
        watch.Stop();
        Console.WriteLine("---");
        Console.WriteLine($"Crawler Finished. Time taken: {watch.Elapsed.Seconds} second(s)");
        return result;
    }
}