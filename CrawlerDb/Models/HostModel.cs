using System.Collections.Generic;
using LibParameters;

// ReSharper disable CollectionNeverUpdated.Global

namespace CrawlerDb.Models;

public sealed class HostModel : ItemData
{
    public int HostId { get; set; }
    public string HostName { get; set; }
    public bool HostProhibited { get; set; }


    public ICollection<UrlModel> Urls { get; set; } = new HashSet<UrlModel>();
    public ICollection<HostByBatch> HostsByBatches { get; set; } = new HashSet<HostByBatch>();
}