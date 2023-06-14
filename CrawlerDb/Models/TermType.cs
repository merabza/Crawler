using System.Collections.Generic;

// ReSharper disable CollectionNeverUpdated.Global

namespace CrawlerDb.Models;

public sealed class TermType
{
    public TermType(string ttKey)
    {
        TtKey = ttKey;
    }

    public int TtId { get; set; }
    public string TtKey { get; set; }
    public string? TtName { get; set; }

    public ICollection<Term> Terms { get; set; } = new HashSet<Term>();
}