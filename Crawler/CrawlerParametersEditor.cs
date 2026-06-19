using System.Net.Http;
using AppCliTools.CliParameters;
using AppCliTools.CliParameters.FieldEditors;
using AppCliTools.CliParametersDataEdit.Cruders;
using AppCliTools.CliParametersDataEdit.FieldEditors;
using AppCliTools.CliParametersEdit.Cruders;
using DoCrawler.Cruders;
using DoCrawler.Models;
using Microsoft.Extensions.Logging;
using ParametersManagement.LibDatabaseParameters;
using ParametersManagement.LibFileParameters.Models;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared;

namespace Crawler;

public sealed class CrawlerParametersEditor : ParametersEditor
{
    public CrawlerParametersEditor(IApplication application, IParameters parameters,
        IParametersManager parametersManager, ILogger logger, IHttpClientFactory httpClientFactory) : base(
        "Crawler Parameters Editor", parameters, parametersManager)
    {
        FieldEditors.Add(new FolderPathFieldEditor(nameof(CrawlerParameters.LogFolder)));

        FieldEditors.Add(new IntFieldEditor(nameof(CrawlerParameters.LoadPagesMaxCount), 10000));
        FieldEditors.Add(new TextFieldEditor(nameof(CrawlerParameters.Alphabet), "აბგდევზთიკლმნოპჟრსტუფქღყშჩცძწჭხჯჰ"));
        FieldEditors.Add(new TextFieldEditor(nameof(CrawlerParameters.ExtraSymbols), "-–"));

        FieldEditors.Add(new DictionaryFieldEditor<PunctuationCruder, PunctuationModel>(
            nameof(CrawlerParameters.Punctuations), x => new PunctuationCruder(logger, parametersManager, x)));

        FieldEditors.Add(new DictionaryFieldEditor<DatabaseServerConnectionCruder, DatabaseServerConnectionData>(
            nameof(CrawlerParameters.DatabaseServerConnections),
            x => new DatabaseServerConnectionCruder(application, logger, httpClientFactory, parametersManager, x)));

        FieldEditors.Add(new DatabaseParametersFieldEditor(application, logger, httpClientFactory,
            nameof(CrawlerParameters.DatabaseParameters), parametersManager));

        FieldEditors.Add(new DictionaryFieldEditor<SmartSchemaCruder, SmartSchema>(
            nameof(CrawlerParameters.SmartSchemas), x => new SmartSchemaCruder(parametersManager, x)));

        FieldEditors.Add(new DictionaryFieldEditor<FileStorageCruder, FileStorageData>(
            nameof(CrawlerParameters.FileStorages), x => new FileStorageCruder(logger, parametersManager, x)));
    }
}
