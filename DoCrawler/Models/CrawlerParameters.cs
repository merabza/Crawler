//Created by ProjectParametersClassCreator at 4/22/2021 17:17:01

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using LibApiClientParameters;
using LibDatabaseParameters;
using LibFileParameters.Interfaces;
using LibFileParameters.Models;

namespace DoCrawler.Models;

public sealed class CrawlerParameters : IParametersWithDatabaseServerConnections, IParametersWithApiClients, IParametersWithSmartSchemas
{
    private string _possibleSymbols = string.Empty;

    private string _punctuationRegex = string.Empty;

    private string _segmentFinisherPunctuationRegex = string.Empty;

    private string _wordDelimiterRegex = string.Empty;

    public string? LogFolder { get; set; }

    public string? DatabaseConnectionName { get; set; }

    public int CommandTimeOut { get; set; }
    public int LoadPagesMaxCount { get; set; }
    public string? Alphabet { get; set; }
    public string? ExtraSymbols { get; set; }

    public Dictionary<string, TaskModel> Tasks { get; set; } = [];
    public Dictionary<string, PunctuationModel> Punctuations { get; init; } = [];
    public DatabaseParameters? DatabaseParameters { get; init; }
    public Dictionary<string, ApiClientSettings> ApiClients { get; } = [];
    public Dictionary<string, DatabaseServerConnectionData> DatabaseServerConnections { get; init; } = [];
    public Dictionary<string, SmartSchema> SmartSchemas { get; } = [];

    public bool CheckBeforeSave()
    {
        return true;
    }

    public TaskModel? GetTask(string taskName)
    {
        return Tasks.GetValueOrDefault(taskName);
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
        return Tasks.Remove(taskName);
    }

    public bool AddTask(string newTaskName, TaskModel task)
    {
        return Tasks.TryAdd(newTaskName, task);
    }

    public string GetSegmentFinisherPunctuationsRegex()
    {
        if (_segmentFinisherPunctuationRegex == string.Empty)
            _segmentFinisherPunctuationRegex =
                GetPunctuationRegex(Punctuations.Where(s => s.Value.PctSentenceFinisher));
        return _segmentFinisherPunctuationRegex;
    }

    private static string GetPunctuationRegex(IEnumerable<KeyValuePair<string, PunctuationModel>> punctuations)
    {
        var rex = string.Empty;
        foreach (var pun in punctuations.OrderBy(s => s.Value.PctSortId))
        {
            if (rex != string.Empty)
                rex += "|";
            if (pun.Value.PctRegexPattern == null)
                rex += "(" + pun.Value.PctPunctuation + ")";
            else
                rex += "(" + pun.Value.PctRegexPattern + ")";
        }

        //if (rex != string.Empty)
        //  rex = "([" + rex + "])";
        return rex;
    }

    internal string GetPossibleSymbols()
    {
        if (_possibleSymbols != string.Empty)
            return _possibleSymbols;
        var sb = new StringBuilder();
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
        if (_punctuationRegex == string.Empty)
            _punctuationRegex = GetPunctuationRegex(Punctuations);
        return _punctuationRegex;
    }

    internal string GetWordDelimiterRegex()
    {
        if (_wordDelimiterRegex == string.Empty)
            _wordDelimiterRegex = "({" + GetPunctuationRegex(Punctuations.Where(s => !s.Value.PctCanBePartOfWord)) +
                                  @"|(\s)|(\n)})";
        return _wordDelimiterRegex;
    }

}