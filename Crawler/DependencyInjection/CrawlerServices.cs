using AppCliTools.CliMenu;
using AppCliTools.CliMenu.DependencyInjection;
using AppCliTools.CliParametersDataEdit;
using AppCliTools.CliTools.DependencyInjection;
using AppCliTools.CliTools.Services.MenuBuilder;
using Crawler.Menu.CrawlerParametersEdit;
using Crawler.Menu.Tasks;
using CrawlerDb;
using DoCrawler.Models;
using LibCrawlerRepositories;
using Microsoft.Extensions.DependencyInjection;
using ParametersManagement.LibDatabaseParameters;
using ParametersManagement.LibParameters.DependencyInjection;
using Serilog.Events;
using SystemTools.DependencyInjection;
using SystemTools.SystemToolsShared;

namespace Crawler.DependencyInjection;

public static class CrawlerServices
{
    public static IServiceCollection AddServices(this IServiceCollection services, string appName,
        CrawlerParameters par, string parametersFileName)
    {
        var databaseServerConnections = new DatabaseServerConnections(par.DatabaseServerConnections);

        (EDatabaseProvider? dataProvider, string? connectionString, int commandTimeout) =
            DbConnectionFactory.GetDataProviderConnectionStringCommandTimeOut(par.DatabaseParameters,
                databaseServerConnections);

        if (!string.IsNullOrEmpty(connectionString))
        {
            services.AddContextByProvider<CrawlerDbContext>(dataProvider, connectionString, commandTimeout);
        }

        // @formatter:off
        services
            .AddSerilogLoggerService(LogEventLevel.Information, appName, par.LogFolder)
            .AddHttpClient()
            .AddSingleton<ICrawlerRepositoryCreatorFactory, CrawlerRepositoryCreatorFactory>()
            .AddScoped<ICrawlerRepository, CrawlerRepository>()

            //.AddMemoryCache()
            //.AddSingleton<MenuParameters>()
            .AddTransientAllStrategies<IMenuCommandListFactoryStrategy>(
                typeof(TasksListFactoryStrategy).Assembly)
            //.AddSingleton<IProcesses, Processes>()
            .AddSingleton<IMenuBuilder, CrawlerMenuBuilder>()
            .AddTransientAllStrategies<IMenuCommandFactoryStrategy>(
                typeof(CrawlerParametersEditorCliMenuCommandFactoryStrategy).Assembly)
            //.AddTransientAllStrategies<IToolCommandFactoryStrategy>(
            //    typeof(CorrectNewDatabaseToolCommandFactoryStrategy).Assembly,
            //    typeof(JetBrainsCleanupCodeRunnerToolCommandFactoryStrategy).Assembly,
            //    typeof(JsonFromProjectDbProjectGetterFactoryStrategy).Assembly,
            //    typeof(GenerateApiRoutesToolCommandFactoryStrategy).Assembly,
            //    typeof(ApplicationSettingsEncoderToolCommandFactoryStrategy).Assembly)
            .AddApplication(x =>
            {
                x.AppName = appName;
            })
            .AddMainParametersManager(x =>
            {
                x.ParametersFileName = parametersFileName;
                x.Par = par;
            });

        // @formatter:on
        return services;
    }
}
