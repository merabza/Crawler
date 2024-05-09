using CrawlerDb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SystemToolsShared;

namespace CrawlerDb.Configurations;

public class BatchPartConfiguration : IEntityTypeConfiguration<BatchPart>
{
    public void Configure(EntityTypeBuilder<BatchPart> builder)
    {
        var tableName = nameof(BatchPart).Pluralize();

        builder.HasKey(e => e.BpId);
        builder.ToTable(tableName.UnCapitalize());
        builder.HasIndex(e => new { e.BatchId, e.Created })
            .HasDatabaseName(tableName.CreateIndexName(true, nameof(BatchPart.BatchId), nameof(BatchPart.Created)))
            .IsUnique();
        builder.Property(e => e.BpId).HasColumnName(nameof(BatchPart.BpId).UnCapitalize());
        builder.Property(e => e.BatchId).HasColumnName(nameof(BatchPart.BatchId).UnCapitalize());
        builder.Property(e => e.Created).HasColumnName(nameof(BatchPart.Created).UnCapitalize());
        builder.Property(e => e.Finished).HasColumnName(nameof(BatchPart.Finished).UnCapitalize());

        builder.HasOne(d => d.BatchNavigation).WithMany(p => p.BatchParts).HasForeignKey(d => d.BatchId)
            .HasConstraintName(tableName.CreateConstraintName(nameof(Batch)));
    }
}