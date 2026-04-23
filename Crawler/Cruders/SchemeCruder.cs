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
    private readonly ICrawlerRepository _crawlerRepository;

    public SchemeCruder(ICrawlerRepository crawlerRepository) : base("Scheme", "Schemes")
    {
        _crawlerRepository = crawlerRepository;
        FieldEditors.Add(new BoolFieldEditor(nameof(SchemeModel.SchProhibited)));
    }

    private List<SchemeModel> GetSchemes()
    {
        return _crawlerRepository.GetSchemesList();
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

        SchemeModel scheme = _crawlerRepository.GetSchemeByName(recordKey) ?? throw new Exception("scheme is null");

        scheme.SchName = newScheme.SchName;
        _crawlerRepository.UpdateScheme(scheme);

        _crawlerRepository.SaveChanges();
        return ValueTask.CompletedTask;
    }

    protected override ValueTask AddRecordWithKey(string recordKey, ItemData newRecord,
        CancellationToken cancellationToken = default)
    {
        if (newRecord is not SchemeModel newScheme)
        {
            return ValueTask.CompletedTask;
        }

        _crawlerRepository.CreateScheme(newScheme);

        _crawlerRepository.SaveChanges();
        return ValueTask.CompletedTask;
    }

    protected override ValueTask RemoveRecordWithKey(string recordKey, CancellationToken cancellationToken = default)
    {
        SchemeModel scheme = _crawlerRepository.GetSchemeByName(recordKey) ?? throw new Exception("scheme is null");
        _crawlerRepository.DeleteScheme(scheme);

        _crawlerRepository.SaveChanges();
        return ValueTask.CompletedTask;
    }

    protected override ItemData CreateNewItem(string? recordKey, ItemData? defaultItemData)
    {
        return new SchemeModel { SchName = recordKey ?? string.Empty };
    }
}
