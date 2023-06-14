using CrawlerDb.Models;
using Microsoft.EntityFrameworkCore;
using SystemToolsShared;

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
    public DbSet<SchemeModel> Schemes => Set<SchemeModel>();
    public DbSet<Term> Terms => Set<Term>();
    public DbSet<TermByUrl> TermsByUrls => Set<TermByUrl>();
    public DbSet<TermType> TermTypes => Set<TermType>();
    public DbSet<UrlGraphNode> UrlGraphNodes => Set<UrlGraphNode>();
    public DbSet<UrlModel> Urls => Set<UrlModel>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Batch>(entity =>
        {
            var tableName = nameof(Batch).Pluralize();

            entity.HasKey(e => e.BatchId);
            entity.ToTable(tableName.UnCapitalize());
            entity.HasIndex(e => e.BatchName)
                .HasDatabaseName($"IX_{tableName}_{nameof(Batch.BatchName).UnCapitalize()}")
                .IsUnique();
            entity.Property(e => e.BatchId).HasColumnName(nameof(Batch.BatchId).UnCapitalize());
            entity.Property(e => e.BatchName).HasColumnName(nameof(Batch.BatchName).UnCapitalize()).HasMaxLength(50);
            entity.Property(e => e.IsOpen).HasColumnName(nameof(Batch.IsOpen).UnCapitalize()).HasDefaultValue(0);
            entity.Property(e => e.AutoCreateNextPart).HasColumnName(nameof(Batch.AutoCreateNextPart).UnCapitalize())
                .HasDefaultValue(0);
        });

        modelBuilder.Entity<BatchPart>(entity =>
        {
            var tableName = nameof(BatchPart).Pluralize();

            entity.HasKey(e => e.BpId);
            entity.ToTable(tableName.UnCapitalize());
            entity.HasIndex(e => new { e.BatchId, e.Created }).HasDatabaseName($"IX_{tableName}_unique").IsUnique();
            entity.Property(e => e.BpId).HasColumnName(nameof(BatchPart.BpId).UnCapitalize());
            entity.Property(e => e.BatchId).HasColumnName(nameof(BatchPart.BatchId).UnCapitalize());
            entity.Property(e => e.Created).HasColumnName(nameof(BatchPart.Created).UnCapitalize());
            entity.Property(e => e.Finished).HasColumnName(nameof(BatchPart.Finished).UnCapitalize());

            entity.HasOne(d => d.BatchNavigation).WithMany(p => p.BatchParts)
                .HasForeignKey(d => d.BatchId).HasConstraintName($"FK_{tableName}_{nameof(Batch).Pluralize()}");
        });

        modelBuilder.Entity<ContentAnalysis>(entity =>
        {
            var tableName = nameof(ContentAnalysis).Pluralize();

            entity.HasKey(e => e.CaId);
            entity.ToTable(tableName.UnCapitalize());
            entity.HasIndex(e => new { e.BatchPartId, e.UrlId }).HasDatabaseName($"IX_{tableName}_unique").IsUnique();
            entity.Property(e => e.CaId).HasColumnName(nameof(ContentAnalysis.CaId).UnCapitalize());
            entity.Property(e => e.BatchPartId).HasColumnName(nameof(ContentAnalysis.BatchPartId).UnCapitalize());
            entity.Property(e => e.UrlId).HasColumnName(nameof(ContentAnalysis.UrlId).UnCapitalize());
            entity.Property(e => e.ResponseStatusCode)
                .HasColumnName(nameof(ContentAnalysis.ResponseStatusCode).UnCapitalize());
            entity.Property(e => e.Finish).HasColumnName(nameof(ContentAnalysis.Finish).UnCapitalize());

            entity.HasOne(d => d.BatchPartNavigation).WithMany(p => p.ContentsAnalysis)
                .HasForeignKey(d => d.BatchPartId).HasConstraintName($"FK_{tableName}_{nameof(BatchPart).Pluralize()}");

            entity.HasOne(d => d.UrlNavigation).WithMany(p => p.ContentsAnalysis)
                .HasForeignKey(d => d.UrlId).HasConstraintName($"FK_{tableName}_urls");
        });

        modelBuilder.Entity<ExtensionModel>(entity =>
        {
            var tableName = "Extensions";

            entity.HasKey(e => e.ExtId);
            entity.ToTable(tableName.UnCapitalize());
            entity.HasIndex(e => e.ExtName)
                .HasDatabaseName($"IX_{tableName}_{nameof(ExtensionModel.ExtName).UnCapitalize()}")
                .IsUnique();
            entity.Property(e => e.ExtId).HasColumnName(nameof(ExtensionModel.ExtId).UnCapitalize());
            entity.Property(e => e.ExtName).HasColumnName(nameof(ExtensionModel.ExtName).UnCapitalize())
                .HasMaxLength(50);
            entity.Property(e => e.ExtProhibited).HasColumnName(nameof(ExtensionModel.ExtProhibited).UnCapitalize())
                .HasDefaultValue(0);
        });

        modelBuilder.Entity<HostByBatch>(entity =>
        {
            var tableName = "HostsByBatches";

            entity.HasKey(e => e.HbbId);
            entity.ToTable(tableName.UnCapitalize());

            entity.HasIndex(e => new { e.BatchId, e.SchemeId, e.HostId }).HasDatabaseName($"IX_{tableName}_unique")
                .IsUnique();

            entity.Property(e => e.HbbId).HasColumnName(nameof(HostByBatch.HbbId).UnCapitalize());
            entity.Property(e => e.BatchId).HasColumnName(nameof(HostByBatch.BatchId).UnCapitalize()).HasMaxLength(50);
            entity.Property(e => e.HostId).HasColumnName(nameof(HostByBatch.HostId).UnCapitalize());
            entity.Property(e => e.SchemeId).HasColumnName(nameof(HostByBatch.SchemeId).UnCapitalize());
            //entity.Property(e => e.SiteMapUrl).HasColumnName(nameof(HostByBatch.SiteMapUrl).UnCapitalize()).HasMaxLength(2048);

            entity.HasOne(d => d.BatchNavigation).WithMany(p => p.HostsByBatches)
                .HasForeignKey(d => d.BatchId).HasConstraintName($"FK_{tableName}_{nameof(Batch).Pluralize()}");

            entity.HasOne(d => d.SchemeNavigation).WithMany(p => p.HostsByBatches)
                .HasForeignKey(d => d.SchemeId).HasConstraintName($"FK_{tableName}_Schemes");

            entity.HasOne(d => d.HostNavigation).WithMany(p => p.HostsByBatches)
                .HasForeignKey(d => d.HostId).HasConstraintName($"FK_{tableName}_Hosts");
        });

        modelBuilder.Entity<HostModel>(entity =>
        {
            var tableName = "Hosts";

            entity.HasKey(e => e.HostId);
            entity.ToTable(tableName.UnCapitalize());
            entity.HasIndex(e => e.HostName)
                .HasDatabaseName($"IX_{tableName}_{nameof(HostModel.HostName).UnCapitalize()}")
                .IsUnique();
            entity.Property(e => e.HostId).HasColumnName(nameof(HostModel.HostId).UnCapitalize());
            entity.Property(e => e.HostName).HasColumnName(nameof(HostModel.HostName).UnCapitalize()).HasMaxLength(50);
            entity.Property(e => e.HostProhibited).HasColumnName(nameof(HostModel.HostProhibited).UnCapitalize())
                .HasDefaultValue(0);
        });

        modelBuilder.Entity<SchemeModel>(entity =>
        {
            var tableName = "Schemes";

            entity.HasKey(e => e.SchId);
            entity.ToTable(tableName.UnCapitalize());
            entity.HasIndex(e => e.SchName)
                .HasDatabaseName($"IX_{tableName}_{nameof(SchemeModel.SchName).UnCapitalize()}")
                .IsUnique();
            entity.Property(e => e.SchId).HasColumnName(nameof(SchemeModel.SchId).UnCapitalize());
            entity.Property(e => e.SchName).HasColumnName(nameof(SchemeModel.SchName).UnCapitalize()).HasMaxLength(50);
            entity.Property(e => e.SchProhibited).HasColumnName(nameof(SchemeModel.SchProhibited).UnCapitalize())
                .HasDefaultValue(0);
        });

        modelBuilder.Entity<Term>(entity =>
        {
            var tableName = nameof(Term).Pluralize();

            entity.HasKey(e => e.TrmId);
            entity.ToTable(tableName.UnCapitalize());
            entity.HasIndex(e => e.TermText).HasDatabaseName($"IX_{tableName}_{nameof(Term.TermText).UnCapitalize()}")
                .IsUnique();
            entity.Property(e => e.TrmId).HasColumnName(nameof(Term.TrmId).UnCapitalize());
            entity.Property(e => e.TermText).HasColumnName(nameof(Term.TermText).UnCapitalize()).HasMaxLength(50);
            entity.Property(e => e.TermTypeId).HasColumnName(nameof(Term.TermTypeId).UnCapitalize());

            entity.HasOne(d => d.TermTypeNavigation).WithMany(p => p.Terms).HasForeignKey(d => d.TermTypeId)
                .HasConstraintName($"FK_{tableName}_{nameof(TermTypes).Pluralize()}");
        });

        modelBuilder.Entity<TermByUrl>(entity =>
        {
            var tableName = "TermsByUrls";

            entity.HasKey(e => e.TbuId);
            entity.ToTable(tableName.UnCapitalize());
            entity.HasIndex(e => new { e.BatchPartId, e.UrlId, e.Position }).HasDatabaseName($"IX_{tableName}_unique")
                .IsUnique();
            entity.Property(e => e.TbuId).HasColumnName(nameof(TermByUrl.TbuId).UnCapitalize());
            entity.Property(e => e.BatchPartId).HasColumnName(nameof(TermByUrl.BatchPartId).UnCapitalize());
            entity.Property(e => e.UrlId).HasColumnName(nameof(TermByUrl.UrlId).UnCapitalize());
            entity.Property(e => e.TermId).HasColumnName(nameof(TermByUrl.TermId).UnCapitalize());
            entity.Property(e => e.Position).HasColumnName(nameof(TermByUrl.Position).UnCapitalize());

            entity.HasOne(d => d.UrlNavigation).WithMany(p => p.TermsByUrls).HasForeignKey(d => d.UrlId)
                .HasConstraintName($"FK_{tableName}_Urls");

            entity.HasOne(d => d.TermNavigation).WithMany(p => p.TermsByUrls).HasForeignKey(d => d.TermId)
                .HasConstraintName($"FK_{tableName}_{nameof(Term).Pluralize()}");

            entity.HasOne(d => d.BatchPartNavigation).WithMany(p => p.TermsByUrls).HasForeignKey(d => d.BatchPartId)
                .HasConstraintName($"FK_{tableName}_{nameof(BatchPart).Pluralize()}");
        });

        modelBuilder.Entity<TermType>(entity =>
        {
            var tableName = nameof(TermType).Pluralize();

            entity.HasKey(e => e.TtId);
            entity.ToTable(tableName.UnCapitalize());
            entity.HasIndex(e => e.TtKey).HasDatabaseName($"IX_{tableName}_{nameof(TermType.TtKey).UnCapitalize()}")
                .IsUnique();
            entity.Property(e => e.TtId).HasColumnName(nameof(TermType.TtId).UnCapitalize());
            entity.Property(e => e.TtKey).HasColumnName(nameof(TermType.TtKey).UnCapitalize()).HasMaxLength(50);
            entity.Property(e => e.TtName).HasColumnName(nameof(TermType.TtName).UnCapitalize()).HasMaxLength(50);
        });

        modelBuilder.Entity<UrlGraphNode>(entity =>
        {
            var tableName = nameof(UrlGraphNode).Pluralize();

            entity.HasKey(e => e.UgnId);
            entity.ToTable(tableName.UnCapitalize());
            entity.HasIndex(e => new { e.BatchPartId, e.FromUrlId, e.GotUrlId })
                .HasDatabaseName($"IX_{tableName}_Unique")
                .IsUnique();
            entity.Property(e => e.UgnId).HasColumnName(nameof(UrlGraphNode.UgnId).UnCapitalize());
            entity.Property(e => e.BatchPartId).HasColumnName(nameof(UrlGraphNode.BatchPartId).UnCapitalize());
            entity.Property(e => e.FromUrlId).HasColumnName(nameof(UrlGraphNode.FromUrlId).UnCapitalize());
            entity.Property(e => e.GotUrlId).HasColumnName(nameof(UrlGraphNode.GotUrlId).UnCapitalize());

            entity.HasOne(d => d.BatchPartNavigation).WithMany(p => p.UrlGraphNodes)
                .HasForeignKey(d => d.BatchPartId).HasConstraintName($"FK_{tableName}_{nameof(BatchPart).Pluralize()}");

            entity.HasOne(d => d.FromUrlNavigation).WithMany(p => p.UrlGraphNodesFrom)
                .HasForeignKey(d => d.FromUrlId).OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName($"FK_{tableName}_{nameof(UrlGraphNode.FromUrlId).UnCapitalize()}");

            entity.HasOne(d => d.GotUrlNavigation).WithMany(p => p.UrlGraphNodesGot)
                .HasForeignKey(d => d.GotUrlId).OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName($"FK_{tableName}_{nameof(UrlGraphNode.GotUrlId).UnCapitalize()}");
        });

        modelBuilder.Entity<UrlModel>(entity =>
        {
            var tableName = "Urls";

            entity.HasKey(e => e.UrlId);
            entity.ToTable(tableName.UnCapitalize());

            entity.HasIndex(e => new { e.UrlHashCode, e.HostId, e.ExtensionId, e.SchemeId })
                .HasDatabaseName($"IX_{tableName}_unique").IsUnique();

            entity.Property(e => e.UrlId).HasColumnName(nameof(UrlModel.UrlId).UnCapitalize());
            entity.Property(e => e.UrlName).HasColumnName(nameof(UrlModel.UrlName).UnCapitalize()).HasMaxLength(2048);
            entity.Property(e => e.UrlHashCode).HasColumnName(nameof(UrlModel.UrlHashCode).UnCapitalize());
            entity.Property(e => e.HostId).HasColumnName(nameof(UrlModel.HostId).UnCapitalize());
            entity.Property(e => e.ExtensionId).HasColumnName(nameof(UrlModel.ExtensionId).UnCapitalize());
            entity.Property(e => e.SchemeId).HasColumnName(nameof(UrlModel.SchemeId).UnCapitalize());
            entity.Property(e => e.IsSiteMap).HasColumnName(nameof(UrlModel.IsSiteMap).UnCapitalize())
                .HasDefaultValue(0);

            entity.HasOne(d => d.HostNavigation).WithMany(p => p.Urls)
                .HasForeignKey(d => d.HostId).HasConstraintName($"FK_{tableName}_Hosts");

            entity.HasOne(d => d.ExtensionNavigation).WithMany(p => p.Urls)
                .HasForeignKey(d => d.ExtensionId).HasConstraintName($"FK_{tableName}_Extensions");

            entity.HasOne(d => d.SchemeNavigation).WithMany(p => p.Urls)
                .HasForeignKey(d => d.SchemeId).HasConstraintName($"FK_{tableName}_Schemes");
        });
    }
}