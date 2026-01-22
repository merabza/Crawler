using System;
using System.Collections.Generic;
using System.Linq;
using RobotsTxt.Entities;
using RobotsTxt.Enums;

namespace RobotsTxt;

public sealed class Robots : IRobotsParser
{
    /// <summary>
    ///     Gets the list of sitemaps declared in the file.
    /// </summary>

    //private readonly bool _malformed;
    //private readonly bool _isAnyPathDisallowed;
    //private readonly bool _hasRules;

    //List<Sitemap> sitemaps, bool malformed, 
    public Robots(List<AccessAllowRule> globalAccessRules, List<AccessAllowRule> specificAccessRules,
        List<CrawlDelayRule> crawlDelayRules, List<Sitemap> sitemaps, bool malformed, bool isAnyPathDisallowed,
        bool hasRules, AllowRuleImplementation allowRuleImplementation)
    {
        GlobalAccessRules = globalAccessRules;
        SpecificAccessRules = specificAccessRules;
        CrawlDelayRules = crawlDelayRules;
        Sitemaps = sitemaps;
        Malformed = malformed;
        IsAnyPathDisallowed = isAnyPathDisallowed;
        HasRules = hasRules;
        AllowRuleImplementation = allowRuleImplementation;
    }

    ///// <summary>
    ///// Gets the raw contents of the robots.txt file.
    ///// </summary>
    //public string Raw { get; private set; }

    public List<Sitemap> Sitemaps { get; set; }

    /// <summary>
    ///     Indicates whether the file has any lines which can't be understood.
    /// </summary>
    public bool Malformed { get; set; }

    /// <summary>
    ///     Indicates whether the file has any rules.
    /// </summary>
    public bool HasRules { get; set; }

    /// <summary>
    ///     Indicates whether there are any disallowed paths.
    /// </summary>
    public bool IsAnyPathDisallowed { get; set; }

    /// <summary>
    ///     How to support the Allow directive. Defaults to <see cref="RobotsTxt.Enums.AllowRuleImplementation.MoreSpecific" />
    ///     .
    /// </summary>
    public AllowRuleImplementation AllowRuleImplementation { get; set; }

    // We could just have a List<Rules>, since Rule is the base class for AccessRule & CrawlDelayRule... 
    // But IsPathAllowed() and CrawlDelay() functions need these specific collections everytime they're called, so
    // it saves us some time to have them pre-populated instead of extracting these lists from a List<Rule> everytime those functions are called.
    public List<AccessAllowRule> GlobalAccessRules { get; set; }
    public List<AccessAllowRule> SpecificAccessRules { get; set; }
    public List<CrawlDelayRule> CrawlDelayRules { get; set; }

    /// <summary>
    ///     Checks if the given user-agent can access the given relative path.
    /// </summary>
    /// <param name="userAgent">User agent string.</param>
    /// <param name="path">Relative path.</param>
    /// <exception cref="System.ArgumentException">
    ///     Thrown when userAgent parameter is null,
    ///     empty or consists only of white-space characters.
    /// </exception>
    public bool IsPathAllowed(string userAgent, string path)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
        {
            throw new ArgumentException("Not a valid user-agent string.", nameof(userAgent));
        }

        if (!HasRules || !IsAnyPathDisallowed)
        {
            return true;
        }

        path = NormalizePath(path);
        List<AccessAllowRule> rulesForThisRobot =
            SpecificAccessRules.FindAll(x => userAgent.Contains(x.For, StringComparison.InvariantCultureIgnoreCase));
        if (GlobalAccessRules.Count == 0 && rulesForThisRobot.Count == 0)
            // no rules for this robot
        {
            return true;
        }

        // If there are rules for this robot, we should only check against them. 
        // If not, we check against the global rules.
        // (though some robots ignore the rest after reading the rules for *)
        // We say "String.IsNullOrEmpty(x.Path)" while filtering because "Disallow: " means "Allow all".
        // And the reason we remove the first characters of the paths before calling IsPathMatch() is because the first characters will always be '/',
        // so there is no point having IsPathMatch() compare them.
        List<AccessAllowRule> matchingRules = rulesForThisRobot.Count > 0
            ? rulesForThisRobot.FindAll(x => string.IsNullOrEmpty(x.Path) || IsPathMatch(path[1..], x.Path[1..]))
            : GlobalAccessRules.FindAll(x => string.IsNullOrEmpty(x.Path) || IsPathMatch(path[1..], x.Path[1..]));

