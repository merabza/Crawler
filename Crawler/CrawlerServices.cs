using System;
using AppCliTools.CliMenu;
using AppCliTools.CliMenu.DependencyInjection;
using AppCliTools.CliParametersDataEdit;
using AppCliTools.CliTools.App;
using AppCliTools.CliTools.DependencyInjection;
using AppCliTools.CliTools.Services.MenuBuilder;
using Crawler.Menu.CrawlerParametersEdit;
using Crawler.Menu.Tasks;
using CrawlerDb;
using DoCrawler.Models;
using LibCrawlerRepositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ParametersManagement.LibDatabaseParameters;
using ParametersManagement.LibParameters;
using Serilog.Events;
using SystemTools.SystemToolsShared;

namespace Crawler;

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
            services.AddContextByProvider(dataProvider, connectionString, commandTimeout);
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
        //if (!string.IsNullOrWhiteSpace(par.RecentCommandsFileName) && par.RecentCommandsCount > 0)
        //{
        //    services.AddRecentCommandsService(x =>
        //    {
        //        x.RecentCommandsFileName = par.RecentCommandsFileName;
        //        x.RecentCommandsCount = par.RecentCommandsCount;
        //    });
        //}

        return services;
    }

    private static IServiceCollection AddApplication(this IServiceCollection services,
        Action<ApplicationOptions> setupAction)
    {
        services.AddSingleton<IApplication, Application>();
        services.Configure(setupAction);
        return services;
    }

    private static IServiceCollection AddMainParametersManager(this IServiceCollection services,
        Action<MainParametersManagerOptions> setupAction)
    {
        services.AddSingleton<IParametersManager, ParametersManager>();
        services.Configure(setupAction);
        return services;
    }

    //private static IServiceCollection AddRecentCommandsService(this IServiceCollection services,
    //    Action<RecentCommandOptions> setupAction)
    //{
    //    services.AddSingleton<IRecentCommandsService, RecentCommandsService>();
    //    services.Configure(setupAction);
    //    return services;
    //}

    private static IServiceCollection AddContextByProvider(this IServiceCollection services,
        EDatabaseProvider? dataProvider, string connectionString, int commandTimeout)
    {
        switch (dataProvider)
        {
            case EDatabaseProvider.SqlServer:
                services.AddDbContext<CrawlerDbContext>(options => options.UseSqlServer(connectionString, sqlOptions =>
                {
                    if (commandTimeout > -1)
                    {
                        sqlOptions.CommandTimeout(commandTimeout);
                    }
                }));
                break;
            case EDatabaseProvider.None:
            case EDatabaseProvider.SqLite:
            case EDatabaseProvider.OleDb:
            case EDatabaseProvider.WebAgent:
            case null:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(dataProvider));
        }

        return services;
    }
}
