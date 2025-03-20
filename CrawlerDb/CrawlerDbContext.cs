using CrawlerDb.Models;
using DatabaseToolsShared;
using Microsoft.EntityFrameworkCore;

namespace CrawlerDb;

public sealed class CrawlerDbContext : DbContext
{
    public CrawlerDbContext(DbContextOptions options, bool isDesignTime) : base(options)
    {
    }

    public CrawlerDbContext(DbContextOptions<CrawlerDbContext> options) : base(options)
    {
    }

    //public CrawlerDbContext(DbContextOptions<CrawlerDbContext> options, bool isDesignTime): base(options)
    //{

    //}

    public DbSet<Batch> Batches => Set<Batch>();
    public DbSet<BatchPart> BatchParts => Set<BatchPart>();
    public DbSet<ContentAnalysis> ContentsAnalysis => Set<ContentAnalysis>();
    public DbSet<ExtensionModel> Extensions => Set<ExtensionModel>();
    public DbSet<HostByBatch> HostsByBatches => Set<HostByBatch>();
    public DbSet<HostModel> Hosts => Set<HostModel>();
    public DbSet<Robot> Robots => Set<Robot>();
    public DbSet<SchemeModel> Schemes => Set<SchemeModel>();
    public DbSet<Term> Terms => Set<Term>();
    public DbSet<TermByUrl> TermsByUrls => Set<TermByUrl>();
    public DbSet<TermType> TermTypes => Set<TermType>();
    public DbSet<UrlGraphNode> UrlGraphNodes => Set<UrlGraphNode>();

    public DbSet<UrlModel> Urls => Set<UrlModel>();
    //public DbSet<UrlAllowModel> UrlAllows => Set<UrlAllowModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(AssemblyReference.Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Conventions.Add(_ => new DatabaseEntitiesDefaultConvention());
    }
}