﻿using System.Net.Http;
using CliParameters;
using CliParameters.FieldEditors;
using CliParametersDataEdit.Cruders;
using CliParametersDataEdit.FieldEditors;
using DoCrawler.Cruders;
using DoCrawler.Models;
using LibDatabaseParameters;
using LibParameters;
using Microsoft.Extensions.Logging;

namespace Crawler;

public sealed class CrawlerParametersEditor : ParametersEditor
{
    public CrawlerParametersEditor(IParameters parameters, ParametersManager parametersManager, ILogger logger,
        IHttpClientFactory httpClientFactory) : base("Crawler Parameters Editor", parameters, parametersManager)
    {
        FieldEditors.Add(new FolderPathFieldEditor(nameof(CrawlerParameters.LogFolder)));

        FieldEditors.Add(new DatabaseServerConnectionNameFieldEditor(logger, httpClientFactory,
            nameof(CrawlerParameters.DatabaseConnectionName), parametersManager, true));

        FieldEditors.Add(new IntFieldEditor(nameof(CrawlerParameters.CommandTimeOut), 10000));
        FieldEditors.Add(new IntFieldEditor(nameof(CrawlerParameters.LoadPagesMaxCount), 10000));
        FieldEditors.Add(new TextFieldEditor(nameof(CrawlerParameters.Alphabet), "აბგდევზთიკლმნოპჟრსტუფქღყშჩცძწჭხჯჰ"));
        FieldEditors.Add(new TextFieldEditor(nameof(CrawlerParameters.ExtraSymbols), "-–"));

        //FieldEditors.Add(new PunctuationsFieldEditor(nameof(CrawlerParameters.Punctuations), parametersManager,
        //    logger));

        FieldEditors.Add(new DictionaryFieldEditor<PunctuationCruder, PunctuationModel>(
            nameof(CrawlerParameters.Punctuations), logger, parametersManager));

        //FieldEditors.Add(new DatabaseServerConnectionsFieldEditor(logger, httpClientFactory, parametersManager,
        //    nameof(CrawlerParameters.DatabaseServerConnections)));

        FieldEditors.Add(new DictionaryFieldEditor<DatabaseServerConnectionCruder, DatabaseServerConnectionData>(
            nameof(CrawlerParameters.DatabaseServerConnections), logger, httpClientFactory, parametersManager));

        FieldEditors.Add(new DatabaseParametersFieldEditor(logger, httpClientFactory,
            nameof(CrawlerParameters.DatabaseParameters), parametersManager));
    }
}