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
        Context = context;
    }

    public ETermType TermType { get; set; }

    public string? Context { get; set; }
    //public int UrlId { get; set; }
    //public int BatchPartId { get; set; }
}