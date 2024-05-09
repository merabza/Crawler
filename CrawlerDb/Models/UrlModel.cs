using System;
using System.Collections.Generic;

// ReSharper disable CollectionNeverUpdated.Global

namespace CrawlerDb.Models;

public sealed class UrlModel
{
    private ExtensionModel? _extensionNavigation;

    private HostModel? _hostNavigation;
    private SchemeModel? _schemeNavigation;

    public UrlModel(string urlName)
    {
        UrlName = urlName;
    }

    public UrlModel(string urlName, HostModel hostNavigation, ExtensionModel extensionNavigation,
        SchemeModel schemeNavigation, int urlHashCode, bool isSiteMap, bool isAllowed)
    {
        UrlName = urlName;
        HostNavigation = hostNavigation;
        ExtensionNavigation = extensionNavigation;
        SchemeNavigation = schemeNavigation;
        UrlHashCode = urlHashCode;
        IsSiteMap = isSiteMap;
        IsAllowed = isAllowed;
    }

    public int UrlId { get; set; }
    public string UrlName { get; set; }
    public int HostId { get; set; }
    public int ExtensionId { get; set; }
    public int SchemeId { get; set; }
    public int UrlHashCode { get; set; }
    public bool IsSiteMap { get; set; }

    public bool IsAllowed { get; set; }
    //public DateTime? LastDownloaded { get; set; }
    //public int DownloadTryCount { get; set; }

    public HostModel HostNavigation
    {
        get => _hostNavigation ??
               throw new InvalidOperationException("Uninitialized property: " + nameof(HostNavigation));
        set => _hostNavigation = value;
    }

    public ExtensionModel ExtensionNavigation
    {
        get => _extensionNavigation ??
               throw new InvalidOperationException("Uninitialized property: " + nameof(ExtensionNavigation));
        set => _extensionNavigation = value;
    }

    public SchemeModel SchemeNavigation
    {
        get => _schemeNavigation ??
               throw new InvalidOperationException("Uninitialized property: " + nameof(SchemeNavigation));
        set => _schemeNavigation = value;
    }

    public ICollection<UrlGraphNode> UrlGraphNodesFrom { get; set; } = new HashSet<UrlGraphNode>();
    public ICollection<UrlGraphNode> UrlGraphNodesGot { get; set; } = new HashSet<UrlGraphNode>();
    public ICollection<TermByUrl> TermsByUrls { get; set; } = new HashSet<TermByUrl>();
    public ICollection<ContentAnalysis> ContentsAnalysis { get; set; } = new HashSet<ContentAnalysis>();
}