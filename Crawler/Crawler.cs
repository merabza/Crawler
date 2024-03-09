//Created by ProjectMainClassCreator at 4/22/2021 17:17:01

using System;
using System.Linq;
using CliMenu;
using CliParameters.MenuCommands;
using CliParametersDataEdit;
using CliParametersDataEdit.Models;
using CliTools;
using CliTools.Commands;
using Crawler.Cruders;
using Crawler.MenuCommands;
using DbTools;
using DbToolsFabric;
using DoCrawler.Models;
using LibCrawlerRepositories;
using LibDataInput;
using LibParameters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SystemToolsShared;

namespace Crawler;

public sealed class Crawler : CliAppLoop
{
    private readonly ILogger _logger;
    private readonly ParametersManager _parametersManager;
    private readonly ServiceProvider _serviceProvider;

    public Crawler(ILogger logger, ParametersManager parametersManager, ServiceProvider serviceProvider)
    {
        _logger = logger;
        _parametersManager = parametersManager;
        _serviceProvider = serviceProvider;
    }


    protected override bool BuildMainMenu()
    {
        var parameters = (CrawlerParameters)_parametersManager.Parameters;

        //if (parameters == null)
        //{
        //    StShared.WriteErrorLine("minimal parameters not found", true);
        //    return false;
        //}

        CliMenuSet mainMenuSet = new("Main Menu");
        AddChangeMenu(mainMenuSet);

        //ძირითადი პარამეტრების რედაქტირება
        CrawlerParametersEditor crawlerParametersEditor = new(parameters, _parametersManager, _logger);
        mainMenuSet.AddMenuItem(new ParametersEditorListCommand(crawlerParametersEditor),
            "Crawler Parameters Editor");

        if (CheckConnection())
        {
            var crawlerRepositoryCreatorFabric =
                _serviceProvider.GetService<ICrawlerRepositoryCreatorFabric>();
            if (crawlerRepositoryCreatorFabric is not null)
            {
                //ჰოსტების რედაქტორი
                HostCruder hostCruder = new(crawlerRepositoryCreatorFabric);
                mainMenuSet.AddMenuItem(new CruderListCommand(hostCruder), "Hosts");

                //სქემების რედაქტორი
                SchemeCruder schemeCruder = new(crawlerRepositoryCreatorFabric);
                mainMenuSet.AddMenuItem(new CruderListCommand(schemeCruder), "Schemes");

                //პაკეტების რედაქტორი
                BatchCruder batchCruder = new(_logger, crawlerRepositoryCreatorFabric, parameters);
                mainMenuSet.AddMenuItem(new CruderListCommand(batchCruder), "Batches");

                //ამოცანები
                NewTaskCommand newAppTaskCommand = new(_parametersManager);
                mainMenuSet.AddMenuItem(newAppTaskCommand);

                foreach (var kvp in parameters.Tasks.OrderBy(o => o.Key))
                    mainMenuSet.AddMenuItem(new TaskSubMenuCommand(_logger, _parametersManager,
                        crawlerRepositoryCreatorFabric,
                        kvp.Key));
            }
        }


        //ქრაულერის ამოცანების სია
        //CruderListCommand crawlerTaskListCommand =
        //  new CruderListCommand(new CrawlerTaskCruder(_logger, _parametersManager, _crawlerRepositoryCreatorFabric));
        //mainMenuSet.AddMenuItem(crawlerTaskListCommand.Name, crawlerTaskListCommand);


        //გასასვლელი
        var key = ConsoleKey.Escape.Value().ToLower();
        mainMenuSet.AddMenuItem(key, "Exit", new ExitCommand(), key.Length);

        return true;
    }


    private bool CheckConnection()
    {
        var parameters = (CrawlerParameters)_parametersManager.Parameters;

        if (string.IsNullOrWhiteSpace(parameters.ConnectionString))
        {
            Console.WriteLine("ConnectionString is empty");
            return false;
        }

        try
        {
            var dbConnectionParameters =
                DbConnectionFabric.GetDbConnectionParameters(parameters.DataProvider, parameters.ConnectionString);
            if (dbConnectionParameters is null)
            {
                Console.WriteLine("dbConnectionParameters is null");
                return false;
            }

            switch (parameters.DataProvider)
            {
                case EDataProvider.Sql:

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

                    var dbAuthSettings = DbAuthSettingsCreator.Create(
                        databaseServerConnectionData.WindowsNtIntegratedSecurity,
                        databaseServerConnectionData.ServerUser, databaseServerConnectionData.ServerPass);

                    if (dbAuthSettings is null)
                        return false;

                    var dc = DbClientFabric.GetDbClient(_logger, true, parameters.DataProvider,
                        databaseServerConnectionData.ServerAddress, dbAuthSettings,
                        ProgramAttributes.Instance.GetAttribute<string>("AppName"),
                        databaseServerConnectionData.DatabaseName);

                    if (dc is null)
                    {
                        Console.WriteLine("Database client does not created. dc is null");
                        return false;
                    }

                    var testConnectionResult = dc.TestConnection();
                    if (testConnectionResult.IsNone)
                        return true;

                    Err.PrintErrorsOnConsole((Err[])testConnectionResult);

                    Console.WriteLine("Database test connection failed");
                    return false;

                case EDataProvider.SqLite:
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