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
public class TasksListFactoryStrategy(
    ILogger<TasksListFactoryStrategy> logger,
    IHttpClientFactory httpClientFactory,
    IParametersManager parametersManager,
    ICrawlerRepository crawlerRepository) : IMenuCommandListFactoryStrategy
{
    public List<CliMenuCommand> CreateMenuCommandsList()
    {
        var parameters = (CrawlerParameters)parametersManager.Parameters;

        return parameters.Tasks.OrderBy(o => o.Key)
            .Select(kvp => new TaskSubMenuCliMenuCommand(logger, httpClientFactory, parametersManager,
                crawlerRepository, kvp.Key)).Cast<CliMenuCommand>().ToList();
    }
}
