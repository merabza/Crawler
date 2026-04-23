using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliParameters.FieldEditors;
using Crawler.Cruders;
using LibCrawlerRepositories;

namespace Crawler.FieldEditors;

public sealed class SchemeFieldEditor : FieldEditor<string>
{
    private readonly ICrawlerRepository _crawlerRepository;

    public SchemeFieldEditor(string propertyName, ICrawlerRepository crawlerRepository) : base(propertyName)
    {
        _crawlerRepository = crawlerRepository;
    }

    public override async ValueTask UpdateField(string? recordKey, object recordForUpdate,
        CancellationToken cancellationToken = default)
    {
        var schemeCruder = new SchemeCruder(_crawlerRepository);
        List<string> keys = schemeCruder.GetKeys();
        string? def = keys.Count > 1 ? null : schemeCruder.GetKeys().SingleOrDefault();
        SetValue(recordForUpdate,
            await schemeCruder.GetNameWithPossibleNewName(FieldName, GetValue(recordForUpdate, def), null, true,
                cancellationToken));
    }
}
