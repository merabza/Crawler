using CrawlerDb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SystemToolsShared;

namespace CrawlerDb.Configurations;

public class BatchConfiguration : IEntityTypeConfiguration<Batch>
{
    public void Configure(EntityTypeBuilder<Batch> builder)
    {

        var tableName = nameof(Batch).Pluralize();

        builder.HasKey(e => e.BatchId);
        builder.ToTable(tableName.UnCapitalize());
        builder.HasIndex(e => e.BatchName).HasDatabaseName(tableName.CreateIndexName(true, nameof(Batch.BatchName)))
            .IsUnique();
        builder.Property(e => e.BatchId).HasColumnName(nameof(Batch.BatchId).UnCapitalize());
        builder.Property(e => e.BatchName).HasColumnName(nameof(Batch.BatchName).UnCapitalize()).HasMaxLength(50);
        builder.Property(e => e.IsOpen).HasColumnName(nameof(Batch.IsOpen).UnCapitalize()).HasDefaultValue(0);
        builder.Property(e => e.AutoCreateNextPart).HasColumnName(nameof(Batch.AutoCreateNextPart).UnCapitalize())
            .HasDefaultValue(0);

    }
}