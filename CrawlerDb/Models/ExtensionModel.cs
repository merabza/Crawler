using System.Collections.Generic;
using SystemTools.SystemToolsShared;

namespace CrawlerDb.Models;

public sealed class ExtensionModel : ItemData
{
    public int ExtId { get; set; }
    public required string ExtName { get; set; }
    public bool ExtProhibited { get; set; }

    public ICollection<UrlModel> Urls { get; set; } = new HashSet<UrlModel>();
}
