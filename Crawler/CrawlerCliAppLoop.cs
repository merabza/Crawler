//Created by ProjectMainClassCreator at 4/22/2021 17:17:01

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using AppCliTools.CliMenu;
using AppCliTools.CliParameters.CliMenuCommands;
using AppCliTools.CliParametersDataEdit;
using AppCliTools.CliParametersDataEdit.Models;
using AppCliTools.CliTools;
using AppCliTools.CliTools.CliMenuCommands;
using AppCliTools.LibDataInput;
using Crawler.Cruders;
using Crawler.MenuCommands;
using DatabaseTools.DbTools;
using DatabaseTools.DbTools.Models;
using DatabaseTools.DbToolsFactory;
using DoCrawler.Models;
using LanguageExt;
using LibCrawlerRepositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OneOf;
using ParametersManagement.LibDatabaseParameters;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared;
using SystemTools.SystemToolsShared.Errors;

namespace Crawler;

public sealed class CrawlerCliAppLoop : CliAppLoop
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;
    private readonly ParametersManager _parametersManager;
    private readonly ServiceProvider _serviceProvider;

    // ReSharper disable once ConvertToPrimaryConstructor
    public CrawlerCliAppLoop(ILogger logger, IHttpClientFactory httpClientFactory, ParametersManager parametersManager,
        ServiceProvider serviceProvider)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _parametersManager = parametersManager;
        _serviceProvider = serviceProvider;
    }

    public override CliMenuSet BuildMainMenu()
    {
        var parameters = (CrawlerParameters)_parametersManager.Parameters;

        //if (parameters == null)
        //{
        //    StShared.WriteErrorLine("minimal parameters not found", true);
        //    return false;
        //}

        var mainMenuSet = new CliMenuSet("Main Menu");

        //ძირითადი პარამეტრების რედაქტირება
        var crawlerParametersEditor =
            new CrawlerParametersEditor(parameters, _parametersManager, _logger, _httpClientFactory);
        mainMenuSet.AddMenuItem(new ParametersEditorListCliMenuCommand(crawlerParametersEditor));

        if (CheckConnection())
        {
            var crawlerRepositoryCreatorFactory = _serviceProvider.GetService<ICrawlerRepositoryCreatorFactory>();
            if (crawlerRepositoryCreatorFactory is not null)
            {
                //ჰოსტების რედაქტორი
                var hostCruder = new HostCruder(crawlerRepositoryCreatorFactory);
                //"Hosts"
                mainMenuSet.AddMenuItem(new CruderListCliMenuCommand(hostCruder));

                //სქემების რედაქტორი
                var schemeCruder = new SchemeCruder(crawlerRepositoryCreatorFactory);
                //"Schemes"
                mainMenuSet.AddMenuItem(new CruderListCliMenuCommand(schemeCruder));

                //პაკეტების რედაქტორი
                var batchCruder = new BatchCruder(_logger, _httpClientFactory, crawlerRepositoryCreatorFactory,
                    parameters);
                //"Batches"
                mainMenuSet.AddMenuItem(new CruderListCliMenuCommand(batchCruder));

                //ამოცანები
                var newAppTaskCommand = new NewTaskCliMenuCommand(_parametersManager);
                mainMenuSet.AddMenuItem(newAppTaskCommand);

                foreach (KeyValuePair<string, TaskModel> kvp in parameters.Tasks.OrderBy(o => o.Key))
                {
                    mainMenuSet.AddMenuItem(new TaskSubMenuCliMenuCommand(_logger, _httpClientFactory,
                        _parametersManager, crawlerRepositoryCreatorFactory, kvp.Key));
                }
            }
        }

        //ქრაულერის ამოცანების სია
        //CruderListCommand crawlerTaskListCommand =
        //  new CruderListCommand(new CrawlerTaskCruder(_logger, _parametersManager, _crawlerRepositoryCreatorFactory));
        //mainMenuSet.AddMenuItem(crawlerTaskListCommand.Name, crawlerTaskListCommand);

        //გასასვლელი
        string key = ConsoleKey.Escape.Value().ToUpperInvariant();
        mainMenuSet.AddMenuItem(key, new ExitCliMenuCommand(), key.Length);

        return mainMenuSet;
    }

    private bool CheckConnection()
    {
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

                    OneOf<DbAuthSettingsBase, Err[]> dbAuthSettingsCreateResult = DbAuthSettingsCreator.Create(
                        databaseServerConnectionData.WindowsNtIntegratedSecurity,
                        databaseServerConnectionData.ServerUser, databaseServerConnectionData.ServerPass, true);

                    if (dbAuthSettingsCreateResult.IsT1)
                    {
                        Err.PrintErrorsOnConsole(dbAuthSettingsCreateResult.AsT1);
                        return false;
                    }

                    DbClient? dc = DbClientFactory.GetDbClient(_logger, true, dataProvider.Value,
                        databaseServerConnectionData.ServerAddress, dbAuthSettingsCreateResult.AsT0,
                        databaseServerConnectionData.TrustServerCertificate, ProgramAttributes.Instance.AppName,
                        databaseServerConnectionData.DatabaseName);

                    if (dc is null)
                    {
                        Console.WriteLine("Database client does not created. dc is null");
                        return false;
                    }

                    Option<Err[]> testConnectionResult = dc.TestConnection(true, token).Result;
                    if (testConnectionResult.IsNone)
                    {
                        return true;
                    }

                    Err.PrintErrorsOnConsole((Err[])testConnectionResult);

                    Console.WriteLine("Database test connection failed");
                    break;

                case EDatabaseProvider.SqLite:
                    return
                        false; //აქ ფაილის შემოწმება არის გასაკეთებელი. ჭეშმარიტი დაბრუნდეს, თუ ფაილი არსებობს და იხსნება
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
