using System;
using System.Collections.Generic;
using System.Linq;
using CliMenu;
using CliParameters;
using CliParameters.FieldEditors;
using CliParameters.MenuCommands;
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
    private readonly ILogger _logger;
    private readonly CrawlerParameters _par;

    public BatchCruder(ILogger logger, ICrawlerRepositoryCreatorFabric crawlerRepositoryCreatorFabric,
        CrawlerParameters par) : base("Batch", "Batches")
    {
        _logger = logger;
        _crawlerRepositoryCreatorFabric = crawlerRepositoryCreatorFabric;
        _par = par;

        FieldEditors.Add(new BoolFieldEditor(nameof(Batch.IsOpen), false));
        FieldEditors.Add(new BoolFieldEditor(nameof(Batch.AutoCreateNextPart), false));
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

        var batch = repo.GetBatchByName(recordKey);
        if (batch is null)
            throw new Exception("batch is null");
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

    protected override ItemData CreateNewItem(ItemData? defaultItemData)
    {
        return new Batch();
    }


    public override void FillDetailsSubMenu(CliMenuSet itemSubMenuSet, string recordKey)
    {
        base.FillDetailsSubMenu(itemSubMenuSet, recordKey);

        var batchesList = GetBatches();
        var batches = batchesList.ToDictionary(k => k.BatchName, v => v);
        var batch = batches[recordKey];

        itemSubMenuSet.AddMenuItem(new BatchTaskCommand(_logger, _crawlerRepositoryCreatorFabric, _par, batch),
            "Run this batch");

        HostByBatchCruder detailsCruder = new(_crawlerRepositoryCreatorFabric, batch);
        NewItemCommand newItemCommand = new(detailsCruder, recordKey, $"Create New {detailsCruder.CrudName}");
        itemSubMenuSet.AddMenuItem(newItemCommand);

        var hostNames = detailsCruder.GetHostNamesByBatch();

        foreach (var detailListCommand in hostNames.Select(s =>
                     new HostSubMenuCommand(detailsCruder, s, recordKey, true)))
            itemSubMenuSet.AddMenuItem(detailListCommand);
    }
}