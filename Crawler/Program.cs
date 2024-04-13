using System;
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

    //პროგრამის ატრიბუტების დაყენება 
    StatProgramAttr.SetAttr();

    IArgumentsParser argParser = new ArgumentsParser<CrawlerParameters>(args, "Crawler", null);
    switch (argParser.Analysis())
    {
        case EParseResult.Ok: break;
        case EParseResult.Usage: return 1;
        case EParseResult.Error: return 1;
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


    var crawler =
        new Crawler.Crawler(logger, new ParametersManager(parametersFileName, par), serviceProvider);
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