//using System.Collections.Generic;
//using System.Linq;
//using CliMenu;
//using CliParameters.FieldEditors;
//using DoCrawler.Cruders;
//using DoCrawler.Models;
//using LibParameters;
//using Microsoft.Extensions.Logging;

//namespace Crawler.FieldEditors;

//public sealed class PunctuationsFieldEditor : FieldEditor<Dictionary<string, PunctuationModel>>
//{
//    private readonly ILogger _logger;
//    private readonly ParametersManager _parametersManager;

//    public PunctuationsFieldEditor(string propertyName, ParametersManager parametersManager, ILogger logger) : base(
//        propertyName, true, null, true)
//    {
//        _parametersManager = parametersManager;
//        _logger = logger;
//    }

//    public override CliMenuSet GetSubMenu(object record)
//    {
//        PunctuationCruder punctuationCruder = new(_parametersManager, _logger);
//        var menuSet = punctuationCruder.GetListMenu();
//        return menuSet;
//    }

//    public override string GetValueStatus(object? record)
//    {
//        var val = GetValue(record);

//        if (val is null || val.Count <= 0)
//            return "No Details";

//        if (val.Count > 1)
//            return $"{val.Count} Details";

//        var kvp = val.Single();
//        return $"{kvp.Key} - {kvp.Value.PctName}";
//    }
//}