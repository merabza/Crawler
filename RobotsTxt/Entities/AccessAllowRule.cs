using System;

namespace RobotsTxt.Entities;

public class AccessAllowRule : Rule
{
    public string Path { get; }
    public bool Allowed { get; private set; }

    public AccessAllowRule(string userAgent, Line line, int order)
        : base(userAgent, order)
    {
        Path = line.Value;
        if (!string.IsNullOrEmpty(Path))
        {
            // get rid of the preceding * wild char
            while (Path.StartsWith("*", StringComparison.Ordinal))
            {
                Path = Path[1..];
            }
            if (!Path.StartsWith('/'))
            {
                Path = "/" + Path;
            }
        }
        Allowed = line.Field.ToLowerInvariant().Equals("allow");
    }
}