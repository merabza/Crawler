using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CliParameters;
using CrawlerDb.Models;
using LibCrawlerRepositories;
using LibParameters;
using SystemToolsShared;

namespace Crawler.Cruders;

public sealed class HostCruder : Cruder
{
    private readonly ICrawlerRepositoryCreatorFabric _crawlerRepositoryCreatorFabric;

    // ReSharper disable once ConvertToPrimaryConstructor
    public HostCruder(ICrawlerRepositoryCreatorFabric crawlerRepositoryCreatorFabric) : base("Host", "Hosts")
    {
        _crawlerRepositoryCreatorFabric = crawlerRepositoryCreatorFabric;
        //FieldEditors.Add(new TextFieldEditor(nameof(HostModel.HostName)));
    }

    private ICrawlerRepository GetCrawlerRepository()
    {
        return _crawlerRepositoryCreatorFabric.GetCrawlerRepository();
    }

    private List<HostModel> GetHosts()
    {
        var repo = GetCrawlerRepository();
        return repo.GetHostsList();
    }

    protected override Dictionary<string, ItemData> GetCrudersDictionary()
    {
        var hostsList = GetHosts();
        return hostsList.ToDictionary(k => k.HostName, v => (ItemData)v);
    }

    public override bool ContainsRecordWithKey(string recordKey)
    {
        var dict = GetCrudersDictionary();
        return dict.ContainsKey(recordKey);
    }

    public override void UpdateRecordWithKey(string recordKey, ItemData newRecord)
    {
        if (newRecord is not HostModel newHost)
            return;

        var repo = GetCrawlerRepository();

        var host = repo.GetHostByName(recordKey) ?? throw new Exception("host is null");
        host.HostName = newHost.HostName;
        repo.UpdateHost(host);

        repo.SaveChanges();
    }

    protected override void AddRecordWithKey(string recordKey, ItemData newRecord)
    {
        if (newRecord is not HostModel newHost)
            return;
        var repo = GetCrawlerRepository();
        repo.CreateHost(newHost);

        repo.SaveChanges();
    }

    protected override void RemoveRecordWithKey(string recordKey)
    {
        var repo = GetCrawlerRepository();
        var host = repo.GetHostByName(recordKey) ?? throw new Exception("host is null");
        repo.DeleteHost(host);

        repo.SaveChanges();
    }

    protected override ItemData CreateNewItem(string? recordKey, ItemData? defaultItemData)
    {
        return new HostModel { HostName = string.Empty };
    }

    public override bool CheckValidation(ItemData item)
    {
        if (item is not HostModel newHost)
        {
            StShared.WriteErrorLine("Invalid Host Data", true);
            return false;
        }

        Regex re = new(@"[-a-zA-Z0-9]{1,256}\.([-a-zA-Z0-9]{1,256}\.)*[a-zA-Z0-9()]{1,6}");
        var m = re.Match(newHost.HostName);
        if (m is { Success: true, Groups.Count: 2 } && m.Groups[0].Value == newHost.HostName)
            return true;
        StShared.WriteErrorLine($"Invalid Host Name {newHost.HostName}.", true);
        return false;
    }
}