using System.Collections.Generic;
using System.Linq;
using AppCliTools.CliParameters.FieldEditors;
using Crawler.Cruders;
using LibCrawlerRepositories;

namespace Crawler.FieldEditors;

public sealed class SchemeFieldEditor : FieldEditor<string>
{
    private readonly ICrawlerRepositoryCreatorFactory _crawlerRepositoryCreatorFactory;

    public SchemeFieldEditor(string propertyName, ICrawlerRepositoryCreatorFactory crawlerRepositoryCreatorFactory) :
        base(propertyName)
    {
        _crawlerRepositoryCreatorFactory = crawlerRepositoryCreatorFactory;
    }

    public override void UpdateField(string? recordKey, object recordForUpdate) //, object currentRecord
    {
        var schemeCruder = new SchemeCruder(_crawlerRepositoryCreatorFactory);
        List<string> keys = schemeCruder.GetKeys();
        string? def = keys.Count > 1 ? null : schemeCruder.GetKeys().SingleOrDefault();
        SetValue(recordForUpdate,
            schemeCruder.GetNameWithPossibleNewName(FieldName, GetValue(recordForUpdate, def), null, true));
    }
}
