//using System.Collections.Generic;
//using System.Linq;
//using CliParameters.FieldEditors;
//using Crawler.Cruders;
//using LibCrawlerRepositories;

//namespace Crawler.FieldEditors;

//public sealed class HostFieldEditor : FieldEditor<string>
//{
//    //private readonly ParametersManager _parametersManager;
//    private readonly ICrawlerRepositoryCreatorFabric _crawlerRepositoryCreatorFabric;

//    public HostFieldEditor(string propertyName, ICrawlerRepositoryCreatorFabric crawlerRepositoryCreatorFabric) :
//        base(propertyName)
//    {
//        //_parametersManager = parametersManager;
//        _crawlerRepositoryCreatorFabric = crawlerRepositoryCreatorFabric;
//    }

//    public override void UpdateField(string? recordKey, object recordForUpdate) //, object currentRecord
//    {
//        HostCruder hostCruder = new HostCruder(_crawlerRepositoryCreatorFabric);
//        List<string> keys = hostCruder.GetKeys();
//        string? def = keys.Count > 1 ? null : hostCruder.GetKeys().SingleOrDefault();
//        SetValue(recordForUpdate,
//            hostCruder.GetNameWithPossibleNewName(FieldName, GetValue(recordForUpdate, def), null, true));
//    }

//}

