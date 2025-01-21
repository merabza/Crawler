using System;
using System.Collections.Generic;
using System.Linq;
using CliParameters;
using CliParameters.FieldEditors;
using DoCrawler.Models;
using LibParameters;
using Microsoft.Extensions.Logging;

namespace DoCrawler.Cruders;

public sealed class PunctuationCruder : ParCruder
{
    private readonly ILogger _logger;

    public PunctuationCruder(ParametersManager parametersManager, ILogger logger) : base(parametersManager,
        "Punctuation", "Punctuations")
    {
        _logger = logger;
        //FieldEditors.Add(new TextFieldEditor(nameof(PunctuationModel.PctKey)));
        FieldEditors.Add(new TextFieldEditor(nameof(PunctuationModel.PctName)));
        FieldEditors.Add(new TextFieldEditor(nameof(PunctuationModel.PctPunctuation)));
        FieldEditors.Add(new TextFieldEditor(nameof(PunctuationModel.PctRegexPattern)));
        FieldEditors.Add(new IntFieldEditor(nameof(PunctuationModel.PctSortId)));
        FieldEditors.Add(new BoolFieldEditor(nameof(PunctuationModel.PctSentenceFinisher), false));
        FieldEditors.Add(new BoolFieldEditor(nameof(PunctuationModel.PctCanBePartOfWord), false));
    }

    protected override Dictionary<string, ItemData> GetCrudersDictionary()
    {
        var parameters = (CrawlerParameters)ParametersManager.Parameters;
        return parameters.Punctuations.ToDictionary(p => p.Key, p => (ItemData)p.Value);
    }

    public override bool ContainsRecordWithKey(string recordKey)
    {
        var parameters = (CrawlerParameters)ParametersManager.Parameters;
        var punctuations = parameters.Punctuations;
        return punctuations.ContainsKey(recordKey);
    }

    public override void UpdateRecordWithKey(string recordKey, ItemData newRecord)
    {
        if (newRecord is not PunctuationModel newPunctuation)
            throw new ArgumentNullException(nameof(newPunctuation));
        var parameters = (CrawlerParameters)ParametersManager.Parameters;
        parameters.Punctuations[recordKey] = newPunctuation;
    }

    protected override void AddRecordWithKey(string recordKey, ItemData newRecord)
    {
        if (newRecord is not PunctuationModel punctuation)
            throw new ArgumentNullException(nameof(punctuation));
        var parameters = (CrawlerParameters)ParametersManager.Parameters;
        parameters.Punctuations.Add(recordKey, punctuation);
    }

    protected override void RemoveRecordWithKey(string recordKey)
    {
        var parameters = (CrawlerParameters)ParametersManager.Parameters;
        var punctuations = parameters.Punctuations;
        punctuations.Remove(recordKey);
    }

    public override bool CheckValidation(ItemData item)
    {
        try
        {
            return item is PunctuationModel;
        }
        catch (Exception e)
        {
            _logger.LogError(e, null);
            return false;
        }
    }

    public override string GetStatusFor(string name)
    {
        var punctuationModel = (PunctuationModel?)GetItemByName(name);
        return punctuationModel == null
            ? "(Empty)"
            : $"{punctuationModel.PctName} -> {punctuationModel.PctPunctuation}";
    }

    protected override ItemData CreateNewItem(string? recordKey, ItemData? defaultItemData)
    {
        return new PunctuationModel();
    }
}