using CrawlerDb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SystemTools.SystemToolsShared;

namespace CrawlerDb.Configurations;

public sealed class SchemeModelConfiguration : IEntityTypeConfiguration<SchemeModel>
{
    public const int SchemeNameLength = 50;

    public void Configure(EntityTypeBuilder<SchemeModel> builder)
    {
        const string tableName = "Schemes";
        builder.ToTable(tableName.UnCapitalize());

        builder.HasKey(e => e.SchId);
        builder.HasIndex(e => e.SchName).IsUnique();

        builder.Property(e => e.SchName).HasMaxLength(SchemeNameLength);
        builder.Property(e => e.SchProhibited).HasDefaultValue(0);
    }
}
