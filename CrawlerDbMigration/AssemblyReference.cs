using System.Reflection;

namespace CrawlerDbMigration;

public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
