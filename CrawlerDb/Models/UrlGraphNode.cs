using System;

namespace CrawlerDb.Models;

public sealed class UrlGraphNode
{
    private BatchPart? _batchPartNavigation;
    private UrlModel? _fromUrlNavigation;
    private UrlModel? _gotUrlNavigation;

    public UrlGraphNode()
    {
    }

    public UrlGraphNode(int fromUrlId, UrlModel gotUrlNavigation, int batchPartId)
    {
        FromUrlId = fromUrlId;
        GotUrlNavigation = gotUrlNavigation;
        BatchPartId = batchPartId;
    }

    public int UgnId { get; set; }
    public int BatchPartId { get; set; }
    public int FromUrlId { get; set; }
    public int GotUrlId { get; set; }

    public BatchPart BatchPartNavigation
    {
        get => _batchPartNavigation ??
               throw new InvalidOperationException("Uninitialized property: " + nameof(BatchPartNavigation));
        set => _batchPartNavigation = value;
    }

    public UrlModel FromUrlNavigation
    {
        get => _fromUrlNavigation ??
               throw new InvalidOperationException("Uninitialized property: " + nameof(FromUrlNavigation));
        set => _fromUrlNavigation = value;
    }

    public UrlModel GotUrlNavigation
    {
        get => _gotUrlNavigation ??
               throw new InvalidOperationException("Uninitialized property: " + nameof(GotUrlNavigation));
        set => _gotUrlNavigation = value;
    }
}