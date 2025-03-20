using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using CliMenu;
using CliParameters;
using CliParameters.CliMenuCommands;
using CliParameters.FieldEditors;
using Crawler.MenuCommands;
using CrawlerDb.Models;
using DoCrawler.Models;
using LibCrawlerRepositories;
using LibParameters;
using Microsoft.Extensions.Logging;

namespace Crawler.Cruders;

public sealed class BatchCruder : Cruder
{
    private readonly ICrawlerRepositoryCreatorFabric _crawlerRepositoryCreatorFabric;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;
    private readonly CrawlerParameters _par;

    public BatchCruder(ILogger logger, IHttpClientFactory httpClientFactory,
        ICrawlerRepositoryCreatorFabric crawlerRepositoryCreatorFabric, CrawlerParameters par) : base("Batch",
        "Batches")
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _crawlerRepositoryCreatorFabric = crawlerRepositoryCreatorFabric;
        _par = par;

        FieldEditors.Add(new BoolFieldEditor(nameof(Batch.IsOpen)));
        FieldEditors.Add(new BoolFieldEditor(nameof(Batch.AutoCreateNextPart)));
    }

    private ICrawlerRepository GetCrawlerRepository()
    {
        return _crawlerRepositoryCreatorFabric.GetCrawlerRepository();
    }

    private List<Batch> GetBatches()
    {
        var repo = GetCrawlerRepository();
        return repo.GetBatchesList();
    }

    protected override Dictionary<string, ItemData> GetCrudersDictionary()
    {
        var batchesList = GetBatches();
        return batchesList.ToDictionary(k => k.BatchName, v => (ItemData)v);
    }

    public override bool ContainsRecordWithKey(string recordKey)
    {
        var dict = GetCrudersDictionary();
        return dict.ContainsKey(recordKey);
    }

    public override void UpdateRecordWithKey(string recordKey, ItemData newRecord)
    {
        if (newRecord is not Batch newBatch)
            return;

        var repo = GetCrawlerRepository();

        var batch = repo.GetBatchByName(recordKey) ?? throw new Exception("batch is null");
        batch.BatchName = newBatch.BatchName;
        repo.UpdateBatch(batch);

        repo.SaveChanges();
    }

    protected override void AddRecordWithKey(string recordKey, ItemData newRecord)
    {
        if (newRecord is not Batch newBatch)
            return;

        var repo = GetCrawlerRepository();
        repo.CreateBatch(newBatch);

        repo.SaveChanges();
    }

    protected override void RemoveRecordWithKey(string recordKey)
    {
        var repo = GetCrawlerRepository();
        var batch = repo.GetBatchByName(recordKey);
        if (batch is null)
            return;
        repo.DeleteBatch(batch);

        repo.SaveChanges();
    }

    protected override ItemData CreateNewItem(string? recordKey, ItemData? defaultItemData)
    {
        return new Batch { BatchName = recordKey ?? string.Empty };
    }

    public override void FillDetailsSubMenu(CliMenuSet itemSubMenuSet, string recordKey)
    {
        base.FillDetailsSubMenu(itemSubMenuSet, recordKey);

        var batchesList = GetBatches();
        var batches = batchesList.ToDictionary(k => k.BatchName, v => v);
        var batch = batches[recordKey];

        itemSubMenuSet.AddMenuItem(new BatchTaskCliMenuCommand(_logger, _httpClientFactory,
            _crawlerRepositoryCreatorFabric, _par, batch));

        HostByBatchCruder detailsCruder = new(_crawlerRepositoryCreatorFabric, batch);
        NewItemCliMenuCommand newItemCommand = new(detailsCruder, recordKey, $"Create New {detailsCruder.CrudName}");
        itemSubMenuSet.AddMenuItem(newItemCommand);

        var hostNames = detailsCruder.GetHostNamesByBatch();

        foreach (var detailListCommand in hostNames.Select(s =>
                     new HostSubMenuCliMenuCommand(detailsCruder, s, recordKey, true)))
            itemSubMenuSet.AddMenuItem(detailListCommand);
    }
}