using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliParameters.Cruders;
using AppCliTools.CliParameters.FieldEditors;
using CrawlerDb.Models;
using LibCrawlerRepositories;
using SystemTools.SystemToolsShared;

namespace Crawler.Cruders;

public sealed class SchemeCruder : Cruder
{
    private readonly ICrawlerRepositoryCreatorFactory _crawlerRepositoryCreatorFactory;

    public SchemeCruder(ICrawlerRepositoryCreatorFactory crawlerRepositoryCreatorFactory) : base("Scheme", "Schemes")
    {
        _crawlerRepositoryCreatorFactory = crawlerRepositoryCreatorFactory;
        FieldEditors.Add(new BoolFieldEditor(nameof(SchemeModel.SchProhibited)));
    }

    private ICrawlerRepository GetCrawlerRepository()
    {
        return _crawlerRepositoryCreatorFactory.GetCrawlerRepository();
    }

    private List<SchemeModel> GetSchemes()
    {
        ICrawlerRepository repo = GetCrawlerRepository();
        return repo.GetSchemesList();
    }

    protected override Dictionary<string, ItemData> GetCrudersDictionary()
    {
        List<SchemeModel> schemesList = GetSchemes();
        return schemesList.ToDictionary(k => k.SchName, ItemData (v) => v);
    }

    public override bool ContainsRecordWithKey(string recordKey)
    {
        Dictionary<string, ItemData> dict = GetCrudersDictionary();
        return dict.ContainsKey(recordKey);
    }

    public override ValueTask UpdateRecordWithKey(string recordKey, ItemData newRecord,
        CancellationToken cancellationToken = default)
    {
        if (newRecord is not SchemeModel newScheme)
        {
            return ValueTask.CompletedTask;
        }

        ICrawlerRepository repo = GetCrawlerRepository();

        SchemeModel scheme = repo.GetSchemeByName(recordKey) ?? throw new Exception("scheme is null");

        scheme.SchName = newScheme.SchName;
        repo.UpdateScheme(scheme);

        repo.SaveChanges();
        return ValueTask.CompletedTask;
    }

    protected override ValueTask AddRecordWithKey(string recordKey, ItemData newRecord,
        CancellationToken cancellationToken = default)
    {
        if (newRecord is not SchemeModel newScheme)
        {
            return ValueTask.CompletedTask;
        }

        ICrawlerRepository repo = GetCrawlerRepository();
        repo.CreateScheme(newScheme);

        repo.SaveChanges();
        return ValueTask.CompletedTask;
    }

    protected override ValueTask RemoveRecordWithKey(string recordKey, CancellationToken cancellationToken = default)
    {
        ICrawlerRepository repo = GetCrawlerRepository();
        SchemeModel scheme = repo.GetSchemeByName(recordKey) ?? throw new Exception("scheme is null");
        repo.DeleteScheme(scheme);

        repo.SaveChanges();
        return ValueTask.CompletedTask;
    }

    protected override ItemData CreateNewItem(string? recordKey, ItemData? defaultItemData)
    {
        return new SchemeModel { SchName = recordKey ?? string.Empty };
    }
}
