using System;
using System.Collections.Generic;

// ReSharper disable CollectionNeverUpdated.Global

namespace CrawlerDb.Models;

public sealed class Term
{
    private TermType? _termTypeNavigation;

    public Term(string termText)
    {
        TermText = termText;
    }

    public Term(string termText, TermType termTypeNavigation)
    {
        TermText = termText;
        TermTypeNavigation = termTypeNavigation;
    }

    public int TrmId { get; set; }
    public string TermText { get; set; }
    public int TermTypeId { get; set; }

    public TermType TermTypeNavigation
    {
        get => _termTypeNavigation ??
               throw new InvalidOperationException("Uninitialized property: " + nameof(TermTypeNavigation));
        set => _termTypeNavigation = value;
    }

    public ICollection<TermByUrl> TermsByUrls { get; set; } = new HashSet<TermByUrl>();
}