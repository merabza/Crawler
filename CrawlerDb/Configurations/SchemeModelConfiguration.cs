using CrawlerDb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SystemToolsShared;

namespace CrawlerDb.Configurations;

public class SchemeModelConfiguration : IEntityTypeConfiguration<SchemeModel>
{
    public const int SchemeNameLength = 50;

    public void Configure(EntityTypeBuilder<SchemeModel> builder)
    {
        const string tableName = "Schemes";

        builder.HasKey(e => e.SchId);
        builder.ToTable(tableName.UnCapitalize());
        builder.HasIndex(e => e.SchName).HasDatabaseName(tableName.CreateIndexName(true, nameof(SchemeModel.SchName)))
            .IsUnique();
        builder.Property(e => e.SchId).HasColumnName(nameof(SchemeModel.SchId).UnCapitalize());
        builder.Property(e => e.SchName).HasColumnName(nameof(SchemeModel.SchName).UnCapitalize())
            .HasMaxLength(SchemeNameLength);
        builder.Property(e => e.SchProhibited).HasColumnName(nameof(SchemeModel.SchProhibited).UnCapitalize())
            .HasDefaultValue(0);
    }
}