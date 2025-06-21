using System;

namespace RobotsTxt.Entities;

public sealed class Sitemap
{
    private Sitemap(Uri url)
    {
        Url = url;
    }

    /// <summary>
    ///     The URL to the sitemap.
    ///     WARNING : This property could be null if the file declared a relative path to the sitemap rather than absolute,
    ///     which is the standard.
    /// </summary>
    public Uri Url { get; private set; }

    internal static Sitemap? FromLine(string strUrl)
    {
        var uri = UriFactory.GetUri(strUrl);
        return uri is null ? null : new Sitemap(uri);
    }
}