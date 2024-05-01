using System;
using System.Collections.Generic;
using System.Linq;
using RobotsTxt.Entities;
using RobotsTxt.Enums;

namespace RobotsTxt;

public static class RobotsFabric
{
    public static Robots? AnaliseContentAndCreateRobots(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return null;
        var lines = content.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
            .Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
        return lines.Length == 0 ? null : ReadLinesAndCreateRobots(lines);
    }

    private static Robots ReadLinesAndCreateRobots(IEnumerable<string> lines)
    {
        var globalAccessRules = new List<AccessAllowRule>();
        var specificAccessRules = new List<AccessAllowRule>();
        var crawlDelayRules = new List<CrawlDelayRule>();
        var sitemaps = new List<Sitemap>();
        var malformed=false;
        var isAnyPathDisallowed=false;
        var hasRules=false;

        var ruleCount = 0;
        var userAgent = string.Empty;

        foreach (var line in lines)
        {
            var robotsLine = Line.Create(line);
            switch (robotsLine.Type)
            {
                case LineType.Comment: //ignore the comments
                    continue;
                case LineType.UserAgent:
                    userAgent = robotsLine.Value;
                    continue;
                case LineType.Sitemap:
                    sitemaps.Add(Sitemap.FromLine(robotsLine));
                    continue;
                case LineType.AccessRule:
                case LineType.CrawlDelayRule:
                    //if there's a rule without user-agent declaration, ignore it
                    if (string.IsNullOrEmpty(userAgent))
                    {
                        malformed = true;
                        continue;
                    }

                    if (robotsLine is { Type: LineType.AccessRule, Field: not null, Value: not null })
                    {
                        var accessRule = AccessAllowRule.Create(userAgent, robotsLine.Field, robotsLine.Value, ++ruleCount);
                        if (accessRule.For.Equals("*"))
                            globalAccessRules.Add(accessRule);
                        else
                            specificAccessRules.Add(accessRule);

                        if (!accessRule.Allowed && !string.IsNullOrEmpty(accessRule.Path))
                            // We say !String.IsNullOrEmpty(x.Path) because the rule "Disallow: " means nothing is disallowed.
                            isAnyPathDisallowed = true;
                    }
                    else
                        crawlDelayRules.Add(new CrawlDelayRule(userAgent, robotsLine, ++ruleCount));

                    hasRules = true;
                    continue;
                case LineType.Unknown:
                    malformed = true;
                    continue;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return new Robots(globalAccessRules, specificAccessRules, crawlDelayRules, sitemaps, malformed, isAnyPathDisallowed, hasRules,
            AllowRuleImplementation.MoreSpecific);
    }


}