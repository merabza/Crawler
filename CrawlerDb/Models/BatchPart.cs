using System;
using System.Collections.Generic;

// ReSharper disable CollectionNeverUpdated.Global

namespace CrawlerDb.Models;

public sealed class BatchPart
{
    private Batch? _batchNavigation;

    // ReSharper disable once ConvertToPrimaryConstructor
    public BatchPart(int batchId, DateTime created)
    {
        BatchId = batchId;
        Created = created;
    }

    public int BpId { get; set; }
    public int BatchId { get; set; }
    public DateTime Created { get; set; }
    public DateTime? Finished { get; set; }

    public Batch BatchNavigation
    {
        get => _batchNavigation ??
               throw new InvalidOperationException("Uninitialized property: " + nameof(BatchNavigation));
        set => _batchNavigation = value;
    }

    public ICollection<UrlGraphNode> UrlGraphNodes { get; set; } = new HashSet<UrlGraphNode>();
    public ICollection<TermByUrl> TermsByUrls { get; set; } = new HashSet<TermByUrl>();
    public ICollection<ContentAnalysis> ContentsAnalysis { get; set; } = new HashSet<ContentAnalysis>();
    public ICollection<Robot> Robots { get; set; } = new HashSet<Robot>();
}