using System;
using System.Diagnostics;
using System.Threading;
using DoCrawler.ToolActions;
using Microsoft.Extensions.Logging;
using SystemTools.SystemToolsShared;

namespace Crawler;

public sealed class CrawlerRunner
{
    private readonly CrawlerToolAction _crawlerRunner;
    private readonly ILogger _logger;

    public CrawlerRunner(CrawlerToolAction crawlerRunner, ILogger logger)
    {
        _crawlerRunner = crawlerRunner;
        _logger = logger;
    }

    public bool Run()
    {
        //დავინიშნოთ დრო
        var watch = Stopwatch.StartNew();
        Console.WriteLine("Crawler is running...");
        Console.WriteLine("---");

        try
        {
            // ReSharper disable once using
            // ReSharper disable once DisposableConstructor
            using var cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;
            token.ThrowIfCancellationRequested();
            bool result = _crawlerRunner.Run(token).Result;
            return result;
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation was canceled.");
        }
        catch (Exception e)
        {
            string contextualMessage = "Error occurred while running the crawler in CrawlerRunner.Run";
            _logger.LogError(e, "{ContextualMessage}", contextualMessage);
            throw new Exception(contextualMessage, e);
        }
        finally
        {
            watch.Stop();
            Console.WriteLine("---");
            Console.WriteLine($"Crawler Finished. Time taken: {watch.Elapsed.Seconds} second(s)");
            StShared.Pause();
        }

        return false;
    }
}
