using CrawlerDb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SystemToolsShared;

namespace CrawlerDb.Configurations;

public class ExtensionModelConfiguration : IEntityTypeConfiguration<ExtensionModel>
{
    public const int ExtensionNameLength = 50;

    public void Configure(EntityTypeBuilder<ExtensionModel> builder)
    {
        const string tableName = "Extensions";
        builder.ToTable(tableName.UnCapitalize());

        builder.HasKey(e => e.ExtId);
        builder.HasIndex(e => e.ExtName).IsUnique();

        builder.Property(e => e.ExtName).HasMaxLength(ExtensionNameLength);
        builder.Property(e => e.ExtProhibited).HasDefaultValue(0);
    }
}