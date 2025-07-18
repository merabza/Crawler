using System;
using System.Collections.Generic;
using System.Linq;
using CliParameters.Cruders;
using CliParameters.FieldEditors;
using CrawlerDb.Models;
using LibCrawlerRepositories;
using LibParameters;

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
        var repo = GetCrawlerRepository();
        return repo.GetSchemesList();
    }

    protected override Dictionary<string, ItemData> GetCrudersDictionary()
    {
        var schemesList = GetSchemes();
        return schemesList.ToDictionary(k => k.SchName, v => (ItemData)v);
    }

    public override bool ContainsRecordWithKey(string recordKey)
    {
        var dict = GetCrudersDictionary();
        return dict.ContainsKey(recordKey);
    }

    public override void UpdateRecordWithKey(string recordKey, ItemData newRecord)
    {
        if (newRecord is not SchemeModel newScheme)
            return;

        var repo = GetCrawlerRepository();

        var scheme = repo.GetSchemeByName(recordKey) ?? throw new Exception("scheme is null");

        scheme.SchName = newScheme.SchName;
        repo.UpdateScheme(scheme);

        repo.SaveChanges();
    }

    protected override void AddRecordWithKey(string recordKey, ItemData newRecord)
    {
        if (newRecord is not SchemeModel newScheme)
            return;
        var repo = GetCrawlerRepository();
        repo.CreateScheme(newScheme);

        repo.SaveChanges();
    }

    protected override void RemoveRecordWithKey(string recordKey)
    {
        var repo = GetCrawlerRepository();
        var scheme = repo.GetSchemeByName(recordKey) ?? throw new Exception("scheme is null");
        repo.DeleteScheme(scheme);

        repo.SaveChanges();
    }

    protected override ItemData CreateNewItem(string? recordKey, ItemData? defaultItemData)
    {
        return new SchemeModel { SchName = recordKey ?? string.Empty };
    }
}