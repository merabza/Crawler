using System;

namespace RobotsTxt;

public static class UriFabric
{
    public static Uri? GetUri(string strRef)
    {
        Uri? newUri = null;
        try
        {
            newUri = new Uri(strRef);
        }
        catch (UriFormatException)
        {
        }

        return newUri;
    }

    public static Uri? GetUri(Uri baseUri, string relativeUri)
    {
        Uri? newUri = null;
        try
        {
            newUri = new Uri(baseUri, relativeUri);
        }
        catch (UriFormatException)
        {
        }

        return newUri;
    }
}