using System.Collections.Generic;
using LibParameters;

// ReSharper disable CollectionNeverUpdated.Global

namespace CrawlerDb.Models;

public sealed class Batch : ItemData
{
    public int BatchId { get; set; }
    public required string BatchName { get; set; }
    public bool IsOpen { get; set; }
    public bool AutoCreateNextPart { get; set; }

    public ICollection<HostByBatch> HostsByBatches { get; set; } = new HashSet<HostByBatch>();
    public ICollection<BatchPart> BatchParts { get; set; } = new HashSet<BatchPart>();
}