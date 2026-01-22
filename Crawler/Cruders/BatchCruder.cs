using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using AppCliTools.CliMenu;
using AppCliTools.CliParameters.CliMenuCommands;
using AppCliTools.CliParameters.Cruders;
using AppCliTools.CliParameters.FieldEditors;
using Crawler.MenuCommands;
using CrawlerDb.Models;
using DoCrawler.Models;
using LibCrawlerRepositories;
using Microsoft.Extensions.Logging;
using ParametersManagement.LibParameters;

namespace Crawler.Cruders;

public sealed class BatchCruder : Cruder
{
    private readonly ICrawlerRepositoryCreatorFactory _crawlerRepositoryCreatorFactory;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;
    private readonly CrawlerParameters _par;

    public BatchCruder(ILogger logger, IHttpClientFactory httpClientFactory,
        ICrawlerRepositoryCreatorFactory crawlerRepositoryCreatorFactory, CrawlerParameters par) : base("Batch",
        "Batches")
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _crawlerRepositoryCreatorFactory = crawlerRepositoryCreatorFactory;
        _par = par;

        FieldEditors.Add(new BoolFieldEditor(nameof(Batch.IsOpen)));
        FieldEditors.Add(new BoolFieldEditor(nameof(Batch.AutoCreateNextPart)));
    }

    private ICrawlerRepository GetCrawlerRepository()
    {
        return _crawlerRepositoryCreatorFactory.GetCrawlerRepository();
    }

    private List<Batch> GetBatches()
    {
        ICrawlerRepository repo = GetCrawlerRepository();
        return repo.GetBatchesList();
    }

    protected override Dictionary<string, ItemData> GetCrudersDictionary()
    {
        List<Batch> batchesList = GetBatches();
        return batchesList.ToDictionary(k => k.BatchName, v => (ItemData)v);
    }

    public override bool ContainsRecordWithKey(string recordKey)
    {
        Dictionary<string, ItemData> dict = GetCrudersDictionary();
        return dict.ContainsKey(recordKey);
    }

    public override void UpdateRecordWithKey(string recordKey, ItemData newRecord)
    {
        if (newRecord is not Batch newBatch)
        {
            return;
        }

        ICrawlerRepository repo = GetCrawlerRepository();

        Batch batch = repo.GetBatchByName(recordKey) ?? throw new Exception("batch is null");
        batch.BatchName = newBatch.BatchName;
        repo.UpdateBatch(batch);

        repo.SaveChanges();
    }

    protected override void AddRecordWithKey(string recordKey, ItemData newRecord)
    {
        if (newRecord is not Batch newBatch)
        {
            return;
        }

        ICrawlerRepository repo = GetCrawlerRepository();
        repo.CreateBatch(newBatch);

        repo.SaveChanges();
    }

    protected override void RemoveRecordWithKey(string recordKey)
    {
        ICrawlerRepository repo = GetCrawlerRepository();
        Batch? batch = repo.GetBatchByName(recordKey);
        if (batch is null)
        {
            return;
        }

        repo.DeleteBatch(batch);

        repo.SaveChanges();
    }

    protected override ItemData CreateNewItem(string? recordKey, ItemData? defaultItemData)
    {
        return new Batch { BatchName = recordKey ?? string.Empty };
    }

    public override void FillDetailsSubMenu(CliMenuSet itemSubMenuSet, string itemName)
    {
        base.FillDetailsSubMenu(itemSubMenuSet, itemName);

        List<Batch> batchesList = GetBatches();
        Dictionary<string, Batch> batches = batchesList.ToDictionary(k => k.BatchName, v => v);
        Batch batch = batches[itemName];

        itemSubMenuSet.AddMenuItem(new BatchTaskCliMenuCommand(_logger, _httpClientFactory,
            _crawlerRepositoryCreatorFactory, _par, batch));

        var detailsCruder = new HostByBatchCruder(_crawlerRepositoryCreatorFactory, batch);
        var newItemCommand =
            new NewItemCliMenuCommand(detailsCruder, itemName, $"Create New {detailsCruder.CrudName}");
        itemSubMenuSet.AddMenuItem(newItemCommand);

        List<string> hostNames = detailsCruder.GetHostNamesByBatch();

        foreach (HostSubMenuCliMenuCommand detailListCommand in hostNames.Select(s =>
                     new HostSubMenuCliMenuCommand(detailsCruder, s, itemName, true)))
        {
            itemSubMenuSet.AddMenuItem(detailListCommand);
        }
    }
}
