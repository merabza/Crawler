using System;

namespace CrawlerDb.Models;

public sealed class ContentAnalysis
{
    private BatchPart? _batchPartNavigation;
    private UrlModel? _urlNavigation;

    public int CaId { get; set; }
    public int BatchPartId { get; set; }
    public int UrlId { get; set; }
    public int ResponseStatusCode { get; set; }
    public DateTime Finish { get; set; }

    public BatchPart BatchPartNavigation
    {
        get =>
            _batchPartNavigation ??
            throw new InvalidOperationException("Uninitialized property: " + nameof(BatchPartNavigation));
        set => _batchPartNavigation = value;
    }

    public UrlModel UrlNavigation
    {
        get =>
            _urlNavigation ?? throw new InvalidOperationException("Uninitialized property: " + nameof(UrlNavigation));
        set => _urlNavigation = value;
    }
}