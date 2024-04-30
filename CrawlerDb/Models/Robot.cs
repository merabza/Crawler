using System;

namespace CrawlerDb.Models;

public sealed class Robot
{
    private BatchPart? _batchPartNavigation;
    private HostModel? _hostNavigation;
    private SchemeModel? _schemeNavigation;

    public Robot()
    {
    }

    public Robot(int batchPartId, SchemeModel schemeNavigation, HostModel hostNavigation)
    {
        BatchPartId = batchPartId;
        SchemeNavigation = schemeNavigation;
        HostNavigation = hostNavigation;
    }

    public int RbtId { get; set; }
    public int BatchPartId { get; set; }
    public int SchemeId { get; set; }
    public int HostId { get; set; }
    public string? RobotsTxt { get; set; }


    public BatchPart BatchPartNavigation
    {
        get => _batchPartNavigation ??
               throw new InvalidOperationException("Uninitialized property: " + nameof(BatchPartNavigation));
        set => _batchPartNavigation = value;
    }

    public SchemeModel SchemeNavigation
    {
        get => _schemeNavigation ??
               throw new InvalidOperationException("Uninitialized property: " + nameof(SchemeNavigation));
        set => _schemeNavigation = value;
    }

    public HostModel HostNavigation
    {
        get => _hostNavigation ??
               throw new InvalidOperationException("Uninitialized property: " + nameof(HostNavigation));
        set => _hostNavigation = value;
    }
}