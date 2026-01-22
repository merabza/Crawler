using System.Net.Http;
using AppCliTools.CliMenu;
using CrawlerDb.Models;
using DoCrawler.Domain;
using DoCrawler.Models;
using DoCrawler.ToolActions;
using LibCrawlerRepositories;
using Microsoft.Extensions.Logging;
using SystemTools.SystemToolsShared;

namespace Crawler.MenuCommands;

public sealed class BatchTaskCliMenuCommand : CliMenuCommand
{
    private readonly Batch _batch;
    private readonly ICrawlerRepositoryCreatorFactory _crawlerRepositoryCreatorFactory;
    private readonly IHttpClientFactory _httpClientFactory;

    private readonly ILogger _logger;
    private readonly CrawlerParameters _par;

    // ReSharper disable once ConvertToPrimaryConstructor
    public BatchTaskCliMenuCommand(ILogger logger, IHttpClientFactory httpClientFactory,
        ICrawlerRepositoryCreatorFactory crawlerRepositoryCreatorFactory, CrawlerParameters par, Batch batch) : base(
        "Run this batch", EMenuAction.Reload)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _crawlerRepositoryCreatorFactory = crawlerRepositoryCreatorFactory;
        _par = par;
        _batch = batch;
    }

    protected override bool RunBody()
    {
        var par = ParseOnePageParameters.Create(_par);
        if (par is null)
        {
            StShared.WriteErrorLine("ParseOnePageParameters does not created", true);
            return false;
        }

        var crawlerRunnerToolAction = new CrawlerRunnerToolAction(_logger, _httpClientFactory,
            _crawlerRepositoryCreatorFactory, _par, par, Name, _batch);

        var crawlerRunner = new CrawlerRunner(crawlerRunnerToolAction, _logger);
        return crawlerRunner.Run();

        ////დავინიშნოთ დრო
        //var watch = Stopwatch.StartNew();
        //Console.WriteLine("Crawler is running...");
        //Console.WriteLine("---");

        //try
        //{
        //    // ReSharper disable once using
        //    // ReSharper disable once DisposableConstructor
        //    using var cts = new CancellationTokenSource();
        //    var token = cts.Token;
        //    token.ThrowIfCancellationRequested();
        //    var result = crawlerRunner.Run(token).Result;
        //    return result;
        //}
        //catch (OperationCanceledException)
        //{
        //    Console.WriteLine("Operation was canceled.");
        //}
        //catch (Exception e)
        //{
        //    _logger.LogError(e, "Error in DbServerFoldersSetNameFieldEditor.UpdateField");
        //    throw;
        //}
        //finally
        //{
        //    watch.Stop();
        //    Console.WriteLine("---");
        //    Console.WriteLine($"Crawler Finished. Time taken: {watch.Elapsed.Seconds} second(s)");
        //    StShared.Pause();
        //}

        //return false;
    }
}
