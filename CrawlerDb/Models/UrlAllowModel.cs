//using System;

//// ReSharper disable CollectionNeverUpdated.Global

//namespace CrawlerDb.Models;

//public sealed class UrlAllowModel
//{
//    private HostModel? _hostNavigation;

//    // ReSharper disable once ConvertToPrimaryConstructor
//    public UrlAllowModel(int hostId, string patternText, bool isAllowed)
//    {
//        HostId = hostId;
//        PatternText = patternText;
//        IsAllowed = isAllowed;
//    }

//    public int UaId { get; set; }
//    public int HostId { get; set; }
//    public string PatternText { get; set; }
//    public bool IsAllowed { get; set; }

//    public HostModel HostNavigation
//    {
//        get => _hostNavigation ??
//               throw new InvalidOperationException("Uninitialized property: " + nameof(HostNavigation));
//        set => _hostNavigation = value;
//    }

//}