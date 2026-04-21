using System.Net.Http;
using AppCliTools.CliMenu;
using AppCliTools.CliParameters.CliMenuCommands;
using DoCrawler.Models;
using Microsoft.Extensions.Logging;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared;

namespace Crawler.Menu.CrawlerParametersEdit;

// ReSharper disable once ClassNeverInstantiated.Global
public class CrawlerParametersEditorCliMenuCommandFactoryStrategy : IMenuCommandFactoryStrategy
{
    private readonly IApplication _application;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<CrawlerParametersEditorCliMenuCommandFactoryStrategy> _logger;
    private readonly IParametersManager _parametersManager;

    // ReSharper disable once ConvertToPrimaryConstructor
    public CrawlerParametersEditorCliMenuCommandFactoryStrategy(
        ILogger<CrawlerParametersEditorCliMenuCommandFactoryStrategy> logger, IHttpClientFactory httpClientFactory,
        IParametersManager parametersManager, IApplication application)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _parametersManager = parametersManager;
        _application = application;
    }

    public CliMenuCommand CreateMenuCommand()
    {
        var parameters = (CrawlerParameters)_parametersManager.Parameters;

        var supportToolsParametersEditor = new CrawlerParametersEditor(_application.AppName, parameters, _parametersManager,
            _logger, _httpClientFactory);
        return new ParametersEditorListCliMenuCommand(supportToolsParametersEditor);
    }
}
