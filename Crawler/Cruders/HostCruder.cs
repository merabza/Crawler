using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliParameters.Cruders;
using AppCliTools.CliParameters.FieldEditors;
using CrawlerDb.Models;
using LibCrawlerRepositories;
using SystemTools.SystemToolsShared;

namespace Crawler.Cruders;

public sealed class HostCruder : Cruder
{
    private readonly ICrawlerRepository _crawlerRepository;
    //private readonly ICrawlerRepositoryCreatorFactory _crawlerRepositoryCreatorFactory;

    // ReSharper disable once ConvertToPrimaryConstructor
    public HostCruder(ICrawlerRepository crawlerRepository) : base("Host", "Hosts")
    {
        //_crawlerRepositoryCreatorFactory = crawlerRepositoryCreatorFactory;
        _crawlerRepository = crawlerRepository;
        FieldEditors.Add(new TextFieldEditor(nameof(HostModel.HostName)));
    }

    //private ICrawlerRepository GetCrawlerRepository()
    //{
    //    return _crawlerRepositoryCreatorFactory.GetCrawlerRepository();
    //}

    private List<HostModel> GetHosts()
    {
        //ICrawlerRepository repo = GetCrawlerRepository();
        return _crawlerRepository.GetHostsList();
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

        //ICrawlerRepository repo = GetCrawlerRepository();

        HostModel host = _crawlerRepository.GetHostByName(recordKey) ?? throw new Exception("host is null");
        host.HostName = newHost.HostName;
        _crawlerRepository.UpdateHost(host);

        _crawlerRepository.SaveChanges();
        return ValueTask.CompletedTask;
    }

    protected override ValueTask AddRecordWithKey(string recordKey, ItemData newRecord,
        CancellationToken cancellationToken = default)
    {
        if (newRecord is not HostModel newHost)
        {
            return ValueTask.CompletedTask;
        }

        //ICrawlerRepository repo = GetCrawlerRepository();
        _crawlerRepository.CreateHost(newHost);

        _crawlerRepository.SaveChanges();
        return ValueTask.CompletedTask;
    }

    protected override ValueTask RemoveRecordWithKey(string recordKey, CancellationToken cancellationToken = default)
    {
        //ICrawlerRepository repo = GetCrawlerRepository();
        HostModel host = _crawlerRepository.GetHostByName(recordKey) ?? throw new Exception("host is null");
        _crawlerRepository.DeleteHost(host);

        _crawlerRepository.SaveChanges();
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

        var re = new Regex(@"[-a-zA-Z0-9]{1,256}\.([-a-zA-Z0-9]{1,256}\.)*[a-zA-Z0-9()]{1,6}", RegexOptions.None,
            TimeSpan.FromSeconds(1));
        Match m = re.Match(newHost.HostName);
        if (m is { Success: true, Groups.Count: 2 } && m.Groups[0].Value == newHost.HostName)
        {
            return true;
        }

        StShared.WriteErrorLine($"Invalid Host Name {newHost.HostName}.", true);
        return false;
    }
}
