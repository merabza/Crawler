using CrawlerDb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrawlerDb.Configurations;

public sealed class BatchConfiguration : IEntityTypeConfiguration<Batch>
{
    public const int BatchNameLength = 50;

    public void Configure(EntityTypeBuilder<Batch> builder)
    {
        builder.HasKey(e => e.BatchId);
        builder.HasIndex(e => e.BatchName).IsUnique();

        builder.Property(e => e.BatchName).HasMaxLength(BatchNameLength);
        builder.Property(e => e.IsOpen).HasDefaultValue(0);
        builder.Property(e => e.AutoCreateNextPart).HasDefaultValue(0);
    }
}
