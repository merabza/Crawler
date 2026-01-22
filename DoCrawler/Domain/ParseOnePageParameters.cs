using DoCrawler.Models;
using SystemTools.SystemToolsShared;

namespace DoCrawler.Domain;

public sealed class ParseOnePageParameters
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public ParseOnePageParameters(string alphabet, string segmentFinisherPunctuationsRegex, string punctuationsRegex,
        string wordDelimiterRegex)
    {
        Alphabet = alphabet;
        SegmentFinisherPunctuationsRegex = segmentFinisherPunctuationsRegex;
        PunctuationsRegex = punctuationsRegex;
        WordDelimiterRegex = wordDelimiterRegex;
    }

    public string Alphabet { get; set; }
    public string SegmentFinisherPunctuationsRegex { get; set; }
    public string PunctuationsRegex { get; set; }
    public string WordDelimiterRegex { get; set; }

    public static ParseOnePageParameters? Create(CrawlerParameters par)
    {
        if (string.IsNullOrWhiteSpace(par.Alphabet))
        {
            StShared.WriteErrorLine("Alphabet does not specified in parameters", true);
            return null;
        }

        string segmentFinisherPunctuationsRegex = par.GetSegmentFinisherPunctuationsRegex();
        if (string.IsNullOrWhiteSpace(segmentFinisherPunctuationsRegex))
        {
            StShared.WriteErrorLine("segmentFinisherPunctuationsRegex cannot get from parameters", true);
            return null;
        }

        string punctuationsRegex = par.GetPunctuationsRegex();
        if (string.IsNullOrWhiteSpace(punctuationsRegex))
        {
            StShared.WriteErrorLine("punctuationsRegex cannot get from parameters", true);
            return null;
        }

        string wordDelimiterRegex = par.GetWordDelimiterRegex();
        if (string.IsNullOrWhiteSpace(wordDelimiterRegex))
        {
            StShared.WriteErrorLine("wordDelimiterRegex cannot get from parameters", true);
            return null;
        }

        return new ParseOnePageParameters(par.Alphabet, segmentFinisherPunctuationsRegex, punctuationsRegex,
            wordDelimiterRegex);
    }
}
