using CrawlerDb;

namespace Crawler;

//ეს კლასი საჭიროა იმისათვის, რომ შესაძლებელი გახდეს მიგრაციასთან მუშაობა.
//ანუ დეველოპერ ბაზის წაშლა და ახლიდან დაგენერირება, ან მიგრაციაში ცვლილებების გაკეთება
public sealed class CrawlerDesignTimeDbContextFactory : DesignTimeDbContextFactory<CrawlerDbContext>
{
    public CrawlerDesignTimeDbContextFactory() : base("CrawlerDbMigration", "ConnectionString",
        "D:\\1WorkSecurity\\Crawler\\CrawlerDb.json")
    {
    }
}