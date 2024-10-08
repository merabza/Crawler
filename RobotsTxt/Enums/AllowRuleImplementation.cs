﻿namespace RobotsTxt.Enums;

public enum AllowRuleImplementation
{
    /// <summary>
    ///     First matching rule will win.
    /// </summary>
    Standard,

    /// <summary>
    ///     Disallow rules will only be checked if no allow rule matches.
    /// </summary>
    AllowOverrides,

    /// <summary>
    ///     The more specific (the longer) rule will apply.
    /// </summary>
    MoreSpecific
}