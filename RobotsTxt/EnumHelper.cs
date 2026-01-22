using RobotsTxt.Enums;

namespace RobotsTxt;

internal static class EnumHelper
{
    internal static LineType GetLineType(string field)
    {
        return field switch
        {
            "user-agent" => LineType.UserAgent,
            "allow" or "disallow" => LineType.AccessRule,
            "crawl-delay" => LineType.CrawlDelayRule,
            "sitemap" => LineType.Sitemap,
            _ => LineType.Unknown
        };
    }
}
