using System.Collections.Generic;
using LibParameters;

namespace CrawlerDb.Models;

public sealed class ExtensionModel : ItemData
{
    public ExtensionModel(string extName)
    {
        ExtName = extName;
    }

    public int ExtId { get; set; }
    public string ExtName { get; set; }
    public bool ExtProhibited { get; set; }

    public ICollection<UrlModel> Urls { get; set; } = new HashSet<UrlModel>();
}