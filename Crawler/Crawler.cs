//Created by ProjectMainClassCreator at 4/22/2021 17:17:01

using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using CliMenu;
using CliParameters.CliMenuCommands;
using CliParametersDataEdit;
using CliParametersDataEdit.Models;
using CliTools;
using CliTools.CliMenuCommands;
using Crawler.Cruders;
using Crawler.MenuCommands;
using DbTools;
using DbToolsFactory;
using DoCrawler.Models;
using LibCrawlerRepositories;
using LibDatabaseParameters;
using LibDataInput;
using LibParameters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SystemToolsShared;
using SystemToolsShared.Errors;

namespace Crawler;

public sealed class Crawler : CliAppLoop
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;
    private readonly ParametersManager _parametersManager;
    private readonly ServiceProvider _serviceProvider;

    // ReSharper disable once ConvertToPrimaryConstructor
    public Crawler(ILogger logger, IHttpClientFactory httpClientFactory, ParametersManager parametersManager,
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

        CliMenuSet mainMenuSet = new("Main Menu");

        //ძირითადი პარამეტრების რედაქტირება
        CrawlerParametersEditor crawlerParametersEditor =
            new(parameters, _parametersManager, _logger, _httpClientFactory);
        mainMenuSet.AddMenuItem(new ParametersEditorListCliMenuCommand(crawlerParametersEditor));

        if (CheckConnection())
        {
            var crawlerRepositoryCreatorFactory = _serviceProvider.GetService<ICrawlerRepositoryCreatorFactory>();
            if (crawlerRepositoryCreatorFactory is not null)
            {
                //ჰოსტების რედაქტორი
                HostCruder hostCruder = new(crawlerRepositoryCreatorFactory);
                //"Hosts"
                mainMenuSet.AddMenuItem(new CruderListCliMenuCommand(hostCruder));

                //სქემების რედაქტორი
                SchemeCruder schemeCruder = new(crawlerRepositoryCreatorFactory);
                //"Schemes"
                mainMenuSet.AddMenuItem(new CruderListCliMenuCommand(schemeCruder));

                //პაკეტების რედაქტორი
                BatchCruder batchCruder = new(_logger, _httpClientFactory, crawlerRepositoryCreatorFactory, parameters);
                //"Batches"
                mainMenuSet.AddMenuItem(new CruderListCliMenuCommand(batchCruder));

                //ამოცანები
                NewTaskCliMenuCommand newAppTaskCommand = new(_parametersManager);
                mainMenuSet.AddMenuItem(newAppTaskCommand);

                foreach (var kvp in parameters.Tasks.OrderBy(o => o.Key))
                    mainMenuSet.AddMenuItem(new TaskSubMenuCliMenuCommand(_logger, _httpClientFactory,
                        _parametersManager, crawlerRepositoryCreatorFactory, kvp.Key));
            }
        }

        //ქრაულერის ამოცანების სია
        //CruderListCommand crawlerTaskListCommand =
        //  new CruderListCommand(new CrawlerTaskCruder(_logger, _parametersManager, _crawlerRepositoryCreatorFactory));
        //mainMenuSet.AddMenuItem(crawlerTaskListCommand.Name, crawlerTaskListCommand);

        //გასასვლელი
        var key = ConsoleKey.Escape.Value().ToLower();
        mainMenuSet.AddMenuItem(key, new ExitCliMenuCommand(), key.Length);

        return mainMenuSet;
    }

    private bool CheckConnection()
    {
        var parameters = (CrawlerParameters)_parametersManager.Parameters;

        var databaseParameters = parameters.DatabaseParameters;

        if (databaseParameters is null)
        {
            Console.WriteLine("databaseParameters is null");
            return false;
        }

        var databaseServerConnections = new DatabaseServerConnections(parameters.DatabaseServerConnections);

        var (dataProvider, connectionString) =
            DbConnectionFactory.GetDataProviderAndConnectionString(databaseParameters, databaseServerConnections);

        if (dataProvider is null || connectionString is null)
        {
            Console.WriteLine("dataProvider is null || connectionString is null");
            return false;
        }

        try
        {
            var dbConnectionParameters =
                DbConnectionFactory.GetDbConnectionParameters(dataProvider.Value, connectionString);
            if (dbConnectionParameters is null)
            {
                Console.WriteLine("dbConnectionParameters is null");
                return false;
            }

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
                        string.IsNullOrWhiteSpace(databaseServerConnectionData.ServerUser) ||
                        string.IsNullOrWhiteSpace(databaseServerConnectionData.ServerPass) ||
                        string.IsNullOrWhiteSpace(databaseServerConnectionData.DatabaseName))
                    {
                        Console.WriteLine("databaseServerConnectionData parameters is not valid");
                        return false;
                    }

                    var dbAuthSettingsCreateResult = DbAuthSettingsCreator.Create(
                        databaseServerConnectionData.WindowsNtIntegratedSecurity,
                        databaseServerConnectionData.ServerUser, databaseServerConnectionData.ServerPass, true);

                    if (dbAuthSettingsCreateResult.IsT1)
                    {
                        Err.PrintErrorsOnConsole(dbAuthSettingsCreateResult.AsT1);
                        return false;
                    }

                    var dc = DbClientFactory.GetDbClient(_logger, true, dataProvider.Value,
                        databaseServerConnectionData.ServerAddress, dbAuthSettingsCreateResult.AsT0,
                        databaseServerConnectionData.TrustServerCertificate, ProgramAttributes.Instance.AppName,
                        databaseServerConnectionData.DatabaseName);

                    if (dc is null)
                    {
                        Console.WriteLine("Database client does not created. dc is null");
                        return false;
                    }

                    var testConnectionResult = dc.TestConnection(true, CancellationToken.None).Result;
                    if (testConnectionResult.IsNone)
                        return true;

                    Err.PrintErrorsOnConsole((Err[])testConnectionResult);

                    Console.WriteLine("Database test connection failed");
                    return false;

                case EDatabaseProvider.SqLite:
                    return
                        false; //აქ ფაილის შემოწმება არის გასაკეთებელი. ჭეშმარიტი დაბრუნდეს, თუ ფაილი არსებობს და იხსნება
            }

            return false;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in CheckConnection");
            return false;
        }
    }
}