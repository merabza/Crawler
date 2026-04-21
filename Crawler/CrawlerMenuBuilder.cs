using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using AppCliTools.CliMenu;
using AppCliTools.CliParametersDataEdit;
using AppCliTools.CliParametersDataEdit.Models;
using AppCliTools.CliTools.Services.MenuBuilder;
using Crawler.Menu;
using Crawler.Menu.Hosts;
using DatabaseTools.DbTools;
using DatabaseTools.DbTools.Models;
using DatabaseTools.DbToolsFactory;
using DoCrawler.Models;
using LanguageExt;
using Microsoft.Extensions.Logging;
using OneOf;
using ParametersManagement.LibDatabaseParameters;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared;
using SystemTools.SystemToolsShared.Errors;

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

    public CliMenuSet BuildMainMenu()
    {
        List<string> excludeList = [];

        if (!CheckConnection())
        {
            excludeList.Add(nameof(HostListCliMenuCommandFactoryStrategy));
        }

        //მთავარი მენიუს ჩატვირთვა
        return CliMenuSetFactory.CreateMenuSet("Main Menu",
            MenuData.MainMenuCommandFactoryStrategyNames.Except(excludeList).ToList(), _serviceProvider, true);
    }

    private bool CheckConnection()
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

                    OneOf<DbAuthSettingsBase, Error[]> dbAuthSettingsCreateResult = DbAuthSettingsCreator.Create(
                        databaseServerConnectionData.WindowsNtIntegratedSecurity,
                        databaseServerConnectionData.ServerUser, databaseServerConnectionData.ServerPass, true);

                    if (dbAuthSettingsCreateResult.IsT1)
                    {
                        Error.PrintErrorsOnConsole(dbAuthSettingsCreateResult.AsT1);
                        return false;
                    }

                    DbClient? dc = DbClientFactory.GetDbClient(_logger, true, dataProvider.Value,
                        databaseServerConnectionData.ServerAddress, dbAuthSettingsCreateResult.AsT0,
                        databaseServerConnectionData.TrustServerCertificate, _application.AppName,
                        databaseServerConnectionData.DatabaseName);

                    if (dc is null)
                    {
                        Console.WriteLine("Database client does not created. dc is null");
                        return false;
                    }

                    Option<Error[]> testConnectionResult = dc.TestConnection(true, token).Result;
                    if (testConnectionResult.IsNone)
                    {
                        return true;
                    }

                    Error.PrintErrorsOnConsole((Error[])testConnectionResult);

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
