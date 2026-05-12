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
public class BatchListCliMenuCommandFactoryStrategy : IMenuCommandFactoryStrategy
{
    private readonly ICrawlerRepository _crawlerRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<BatchListCliMenuCommandFactoryStrategy> _logger;
    private readonly IParametersManager _parametersManager;

    public BatchListCliMenuCommandFactoryStrategy(ILogger<BatchListCliMenuCommandFactoryStrategy> logger,
        IHttpClientFactory httpClientFactory, IParametersManager parametersManager,
        ICrawlerRepository crawlerRepository)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _parametersManager = parametersManager;
        _crawlerRepository = crawlerRepository;
    }

    public CliMenuCommand CreateMenuCommand()
    {
        var parameters = (CrawlerParameters)_parametersManager.Parameters;
        //პაკეტების რედაქტორი
        var batchCruder = new BatchCruder(_logger, _httpClientFactory, parameters, _crawlerRepository);
        //"Batches"
        return new CruderListCliMenuCommand(batchCruder);
    }
}
