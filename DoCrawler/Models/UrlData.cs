using CrawlerDb.Models;

namespace DoCrawler.Models;

public sealed class UrlData
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public UrlData(HostModel host, ExtensionModel extension, SchemeModel scheme, string checkedUri, string absolutePath,
        int urlHashCode, UrlModel? url)
    {
        Host = host;
        Extension = extension;
        Scheme = scheme;
        CheckedUri = checkedUri;
        AbsolutePath = absolutePath;
        UrlHashCode = urlHashCode;
        Url = url;
    }

    //public string StartUri { get; set; }
    public HostModel Host { get; set; }
    public ExtensionModel Extension { get; set; }
    public SchemeModel Scheme { get; set; }
    public string CheckedUri { get; set; }
    public string AbsolutePath { get; }
    public int UrlHashCode { get; set; }
    public UrlModel? Url { get; set; }
}