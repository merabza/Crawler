using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliParameters.Cruders;
using CrawlerDb.Models;
using LibCrawlerRepositories;
using SystemTools.SystemToolsShared;

namespace Crawler.Cruders;

public sealed class HostCruder : Cruder
{
    private readonly ICrawlerRepositoryCreatorFactory _crawlerRepositoryCreatorFactory;

    // ReSharper disable once ConvertToPrimaryConstructor
    public HostCruder(ICrawlerRepositoryCreatorFactory crawlerRepositoryCreatorFactory) : base("Host", "Hosts")
    {
        _crawlerRepositoryCreatorFactory = crawlerRepositoryCreatorFactory;
        //FieldEditors.Add(new TextFieldEditor(nameof(HostModel.HostName)));
    }

    private ICrawlerRepository GetCrawlerRepository()
    {
        return _crawlerRepositoryCreatorFactory.GetCrawlerRepository();
    }

    private List<HostModel> GetHosts()
    {
        ICrawlerRepository repo = GetCrawlerRepository();
        return repo.GetHostsList();
    }

    protected override Dictionary<string, ItemData> GetCrudersDictionary()
    {
        List<HostModel> hostsList = GetHosts();
        return hostsList.ToDictionary(k => k.HostName, v => (ItemData)v);
    }

    public override bool ContainsRecordWithKey(string recordKey)
    {
        Dictionary<string, ItemData> dict = GetCrudersDictionary();
        return dict.ContainsKey(recordKey);
    }

    public override ValueTask UpdateRecordWithKey(string recordKey, ItemData newRecord,
        CancellationToken cancellationToken = default)
    {
        if (newRecord is not HostModel newHost)
        {
            return ValueTask.CompletedTask;
        }

        ICrawlerRepository repo = GetCrawlerRepository();

        HostModel host = repo.GetHostByName(recordKey) ?? throw new Exception("host is null");
        host.HostName = newHost.HostName;
        repo.UpdateHost(host);

        repo.SaveChanges();
        return ValueTask.CompletedTask;
    }

    protected override ValueTask AddRecordWithKey(string recordKey, ItemData newRecord,
        CancellationToken cancellationToken = default)
    {
        if (newRecord is not HostModel newHost)
        {
            return ValueTask.CompletedTask;
        }

        ICrawlerRepository repo = GetCrawlerRepository();
        repo.CreateHost(newHost);

        repo.SaveChanges();
        return ValueTask.CompletedTask;
    }

    protected override ValueTask RemoveRecordWithKey(string recordKey, CancellationToken cancellationToken = default)
    {
        ICrawlerRepository repo = GetCrawlerRepository();
        HostModel host = repo.GetHostByName(recordKey) ?? throw new Exception("host is null");
        repo.DeleteHost(host);

        repo.SaveChanges();
        return ValueTask.CompletedTask;
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

        var re = new Regex(@"[-a-zA-Z0-9]{1,256}\.([-a-zA-Z0-9]{1,256}\.)*[a-zA-Z0-9()]{1,6}");
        Match m = re.Match(newHost.HostName);
        if (m is { Success: true, Groups.Count: 2 } && m.Groups[0].Value == newHost.HostName)
        {
            return true;
        }

        StShared.WriteErrorLine($"Invalid Host Name {newHost.HostName}.", true);
        return false;
    }
}
