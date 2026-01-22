using System;

namespace RobotsTxt;

public static class UriFactory
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
            // Intentionally left blank: return null if format is invalid
        }

        return newUri;
    }

    public static Uri? GetUri(Uri baseUri, string relativeUrName)
    {
        Uri? newUri = null;
        try
        {
            newUri = new Uri(baseUri, relativeUrName);
        }
        catch (UriFormatException)
        {
            // Intentionally left blank: return null if format is invalid
        }

        return newUri;
    }
}
