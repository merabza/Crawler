using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliParameters;
using AppCliTools.CliParameters.Cruders;
using CrawlerDb.Models;
using LibCrawlerRepositories;
using SystemTools.SystemToolsShared;

namespace Crawler.Cruders;

public sealed class HostByBatchCruder : Cruder
{
    private readonly Batch _batch;

    //private readonly ICrawlerRepositoryCreatorFactory _crawlerRepositoryCreatorFactory;
    private readonly ICrawlerRepository _crawlerRepository;

    // ReSharper disable once ConvertToPrimaryConstructor
    public HostByBatchCruder(ICrawlerRepository crawlerRepository, Batch batch) : base("Host Name", "Host Names")
    {
        _batch = batch;
        _crawlerRepository = crawlerRepository;
    }

    public List<string> GetHostNamesByBatch()
    {
        return _crawlerRepository.GetHostStartUrlNamesByBatch(_batch);
    }

    protected override Dictionary<string, ItemData> GetCrudersDictionary()
    {
        return GetHostNamesByBatch().ToDictionary(k => k, ItemData (v) => new TextItemData { Text = v });
    }

    protected override ItemData CreateNewItem(string? recordKey, ItemData? defaultItemData)
    {
        return new TextItemData();
    }

    public override bool ContainsRecordWithKey(string recordKey)
    {
        List<string> hostNames = GetHostNamesByBatch();
        return hostNames.Contains(recordKey);
    }

    protected override ValueTask RemoveRecordWithKey(string recordKey, CancellationToken cancellationToken = default)
    {
        //List<string> hostNames = GetHostNamesByBatch();
        //hostNames?.Remove(recordKey);
        var uri = new Uri(recordKey);
        _crawlerRepository.RemoveHostNamesByBatch(_batch, uri.Scheme, uri.Host);

        _crawlerRepository.SaveChanges();
        return ValueTask.CompletedTask;
    }

    protected override ValueTask AddRecordWithKey(string recordKey, ItemData newRecord,
        CancellationToken cancellationToken = default)
    {
        //SupportToolsParameters parameters = (SupportToolsParameters)ParametersManager?.Parameters;
        //Dictionary<string, ProjectModel> projects = parameters?.Projects;
        //if (projects == null || !projects.ContainsKey(_projectName)) 
        //  return;
        //ProjectModel project = projects[_projectName];

        //project.RedundantFileNames ??= new List<string>();
        //project.RedundantFileNames.Add(recordKey);

        var uri = new Uri(recordKey);
        _crawlerRepository.AddHostNamesByBatch(_batch, uri.Scheme, uri.Host);
        _crawlerRepository.SaveChanges();
        return ValueTask.CompletedTask;
    }
}
