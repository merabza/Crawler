using CrawlerDb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SystemToolsShared;

namespace CrawlerDb.Configurations;

public class HostByBatchConfiguration : IEntityTypeConfiguration<HostByBatch>
{
    public void Configure(EntityTypeBuilder<HostByBatch> builder)
    {
        const string tableName = "HostsByBatches";

        builder.HasKey(e => e.HbbId);
        builder.ToTable(tableName.UnCapitalize());

        builder.HasIndex(e => new { e.BatchId, e.SchemeId, e.HostId }).HasDatabaseName(tableName.CreateIndexName(true,
            nameof(HostByBatch.BatchId), nameof(HostByBatch.SchemeId), nameof(HostByBatch.HostId))).IsUnique();

        builder.Property(e => e.HbbId).HasColumnName(nameof(HostByBatch.HbbId).UnCapitalize());
        builder.Property(e => e.BatchId).HasColumnName(nameof(HostByBatch.BatchId).UnCapitalize());
        builder.Property(e => e.HostId).HasColumnName(nameof(HostByBatch.HostId).UnCapitalize());
        builder.Property(e => e.SchemeId).HasColumnName(nameof(HostByBatch.SchemeId).UnCapitalize());

        builder.HasOne(d => d.BatchNavigation).WithMany(p => p.HostsByBatches).HasForeignKey(d => d.BatchId)
            .HasConstraintName(tableName.CreateConstraintName(nameof(Batch)));
        builder.HasOne(d => d.SchemeNavigation).WithMany(p => p.HostsByBatches).HasForeignKey(d => d.SchemeId)
            .HasConstraintName(tableName.CreateConstraintName(nameof(SchemeModel)));
        builder.HasOne(d => d.HostNavigation).WithMany(p => p.HostsByBatches).HasForeignKey(d => d.HostId)
            .HasConstraintName(tableName.CreateConstraintName(nameof(HostModel)));
    }
}