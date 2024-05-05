using CrawlerDb.Configurations;

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
        Context = context.Truncate(TermConfiguration.TermTextLength);
    }

    public ETermType TermType { get; set; }

    public string? Context { get; set; }
}