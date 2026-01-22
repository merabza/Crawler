using CrawlerDb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SystemTools.SystemToolsShared;

namespace CrawlerDb.Configurations;

public sealed class TermTypeConfiguration : IEntityTypeConfiguration<TermType>
{
    public void Configure(EntityTypeBuilder<TermType> builder)
    {
        string tableName = nameof(TermType).Pluralize();
        builder.ToTable(tableName.UnCapitalize());

        builder.HasKey(e => e.TtId);
        builder.HasIndex(e => e.TtKey).IsUnique();

        builder.Property(e => e.TtKey).HasMaxLength(50);
        builder.Property(e => e.TtName).HasMaxLength(50);
    }
}
