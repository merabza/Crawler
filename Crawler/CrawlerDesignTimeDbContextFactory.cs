using CrawlerDb;

namespace Crawler;

//ეს კლასი საჭიროა იმისათვის, რომ შესაძლებელი გახდეს მიგრაციასთან მუშაობა.
//ანუ დეველოპერ ბაზის წაშლა და ახლიდან დაგენერირება, ან მიგრაციაში ცვლილებების გაკეთება
// ReSharper disable once UnusedType.Global
public sealed class CrawlerDesignTimeDbContextFactory : DesignTimeDbContextFactory<CrawlerDbContext>
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public CrawlerDesignTimeDbContextFactory() : base("CrawlerDbMigration", "ConnectionString",
        @"D:\1WorkSecurity\Crawler\CrawlerDb.json")
    {
    }
}