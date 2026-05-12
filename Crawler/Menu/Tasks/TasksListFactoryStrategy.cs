using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using AppCliTools.CliMenu;
using Crawler.MenuCommands;
using DoCrawler.Models;
using LibCrawlerRepositories;
using Microsoft.Extensions.Logging;
using ParametersManagement.LibParameters;

namespace Crawler.Menu.Tasks;

// ReSharper disable once ClassNeverInstantiated.Global
public class TasksListFactoryStrategy : IMenuCommandListFactoryStrategy
{
    private readonly ICrawlerRepository _crawlerRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<TasksListFactoryStrategy> _logger;
    private readonly IParametersManager _parametersManager;

    public TasksListFactoryStrategy(ILogger<TasksListFactoryStrategy> logger, IHttpClientFactory httpClientFactory,
        IParametersManager parametersManager, ICrawlerRepository crawlerRepository)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _crawlerRepository = crawlerRepository;
        _parametersManager = parametersManager;
    }

    public List<CliMenuCommand> CreateMenuCommandsList()
    {
        var parameters = (CrawlerParameters)_parametersManager.Parameters;

        return parameters.Tasks.OrderBy(o => o.Key)
            .Select(kvp => new TaskSubMenuCliMenuCommand(_logger, _httpClientFactory, _parametersManager,
                _crawlerRepository, kvp.Key)).Cast<CliMenuCommand>().ToList();
    }
}
