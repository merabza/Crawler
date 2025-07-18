using System;
using System.Collections.Generic;
using System.Linq;
using CliParameters;
using CliParameters.Cruders;
using CrawlerDb.Models;
using LibCrawlerRepositories;
using LibParameters;

namespace Crawler.Cruders;

public sealed class HostByBatchCruder : Cruder
{
    private readonly Batch _batch;
    private readonly ICrawlerRepositoryCreatorFactory _crawlerRepositoryCreatorFactory;

    // ReSharper disable once ConvertToPrimaryConstructor
    public HostByBatchCruder(ICrawlerRepositoryCreatorFactory crawlerRepositoryCreatorFactory, Batch batch) : base(
        "Host Name", "Host Names")
    {
        _crawlerRepositoryCreatorFactory = crawlerRepositoryCreatorFactory;
        _batch = batch;
    }

    private ICrawlerRepository GetCrawlerRepository()
    {
        return _crawlerRepositoryCreatorFactory.GetCrawlerRepository();
    }

    public List<string> GetHostNamesByBatch()
    {
        var repo = GetCrawlerRepository();
        return repo.GetHostStartUrlNamesByBatch(_batch);
    }

    protected override Dictionary<string, ItemData> GetCrudersDictionary()
    {
        return GetHostNamesByBatch().ToDictionary(k => k, v => (ItemData)new TextItemData { Text = v });
    }

    protected override ItemData CreateNewItem(string? recordKey, ItemData? defaultItemData)
    {
        return new TextItemData();
    }

    public override bool ContainsRecordWithKey(string recordKey)
    {
        var hostNames = GetHostNamesByBatch();
        return hostNames.Contains(recordKey);
    }

    protected override void RemoveRecordWithKey(string recordKey)
    {
        //List<string> hostNames = GetHostNamesByBatch();
        //hostNames?.Remove(recordKey);
        var repo = GetCrawlerRepository();
        var uri = new Uri(recordKey);
        repo.RemoveHostNamesByBatch(_batch, uri.Scheme, uri.Host);

        repo.SaveChanges();
    }

    protected override void AddRecordWithKey(string recordKey, ItemData newRecord)
    {
        //SupportToolsParameters parameters = (SupportToolsParameters)ParametersManager?.Parameters;
        //Dictionary<string, ProjectModel> projects = parameters?.Projects;
        //if (projects == null || !projects.ContainsKey(_projectName)) 
        //  return;
        //ProjectModel project = projects[_projectName];

        //project.RedundantFileNames ??= new List<string>();
        //project.RedundantFileNames.Add(recordKey);

        var repo = GetCrawlerRepository();
        var uri = new Uri(recordKey);
        repo.AddHostNamesByBatch(_batch, uri.Scheme, uri.Host);
        repo.SaveChanges();
    }
}