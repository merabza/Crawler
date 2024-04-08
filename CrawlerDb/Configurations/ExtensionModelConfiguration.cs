using CrawlerDb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SystemToolsShared;

namespace CrawlerDb.Configurations;

public class ExtensionModelConfiguration : IEntityTypeConfiguration<ExtensionModel>
{
    public void Configure(EntityTypeBuilder<ExtensionModel> builder)
    {

        const string tableName = "Extensions";

        builder.HasKey(e => e.ExtId);
        builder.ToTable(tableName.UnCapitalize());
        builder.HasIndex(e => e.ExtName)
            .HasDatabaseName(tableName.CreateIndexName(true, nameof(ExtensionModel.ExtName))).IsUnique();
        builder.Property(e => e.ExtId).HasColumnName(nameof(ExtensionModel.ExtId).UnCapitalize());
        builder.Property(e => e.ExtName).HasColumnName(nameof(ExtensionModel.ExtName).UnCapitalize()).HasMaxLength(50);
        builder.Property(e => e.ExtProhibited).HasColumnName(nameof(ExtensionModel.ExtProhibited).UnCapitalize())
            .HasDefaultValue(0);

    }
}