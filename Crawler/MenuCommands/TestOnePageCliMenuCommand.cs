using System;
using System.Diagnostics;
using System.Net.Http;
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

public sealed class TestOnePageCliMenuCommand : CliMenuCommand
{
    private readonly ICrawlerRepositoryCreatorFabric _crawlerRepositoryCreatorFabric;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;
    private readonly ParametersManager _parametersManager;
    private readonly string _taskName;

    // ReSharper disable once ConvertToPrimaryConstructor
    public TestOnePageCliMenuCommand(ILogger logger, IHttpClientFactory httpClientFactory,
        ICrawlerRepositoryCreatorFabric crawlerRepositoryCreatorFabric, ParametersManager parametersManager,
        string taskName) : base("Test One Page", EMenuAction.Reload)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _crawlerRepositoryCreatorFabric = crawlerRepositoryCreatorFabric;
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

        var crawlerRepository = _crawlerRepositoryCreatorFabric.GetCrawlerRepository();

        var strUrl = Inputer.InputText("Page for Test", null);
        if (string.IsNullOrWhiteSpace(strUrl))
        {
            StShared.WriteErrorLine("Page for Test is empty", true);
            return false;
        }

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
        var result = crawlerRunner.RunOnePage(strUrl);
        watch.Stop();
        Console.WriteLine("---");
        Console.WriteLine($"Crawler Finished. Time taken: {watch.Elapsed.Seconds} second(s)");
        return result;
    }
}