using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using RobotsTxt.Entities;
using RobotsTxt.Enums;

namespace RobotsTxt;

public static class RobotsFactory
{
    public static Robots? AnaliseContentAndCreateRobots(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        string[] lines = content.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
            .Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
        return lines.Length == 0 ? null : ReadLinesAndCreateRobots(lines);
    }

    private static Robots ReadLinesAndCreateRobots(IEnumerable<string> lines)
    {
        var globalAccessRules = new List<AccessAllowRule>();
        var specificAccessRules = new List<AccessAllowRule>();
        var crawlDelayRules = new List<CrawlDelayRule>();
        var sitemaps = new List<Sitemap>();
        bool malformed = false;
        bool isAnyPathDisallowed = false;
        bool hasRules = false;

        int ruleCount = 0;
        bool lastLineWasUserAgentLine = false;
        var userAgents = new List<string>();

        foreach (string line in lines)
        {
            var robotsLine = Line.Create(line);
            switch (robotsLine.Type)
            {
                case LineType.Comment: //ignore the comments
                    continue;
                case LineType.UserAgent:
                    string? userAgentFromLine = robotsLine.Value;
                    if (string.IsNullOrWhiteSpace(userAgentFromLine))
                    {
                        continue;
                    }

                    if (!lastLineWasUserAgentLine)
                    {
                        userAgents.Clear();
                    }

                    userAgents.Add(userAgentFromLine);
                    lastLineWasUserAgentLine = true;
                    continue;
                case LineType.Sitemap:
                    lastLineWasUserAgentLine = false;
                    string? siteMapPath = robotsLine.Value;
                    if (siteMapPath is null)
                    {
                        continue;
                    }

                    Sitemap? siteMap = Sitemap.FromLine(siteMapPath);
                    if (siteMap is not null)
                    {
                        sitemaps.Add(siteMap);
                    }

                    continue;
                case LineType.AccessRule:
                case LineType.CrawlDelayRule:
                    lastLineWasUserAgentLine = false;
                    //if there's a rule without user-agent declaration, ignore it
                    if (userAgents.Count == 0)
                    {
                        malformed = true;
                        continue;
                    }

                    foreach (string userAgent in userAgents)
                    {
                        if (robotsLine is { Type: LineType.AccessRule, Field: not null, Value: not null })
                        {
                            var accessRule = AccessAllowRule.Create(userAgent, robotsLine.Field, robotsLine.Value,
                                ++ruleCount);
                            if (accessRule.For.Equals("*", StringComparison.Ordinal))
                            {
                                globalAccessRules.Add(accessRule);
                            }
                            else
                            {
                                specificAccessRules.Add(accessRule);
                            }

                            if (!accessRule.Allowed && !string.IsNullOrEmpty(accessRule.Path))
                                // We say !String.IsNullOrEmpty(x.Path) because the rule "Disallow: " means nothing is disallowed.
                            {
                                isAnyPathDisallowed = true;
                            }
                        }
                        else
                        {
                            crawlDelayRules.Add(new CrawlDelayRule(userAgent, robotsLine, ++ruleCount));
                        }
                    }

                    hasRules = true;
                    continue;
                case LineType.Unknown:
                    lastLineWasUserAgentLine = false;
                    malformed = true;
                    continue;
                default:
                    throw new SwitchExpressionException(
                        "Unexpected LineType encountered in RobotsFactory.ReadLinesAndCreateRobots.");
            }
        }

        return new Robots(globalAccessRules, specificAccessRules, crawlDelayRules, sitemaps, malformed,
            isAnyPathDisallowed, hasRules, AllowRuleImplementation.MoreSpecific);
    }
}
