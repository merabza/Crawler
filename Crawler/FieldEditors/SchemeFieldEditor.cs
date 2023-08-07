using System.Linq;
using CliParameters.FieldEditors;
using Crawler.Cruders;
using LibCrawlerRepositories;

namespace Crawler.FieldEditors;

public sealed class SchemeFieldEditor : FieldEditor<string>
{
    private readonly ICrawlerRepositoryCreatorFabric _crawlerRepositoryCreatorFabric;

    public SchemeFieldEditor(string propertyName, ICrawlerRepositoryCreatorFabric crawlerRepositoryCreatorFabric) :
        base(propertyName)
    {
        _crawlerRepositoryCreatorFabric = crawlerRepositoryCreatorFabric;
    }

    public override void UpdateField(string? recordKey, object recordForUpdate) //, object currentRecord
    {
        SchemeCruder schemeCruder = new(_crawlerRepositoryCreatorFabric);
        var keys = schemeCruder.GetKeys();
        var def = keys.Count > 1 ? null : schemeCruder.GetKeys().SingleOrDefault();
        SetValue(recordForUpdate,
            schemeCruder.GetNameWithPossibleNewName(FieldName, GetValue(recordForUpdate, def), null, true));
    }
}