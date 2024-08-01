using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using CliMenu;
using CrawlerDb.Models;
using DoCrawler;
using DoCrawler.Domain;
using DoCrawler.Models;
using LibCrawlerRepositories;
using Microsoft.Extensions.Logging;
using SystemToolsShared;

namespace Crawler.MenuCommands;

public sealed class BatchTaskCliMenuCommand : CliMenuCommand
{
    private readonly Batch _batch;
    private readonly ICrawlerRepositoryCreatorFabric _crawlerRepositoryCreatorFabric;
    private readonly IHttpClientFactory _httpClientFactory;

    private readonly ILogger _logger;
    private readonly CrawlerParameters _par;

    // ReSharper disable once ConvertToPrimaryConstructor
    public BatchTaskCliMenuCommand(ILogger logger, IHttpClientFactory httpClientFactory,
        ICrawlerRepositoryCreatorFabric crawlerRepositoryCreatorFabric, CrawlerParameters par, Batch batch) : base(
        "Run this batch", EMenuAction.Reload)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _crawlerRepositoryCreatorFabric = crawlerRepositoryCreatorFabric;
        _par = par;
        _batch = batch;
    }

    protected override bool RunBody()
    {
        var crawlerRepository = _crawlerRepositoryCreatorFabric.GetCrawlerRepository();

        var par = ParseOnePageParameters.Create(_par);
        if (par is null)
        {
            StShared.WriteErrorLine("ParseOnePageParameters does not created", true);
            return false;
        }

        CrawlerRunner crawlerRunner = new(_logger, _httpClientFactory, crawlerRepository, _par, par, Name, _batch);

        //დავინიშნოთ დრო
        var watch = Stopwatch.StartNew();
        Console.WriteLine("Crawler is running...");
        Console.WriteLine("---");
        var result = crawlerRunner.Run(CancellationToken.None).Result;
        watch.Stop();
        Console.WriteLine("---");
        Console.WriteLine($"Crawler Finished. Time taken: {watch.Elapsed.Seconds} second(s)");
        StShared.Pause();

        return result;
    }
}