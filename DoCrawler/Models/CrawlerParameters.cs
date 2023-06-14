//Created by ProjectParametersClassCreator at 4/22/2021 17:17:01

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using DbTools;
using LibParameters;

namespace DoCrawler.Models;

public sealed class CrawlerParameters : IParameters
{
    private string _possibleSymbols = "";

    private string _punctuationRegex = "";

    private string _segmentFinisherPunctuationRegex = "";

    private string _wordDelimiterRegex = "";

    public string? LogFolder { get; set; }

    //public string? LogFileName { get; set; }
    public EDataProvider DataProvider { get; set; }
    public string? ConnectionString { get; set; }
    public int CommandTimeOut { get; set; }
    public int LoadPagesMaxCount { get; set; }
    public string? Alphabet { get; set; }
    public string? ExtraSymbols { get; set; }

    public Dictionary<string, TaskModel> Tasks { get; set; } = new();
    public Dictionary<string, PunctuationModel> Punctuations { get; set; } = new();

    public bool CheckBeforeSave()
    {
        return true;
    }

    public TaskModel? GetTask(string taskName)
    {
        return !Tasks.ContainsKey(taskName) ? null : Tasks[taskName];
    }

    public bool CheckNewTaskNameValid(string oldTaskName, string newTaskName)
    {
        if (oldTaskName == newTaskName)
            return true;

        if (!Tasks.ContainsKey(oldTaskName))
            return false;

        return !Tasks.ContainsKey(newTaskName);
    }

    public bool RemoveTask(string taskName)
    {
        if (!Tasks.ContainsKey(taskName))
            return false;
        Tasks.Remove(taskName);
        return true;
    }

    public bool AddTask(string newTaskName, TaskModel task)
    {
        if (Tasks.ContainsKey(newTaskName))
            return false;
        Tasks.Add(newTaskName, task);
        return true;
    }

    public string GetSegmentFinisherPunctuationsRegex()
    {
        if (_segmentFinisherPunctuationRegex == "")
            _segmentFinisherPunctuationRegex =
                GetPunctuationRegex(Punctuations.Where(s => s.Value.PctSentenceFinisher));
        return _segmentFinisherPunctuationRegex;
    }

    private static string GetPunctuationRegex(IEnumerable<KeyValuePair<string, PunctuationModel>> punctuations)
    {
        var rex = "";
        foreach (var pun in punctuations.OrderBy(s => s.Value.PctSortId))
        {
            if (rex != "")
                rex += "|";
            if (pun.Value.PctRegexPattern == null)
                rex += "(" + pun.Value.PctPunctuation + ")";
            else
                rex += "(" + pun.Value.PctRegexPattern + ")";
        }

        //if (rex != "")
        //  rex = "([" + rex + "])";
        return rex;
    }

    internal string GetPossibleSymbols()
    {
        if (_possibleSymbols != "") return _possibleSymbols;
        StringBuilder sb = new();
        //შეგროვდეს პუნქტუაციის ნიშნებში მონაწილე სიმბოლოები, ისე რომ ერთნაირი სიმბოლოები არ გამეორდეს
        foreach (var pun in Punctuations)
            sb.Append(pun.Value.PctPunctuation);
        //დაემატოს ციფრები
        for (var i = 0; i < 10; i++)
            sb.Append(i.ToString(CultureInfo.InvariantCulture));
        //დაემატოს ანბანი
        sb.Append(Alphabet);
        //დაემატოს პრობელი
        sb.Append(' ');
        _possibleSymbols = new string(sb.ToString().Distinct().ToArray());
        return _possibleSymbols;
    }

    internal string GetPunctuationsRegex()
    {
        if (_punctuationRegex == "")
            _punctuationRegex = GetPunctuationRegex(Punctuations);
        return _punctuationRegex;
    }

    internal string GetWordDelimiterRegex()
    {
        if (_wordDelimiterRegex == "")
            _wordDelimiterRegex = "({" + GetPunctuationRegex(Punctuations.Where(s => !s.Value.PctCanBePartOfWord)) +
                                  "|(\\s)|(\\n)})";
        return _wordDelimiterRegex;
    }
}