namespace DoCrawler.Models;

public sealed class UriTerm
{
    public UriTerm(ETermType termType)
    {
        TermType = termType;
    }

    public UriTerm(ETermType termType, string context)
    {
        TermType = termType;
        var contextRightLength = context;
        if (context.Length >= 50)
            contextRightLength = context[..50];
        Context = contextRightLength;
    }

    public ETermType TermType { get; set; }

    public string? Context { get; set; }
}