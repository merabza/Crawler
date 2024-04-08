using CrawlerDb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SystemToolsShared;

namespace CrawlerDb.Configurations;

public class ContentAnalysisConfiguration : IEntityTypeConfiguration<ContentAnalysis>
{
    public void Configure(EntityTypeBuilder<ContentAnalysis> builder)
    {

        var tableName = nameof(ContentAnalysis).Pluralize();

        builder.HasKey(e => e.CaId);
        builder.ToTable(tableName.UnCapitalize());
        builder.HasIndex(e => new { e.BatchPartId, e.UrlId })
            .HasDatabaseName(tableName.CreateIndexName(true, nameof(ContentAnalysis.BatchPartId),
                nameof(ContentAnalysis.UrlId))).IsUnique();
        builder.Property(e => e.CaId).HasColumnName(nameof(ContentAnalysis.CaId).UnCapitalize());
        builder.Property(e => e.BatchPartId).HasColumnName(nameof(ContentAnalysis.BatchPartId).UnCapitalize());
        builder.Property(e => e.UrlId).HasColumnName(nameof(ContentAnalysis.UrlId).UnCapitalize());
        builder.Property(e => e.ResponseStatusCode)
            .HasColumnName(nameof(ContentAnalysis.ResponseStatusCode).UnCapitalize());
        builder.Property(e => e.Finish).HasColumnName(nameof(ContentAnalysis.Finish).UnCapitalize());

        builder.HasOne(d => d.BatchPartNavigation).WithMany(p => p.ContentsAnalysis).HasForeignKey(d => d.BatchPartId)
            .HasConstraintName(tableName.CreateConstraintName(nameof(BatchPart)));

        builder.HasOne(d => d.UrlNavigation).WithMany(p => p.ContentsAnalysis).HasForeignKey(d => d.UrlId)
            .HasConstraintName(tableName.CreateConstraintName(nameof(UrlModel)));

    }
}