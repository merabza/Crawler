using System;

namespace CrawlerDb.Models;

public sealed class HostByBatch
{
    private Batch? _batchNavigation;
    private HostModel? _hostNavigation;
    private SchemeModel? _schemeNavigation;

    public HostByBatch()
    {
    }

    public HostByBatch(int batchId, SchemeModel schemeNavigation, HostModel hostNavigation)
    {
        BatchId = batchId;
        SchemeNavigation = schemeNavigation;
        HostNavigation = hostNavigation;
    }

    public int HbbId { get; set; }
    public int BatchId { get; set; }
    public int SchemeId { get; set; }

    public int HostId { get; set; }
    //public string? RobotsTxt { get; set; }


    public Batch BatchNavigation
    {
        get =>
            _batchNavigation ??
            throw new InvalidOperationException("Uninitialized property: " + nameof(BatchNavigation));
        set => _batchNavigation = value;
    }

    public SchemeModel SchemeNavigation
    {
        get =>
            _schemeNavigation ??
            throw new InvalidOperationException("Uninitialized property: " + nameof(SchemeNavigation));
        set => _schemeNavigation = value;
    }

    public HostModel HostNavigation
    {
        get =>
            _hostNavigation ?? throw new InvalidOperationException("Uninitialized property: " + nameof(HostNavigation));
        set => _hostNavigation = value;
    }
}