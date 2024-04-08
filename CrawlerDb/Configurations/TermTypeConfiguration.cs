using CrawlerDb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SystemToolsShared;

namespace CrawlerDb.Configurations;

public class TermTypeConfiguration : IEntityTypeConfiguration<TermType>
{
    public void Configure(EntityTypeBuilder<TermType> builder)
    {

        var tableName = nameof(TermType).Pluralize();

        builder.HasKey(e => e.TtId);
        builder.ToTable(tableName.UnCapitalize());
        builder.HasIndex(e => e.TtKey).HasDatabaseName(tableName.CreateIndexName(true, nameof(TermType.TtKey)))
            .IsUnique();
        builder.Property(e => e.TtId).HasColumnName(nameof(TermType.TtId).UnCapitalize());
        builder.Property(e => e.TtKey).HasColumnName(nameof(TermType.TtKey).UnCapitalize()).HasMaxLength(50);
        builder.Property(e => e.TtName).HasColumnName(nameof(TermType.TtName).UnCapitalize()).HasMaxLength(50);

    }
}