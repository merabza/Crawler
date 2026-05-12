using System.Net.Http;
using AppCliTools.CliMenu;
using AppCliTools.CliParameters.CliMenuCommands;
using Crawler.Cruders;
using DoCrawler.Models;
using LibCrawlerRepositories;
using Microsoft.Extensions.Logging;
using ParametersManagement.LibParameters;

namespace Crawler.Menu.Batches;

// ReSharper disable once ClassNeverInstantiated.Global
public class BatchListCliMenuCommandFactoryStrategy(
    ILogger<BatchListCliMenuCommandFactoryStrategy> logger,
    IHttpClientFactory httpClientFactory,
    IParametersManager parametersManager,
    ICrawlerRepository crawlerRepository) : IMenuCommandFactoryStrategy
{
    public CliMenuCommand CreateMenuCommand()
    {
        var parameters = (CrawlerParameters)parametersManager.Parameters;
        //პაკეტების რედაქტორი
        var batchCruder = new BatchCruder(logger, httpClientFactory, parameters, crawlerRepository);
        //"Batches"
        return new CruderListCliMenuCommand(batchCruder);
    }
}
