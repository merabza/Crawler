//Created by FakeProjectDesignTimeDbContextFactoryCreator at 8/2/2025 5:16:00 PM

using CrawlerDb;

namespace FakeHost;

//ეს კლასი საჭიროა იმისათვის, რომ შესაძლებელი გახდეს მიგრაციასთან მუშაობა.
//ანუ დეველოპერ ბაზის წაშლა და ახლიდან დაგენერირება, ან მიგრაციაში ცვლილებების გაკეთება
// ReSharper disable once UnusedType.Global
public sealed class CrawlerDbDesignTimeDbContextFactory : DesignTimeDbContextFactory<CrawlerDbContext>
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public CrawlerDbDesignTimeDbContextFactory() : base("CrawlerDbMigration", "ConnectionStringSeed",
        @"D:\1WorkSecurity\Crawler\FakeHost.json")
    {
    }
}