        if (matchingRules.Count == 0)
        {
            return true;
        }

        AccessAllowRule ruleToUse = AllowRuleImplementation == AllowRuleImplementation.MoreSpecific
            ? matchingRules.OrderByDescending(x => x.Path.Length).ThenBy(x => x.Order).First()
            : matchingRules.OrderBy(x => x.Order).First();

        return AllowRuleImplementation switch
        {
            AllowRuleImplementation.Standard => string.IsNullOrEmpty(ruleToUse.Path) || ruleToUse.Allowed,
            AllowRuleImplementation.AllowOverrides =>
                // check if there's any allow rule, if not follow the first disallow rule.
                // (again, "disallow:" means allow. which is why String.IsNullOrEmpty(ruleToUse.Path))
                matchingRules.Any(x => x.Allowed) || string.IsNullOrEmpty(ruleToUse.Path),
            AllowRuleImplementation.MoreSpecific => string.IsNullOrEmpty(ruleToUse.Path) || ruleToUse.Allowed,
            _ => false
        };
    }

    /// <summary>
    ///     Gets the number of milliseconds to wait between successive requests for this robot.
    /// </summary>
    /// <param name="userAgent">User agent string.</param>
    /// <returns>Returns zero if there's not any matching crawl-delay rules for this robot.</returns>
    /// <exception cref="System.ArgumentException">
    ///     Thrown when userAgent parameter is null,
    ///     empty or consists only of white-space characters.
    /// </exception>
    public long CrawlDelay(string userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
        {
            throw new ArgumentException("Not a valid user-agent string.", nameof(userAgent));
        }

        if (!HasRules || CrawlDelayRules.Count == 0)
        {
            return 0;
        }

        List<CrawlDelayRule> rulesForAllRobots =
            CrawlDelayRules.FindAll(x => x.For.Equals("*", StringComparison.Ordinal));
        List<CrawlDelayRule> rulesForThisRobot =
            CrawlDelayRules.FindAll(x => x.For.Contains(userAgent, StringComparison.InvariantCultureIgnoreCase));
        if (rulesForAllRobots.Count == 0 && rulesForThisRobot.Count == 0)
        {
            return 0;
        }

        return rulesForThisRobot.Count > 0 ? rulesForThisRobot[0].Delay : rulesForAllRobots[0].Delay;
    }

    private static bool IsPathMatch(string path, string rulePath)
    {
        int rulePathLength = rulePath.Length;
        for (int i = 0; i < rulePathLength; i++)
        {
            char c = rulePath[i];
            switch (c)
            {
                case '$' when i == rulePathLength - 1:
                    // If the '$' wildcard is the last character of the rulePath and if the path has one less character than rulePath,
                    // then it means the end of path matched the rulePath.
                    return i == path.Length;
                case '*' when i == rulePathLength - 1:
                    // Return true when '*' is the last char of rulePath because it doesn't matter what the rest of path is in this situation.
                    // (example : when rulePath is "/foo*" and path is "/foobar")
                    return true;
                case '*':
                    {
                        for (int j = i; j < path.Length; j++)
                            // When the '*' wildcard is not the last char,
                            // recursively call the method to see if the part of rulePath after '*' and the rest of the path matches.
                        {
                            if (IsPathMatch(path[j..], rulePath[(i + 1)..]))
                            {
                                return true;
                            }
                        }

                        // There's no match between the rest of the paths...
                        return false;
                    }
            }

            // When the char is not a wild card, check if path has any chars left to compare to.
            // And we return false when path has no more chars to compare to or if the comparison with the current char fails.
            if (i >= path.Length || !c.Equals(path[i]))
            {
                return false;
            }
        }

        // Ran out of rulePath characters... If the rest matches, that's good enough.
        // (example : when rulePath is "/foo/" and path is "/foo/bar")
        return path.StartsWith(rulePath, StringComparison.Ordinal);
    }

    private static string NormalizePath(string path)
    {
        const string uriPathDelimiter = "/";
        const string uriPathDelimiterDouble = "//";

        if (string.IsNullOrWhiteSpace(path))
        {
            path = uriPathDelimiter;
        }

        if (!path.StartsWith(uriPathDelimiter, StringComparison.Ordinal))
        {
            path = uriPathDelimiter + path;
        }

        while (path.Contains(uriPathDelimiterDouble))
        {
            path = path.Replace(uriPathDelimiterDouble, uriPathDelimiter);
        }

        return path;
    }
}
