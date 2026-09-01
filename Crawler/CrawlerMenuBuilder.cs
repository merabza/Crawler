using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using AppCliTools.CliParametersDataEdit;
using AppCliTools.CliParametersDataEdit.Models;
using AppCliTools.CliTools.Services.MenuBuilder;
using Crawler.Menu;
using Crawler.Menu.Batches;
using Crawler.Menu.Hosts;
using Crawler.Menu.Schemes;
using Crawler.Menu.Tasks;
using DatabaseTools.DbTools;
using DatabaseTools.DbTools.Models;
using DatabaseTools.DbToolsFactory;
using DoCrawler.Models;
using Microsoft.Extensions.Logging;
using ParametersManagement.LibDatabaseParameters;
using ParametersManagement.LibParameters;
using SystemTools.SharedKernel;
using SystemTools.SystemToolsShared;

namespace Crawler;

public sealed class CrawlerMenuBuilder : IMenuBuilder
{
    private readonly IApplication _application;
    private readonly ILogger<CrawlerMenuBuilder> _logger;
    private readonly IParametersManager _parametersManager;
    private readonly IServiceProvider _serviceProvider;

    // ReSharper disable once ConvertToPrimaryConstructor
    public CrawlerMenuBuilder(IServiceProvider serviceProvider, IParametersManager parametersManager,
        ILogger<CrawlerMenuBuilder> logger, IApplication application)
    {
        _serviceProvider = serviceProvider;
        _parametersManager = parametersManager;
        _logger = logger;
        _application = application;
    }

    public async Task<CliMenuSet?> BuildMainMenu()
    {
        List<string> excludeList = [];

        if (await CheckConnection())
        {
            return CliMenuSetFactory.CreateMenuSet("Main Menu",
                [.. MenuData.MainMenuCommandFactoryStrategyNames.Except(excludeList)], _serviceProvider, true);
        }

        excludeList.Add(nameof(HostListCliMenuCommandFactoryStrategy));
        excludeList.Add(nameof(SchemeListCliMenuCommandFactoryStrategy));
        excludeList.Add(nameof(BatchListCliMenuCommandFactoryStrategy));
        excludeList.Add(nameof(NewTaskCliMenuCommandFactoryStrategy));
        excludeList.Add(nameof(TasksListFactoryStrategy));

        //მთავარი მენიუს ჩატვირთვა
        return CliMenuSetFactory.CreateMenuSet("Main Menu",
            [.. MenuData.MainMenuCommandFactoryStrategyNames.Except(excludeList)], _serviceProvider, true);
    }

    private async Task<bool> CheckConnection()
    {
        Console.WriteLine("Checking connection to database...");

        var parameters = (CrawlerParameters)_parametersManager.Parameters;

        DatabaseParameters? databaseParameters = parameters.DatabaseParameters;

        if (databaseParameters is null)
        {
            Console.WriteLine("databaseParameters is null");
            return false;
        }

        var databaseServerConnections = new DatabaseServerConnections(parameters.DatabaseServerConnections);

        (EDatabaseProvider? dataProvider, string? connectionString, _) =
            DbConnectionFactory.GetDataProviderConnectionStringCommandTimeOut(databaseParameters,
                databaseServerConnections);

        if (dataProvider is null || connectionString is null)
        {
            Console.WriteLine("dataProvider is null || connectionString is null");
            return false;
        }

        try
        {
            DbConnectionParameters? dbConnectionParameters =
                DbConnectionFactory.GetDbConnectionParameters(dataProvider.Value, connectionString);
            if (dbConnectionParameters is null)
            {
                Console.WriteLine("dbConnectionParameters is null");
                return false;
            }

            // ReSharper disable once using
            // ReSharper disable once DisposableConstructor
            using var cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;
            token.ThrowIfCancellationRequested();

            switch (dataProvider.Value)
            {
                case EDatabaseProvider.SqlServer:

                    if (dbConnectionParameters is not SqlServerConnectionParameters databaseServerConnectionData)
                    {
                        Console.WriteLine("databaseServerConnectionData is null");
                        return false;
                    }

                    //Console.WriteLine("Try connect to server...");

                    //მოისინჯოს ბაზასთან დაკავშირება.
                    //თუ დაკავშირება ვერ მოხერხდა, გამოვიდეს ამის შესახებ შეტყობინება და შევთავაზოთ მონაცემების შეყვანის გაგრძელება, ან გაჩერება
                    //აქ გამოიყენება ბაზასთან პირდაპირ დაკავშირება ვებაგენტის გარეშე,
                    //რადგან სწორედ ასეთი ტიპის კავშირების რედაქტორია ეს.
                    if (string.IsNullOrWhiteSpace(databaseServerConnectionData.ServerAddress) ||
                        //string.IsNullOrWhiteSpace(databaseServerConnectionData.ServerUser) ||
                        //string.IsNullOrWhiteSpace(databaseServerConnectionData.ServerPass) ||
                        string.IsNullOrWhiteSpace(databaseServerConnectionData.DatabaseName))
                    {
                        Console.WriteLine("databaseServerConnectionData parameters is not valid");
                        return false;
                    }

                    Result<DbAuthSettingsBase> dbAuthSettingsCreateResult = DbAuthSettingsCreator.Create(
                        databaseServerConnectionData.WindowsNtIntegratedSecurity,
                        databaseServerConnectionData.ServerUser, databaseServerConnectionData.ServerPass, true);

                    if (dbAuthSettingsCreateResult.IsFailure)
                    {
                        dbAuthSettingsCreateResult.Error.PrintErrorsOnConsole();
                        return false;
                    }

                    DbClient? dc = DbClientFactory.GetDbClient(_logger, true, dataProvider.Value,
                        databaseServerConnectionData.ServerAddress, dbAuthSettingsCreateResult.Value,
                        databaseServerConnectionData.TrustServerCertificate, _application.AppName,
                        databaseServerConnectionData.DatabaseName);

                    if (dc is null)
                    {
                        Console.WriteLine("Database client does not created. dc is null");
                        return false;
                    }

                    Result testConnectionResult = await dc.TestConnection(true, token);
                    if (testConnectionResult.IsSuccess)
                    {
                        return true;
                    }

                    testConnectionResult.Error.PrintErrorsOnConsole();

                    Console.WriteLine("Database test connection failed");
                    break;

                case EDatabaseProvider.SqLite:
                    return
                        false; //აქ ფაილის შემოწმება არის გასაკეთებელი. ჭეშმარიტი დაბრუნდეს, თუ ფაილი არსებობს და იხსნება
                case EDatabaseProvider.None:
                case EDatabaseProvider.OleDb:
                case EDatabaseProvider.WebAgent:
                    break;
                default:
                    throw new SwitchExpressionException("Unsupported database provider.");
            }

            return false;
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation was canceled.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in CheckConnection");
            return false;
        }

        return false;
    }
}
