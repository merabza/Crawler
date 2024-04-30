namespace RobotsTxt.Entities;

public abstract class Rule
{
    public string For { get; private set; }
    public int Order { get; private set; }

    // ReSharper disable once ConvertToPrimaryConstructor
    protected Rule(string userAgent, int order)
    {
        For = userAgent;
        Order = order;
    }
}