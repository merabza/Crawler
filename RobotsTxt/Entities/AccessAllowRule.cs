using System;

namespace RobotsTxt.Entities;

public sealed class AccessAllowRule : Rule
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public AccessAllowRule(string userAgent, string path, bool allowed, int order) : base(userAgent, order)
    {
        Path = path;
        Allowed = allowed;
    }

    public string Path { get; }
    public bool Allowed { get; private set; }

    public static AccessAllowRule Create(string userAgent, string field, string path, int order)
    {
        const string uriPathDelimiter = "/";
        if (!string.IsNullOrEmpty(path))
        {
            // get rid of the preceding * wild char
            while (path.StartsWith('*'))
            {
                path = path[1..];
            }

            if (!path.StartsWith('/'))
            {
                path = uriPathDelimiter + path;
            }
        }

        bool allowed = string.Equals(field.ToUpperInvariant(), "ALLOW", StringComparison.Ordinal);
        return new AccessAllowRule(userAgent, path, allowed, order);
    }
}
