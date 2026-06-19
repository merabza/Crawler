using System;
using System.Collections.Generic;
using AppCliTools.CliParameters;
using AppCliTools.CliParameters.FieldEditors;
using DoCrawler.Models;
using Microsoft.Extensions.Logging;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared;

namespace DoCrawler.Cruders;

public sealed class PunctuationCruder : ParCruder<PunctuationModel>
{
    private readonly ILogger _logger;

    public PunctuationCruder(ILogger logger, IParametersManager parametersManager,
        Dictionary<string, PunctuationModel> currentValuesDictionary) : base(parametersManager, currentValuesDictionary,
        "Punctuation", "Punctuations")
    {
        _logger = logger;
        FieldEditors.Add(new TextFieldEditor(nameof(PunctuationModel.PctName)));
        FieldEditors.Add(new TextFieldEditor(nameof(PunctuationModel.PctPunctuation)));
        FieldEditors.Add(new TextFieldEditor(nameof(PunctuationModel.PctRegexPattern)));
        FieldEditors.Add(new IntFieldEditor(nameof(PunctuationModel.PctSortId)));
        FieldEditors.Add(new BoolFieldEditor(nameof(PunctuationModel.PctSentenceFinisher)));
        FieldEditors.Add(new BoolFieldEditor(nameof(PunctuationModel.PctCanBePartOfWord)));
    }

    public override bool CheckValidation(ItemData item)
    {
        try
        {
            return item is PunctuationModel;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception occurred during validation in {MethodName}", nameof(CheckValidation));
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
}
