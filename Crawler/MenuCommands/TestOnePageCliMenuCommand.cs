using System.Net.Http;
using CliMenu;
using DoCrawler.Domain;
using DoCrawler.Models;
using DoCrawler.ToolActions;
using LibCrawlerRepositories;
using LibDataInput;
using LibParameters;
using Microsoft.Extensions.Logging;

namespace Crawler.MenuCommands;

public sealed class TestOnePageCliMenuCommand : CliMenuCommand
{
    private readonly ICrawlerRepositoryCreatorFactory _crawlerRepositoryCreatorFactory;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;
    private readonly ParametersManager _parametersManager;
    private readonly string _taskName;

    // ReSharper disable once ConvertToPrimaryConstructor
    public TestOnePageCliMenuCommand(ILogger logger, IHttpClientFactory httpClientFactory,
        ICrawlerRepositoryCreatorFactory crawlerRepositoryCreatorFactory, ParametersManager parametersManager,
        string taskName) : base("Test One Page", EMenuAction.Reload)
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

        var crawlerRunnerToolAction = new OnePageCrawlerRunnerToolAction(_logger, _httpClientFactory,
            _crawlerRepositoryCreatorFactory, parameters, par, _taskName, task, strUrl);

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
