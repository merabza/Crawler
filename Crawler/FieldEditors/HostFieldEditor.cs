//using System.Collections.Generic;
//using System.Linq;
//using CliParameters.FieldEditors;
//using Crawler.Cruders;
//using LibCrawlerRepositories;

//namespace Crawler.FieldEditors;

//public sealed class HostFieldEditor : FieldEditor<string>
//{
//    //private readonly ParametersManager _parametersManager;
//    private readonly ICrawlerRepositoryCreatorFactory _crawlerRepositoryCreatorFactory;

//    public HostFieldEditor(string propertyName, ICrawlerRepositoryCreatorFactory crawlerRepositoryCreatorFactory) :
//        base(propertyName)
//    {
//        //_parametersManager = parametersManager;
//        _crawlerRepositoryCreatorFactory = crawlerRepositoryCreatorFactory;
//    }

//    public override void UpdateField(string? recordKey, object recordForUpdate) //, object currentRecord
//    {
//        HostCruder hostCruder = new HostCruder(_crawlerRepositoryCreatorFactory);
//        List<string> keys = hostCruder.GetKeys();
//        string? def = keys.Count > 1 ? null : hostCruder.GetKeys().SingleOrDefault();
//        SetValue(recordForUpdate,
//            hostCruder.GetNameWithPossibleNewName(FieldName, GetValue(recordForUpdate, def), null, true));
//    }

//}


