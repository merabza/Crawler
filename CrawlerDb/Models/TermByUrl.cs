using System;

namespace CrawlerDb.Models;

public sealed class TermByUrl
{
    private BatchPart? _batchPartNavigation;
    private Term? _termNavigation;
    private UrlModel? _urlNavigation;

    public int TbuId { get; set; }
    public int BatchPartId { get; set; }
    public int UrlId { get; set; }
    public int TermId { get; set; }
    public int Position { get; set; }

    public UrlModel UrlNavigation
    {
        get =>
            _urlNavigation ?? throw new InvalidOperationException("Uninitialized property: " + nameof(UrlNavigation));
        set => _urlNavigation = value;
    }

    public Term TermNavigation
    {
        get =>
            _termNavigation ?? throw new InvalidOperationException("Uninitialized property: " + nameof(TermNavigation));
        set => _termNavigation = value;
    }

    public BatchPart BatchPartNavigation
    {
        get =>
            _batchPartNavigation ??
            throw new InvalidOperationException("Uninitialized property: " + nameof(BatchPartNavigation));
        set => _batchPartNavigation = value;
    }
}
