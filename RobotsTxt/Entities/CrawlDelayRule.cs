using System.Globalization;

namespace RobotsTxt.Entities;

public sealed class CrawlDelayRule : Rule
{
    public CrawlDelayRule(string userAgent, Line line, int order) : base(userAgent, order)
    {
        double.TryParse(line.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var delay);
        Delay = (long)(delay * 1000);
    }

    public long Delay { get; private set; } // milliseconds
}