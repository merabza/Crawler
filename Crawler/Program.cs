using System;
using System.Net.Http;
using CliParameters;
using Crawler;
using DoCrawler.Models;
using LibParameters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using SystemToolsShared;

ILogger<Program>? logger = null;
try
{
    Console.WriteLine("Loading...");

    const string appName = "Crawler";

    //პროგრამის ატრიბუტების დაყენება 
    ProgramAttributes.Instance.AppName = appName;

    var argParser = new ArgumentsParser<CrawlerParameters>(args, appName, null);
    switch (argParser.Analysis())
    {
        case EParseResult.Ok: break;
        case EParseResult.Usage: return 1;
        case EParseResult.Error: return 2;
        default: throw new ArgumentOutOfRangeException();
    }

    var par = (CrawlerParameters?)argParser.Par;
    if (par is null)
    {
        StShared.WriteErrorLine("CrawlerParameters is null", true);
        return 3;
    }

    var parametersFileName = argParser.ParametersFileName;
    var servicesCreator = new CrawlerServicesCreator(par);
    // ReSharper disable once using
    using var serviceProvider = servicesCreator.CreateServiceProvider(LogEventLevel.Error);

    if (serviceProvider == null)
    {
        Console.WriteLine("Logger not created");
        return 8;
    }

    logger = serviceProvider.GetService<ILogger<Program>>();
    if (logger is null)
    {
        StShared.WriteErrorLine("logger is null", true);
        return 3;
    }

    var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
    if (httpClientFactory is null)
    {
        StShared.WriteErrorLine("httpClientFactory is null", true);
        return 5;
    }

    var crawler = new Crawler.Crawler(logger, httpClientFactory, new ParametersManager(parametersFileName, par),
        serviceProvider);
    return crawler.Run() ? 0 : 1;
}
catch (Exception e)
{
    StShared.WriteException(e, true, logger);
    return 7;
}
finally
{
    Log.CloseAndFlush();
}