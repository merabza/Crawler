using System;

namespace RobotsTxt.Entities;

public class AccessAllowRule : Rule
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
        if (!string.IsNullOrEmpty(path))
        {
            // get rid of the preceding * wild char
            while (path.StartsWith("*", StringComparison.Ordinal)) path = path[1..];
            if (!path.StartsWith('/')) path = "/" + path;
        }

        var allowed = field.ToLowerInvariant().Equals("allow");
        return new AccessAllowRule(userAgent, path, allowed, order);
    }
}