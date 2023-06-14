//using System;
//using System.Collections.Generic;
//using System.Linq;
//using CliMenu;
//using CliParameters;
//using Crawler.MenuCommands;
//using DoCrawler.Models;
//using LibCrawlerRepositories;
//using Microsoft.Extensions.Logging;

//namespace Crawler.Cruders;

//public sealed class CrawlerTaskCruder : ParCruder
//{

//    private readonly ILogger _logger;
//    private readonly ICrawlerRepositoryCreatorFabric _crawlerRepositoryCreatorFabric;

//    public CrawlerTaskCruder(ILogger logger, ParametersManager parametersManager,
//        ICrawlerRepositoryCreatorFabric crawlerRepositoryCreatorFabric) : base(parametersManager, "Crawler Task",
//        "Crawler Tasks")
//    {
//        _logger = logger;
//        _crawlerRepositoryCreatorFabric = crawlerRepositoryCreatorFabric;
//        //FieldEditors.Add(new SchemeFieldEditor(nameof(TaskModel.SchemeName), crawlerRepositoryCreatorFabric));
//        //FieldEditors.Add(new HostFieldEditor(nameof(TaskModel.HostName), crawlerRepositoryCreatorFabric));
//    }

//    protected override Dictionary<string, ItemData> GetCrudersDictionary()
//    {
//        CrawlerParameters parameters = (CrawlerParameters) ParametersManager.Parameters;
//        return parameters.Tasks.ToDictionary(p => p.Key, p => (ItemData) p.Value);
//    }

//    public override bool ContainsRecordWithKey(string recordKey)
//    {
//        CrawlerParameters parameters = (CrawlerParameters) ParametersManager.Parameters;
//        Dictionary<string, TaskModel> tasks = parameters.Tasks;
//        return tasks.ContainsKey(recordKey);
//    }

//    public override void UpdateRecordWithKey(string recordName, ItemData newRecord)
//    {
//        TaskModel? newJobSchedule = newRecord as TaskModel;
//        if (newJobSchedule is null)
//            throw new Exception("newJobSchedule is null");
//        CrawlerParameters parameters = (CrawlerParameters) ParametersManager.Parameters;
//        parameters.Tasks[recordName] = newJobSchedule;
//    }

//    protected override void AddRecordWithKey(string recordName, ItemData newRecord)
//    {
//        TaskModel? newJobSchedule = newRecord as TaskModel;
//        if (newJobSchedule is null)
//            throw new Exception("newJobSchedule is null");
//        CrawlerParameters parameters = (CrawlerParameters) ParametersManager.Parameters;
//        parameters.Tasks.Add(recordName, newJobSchedule);

//    }

//    protected override void RemoveRecordWithKey(string recordKey)
//    {
//        CrawlerParameters parameters = (CrawlerParameters) ParametersManager.Parameters;
//        Dictionary<string, TaskModel> jobSchedules = parameters.Tasks;
//        jobSchedules.Remove(recordKey);
//    }

//    protected override ItemData CreateNewItem(string recordName, ItemData? defaultItemData)
//    {
//        return new TaskModel();
//    }

//    public override void FillDetailsSubMenu(CliMenuSet itemSubMenuSet, string recordName)
//    {

//        base.FillDetailsSubMenu(itemSubMenuSet, recordName);
//        itemSubMenuSet.AddMenuItem(
//            new TaskCommand(_logger, _crawlerRepositoryCreatorFabric, ParametersManager, recordName),
//            "Run this task");

//    }

//}

