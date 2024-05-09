using System.Collections.Generic;
using LibParameters;

// ReSharper disable CollectionNeverUpdated.Global

namespace CrawlerDb.Models;

public sealed class HostModel : ItemData
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public HostModel(string hostName)
    {
        HostName = hostName;
    }

    public int HostId { get; set; }
    public string HostName { get; set; }
    public bool HostProhibited { get; set; }


    public ICollection<UrlModel> Urls { get; set; } = new HashSet<UrlModel>();

    //public ICollection<UrlAllowModel> UrlAllows { get; set; } = new HashSet<UrlAllowModel>();
    public ICollection<HostByBatch> HostsByBatches { get; set; } = new HashSet<HostByBatch>();
    public ICollection<Robot> Robots { get; set; } = new HashSet<Robot>();
}