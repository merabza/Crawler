using CrawlerDb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SystemToolsShared;

namespace CrawlerDb.Configurations;

public class TermConfiguration : IEntityTypeConfiguration<Term>
{
    public const int TermTextLength = 50;

    public void Configure(EntityTypeBuilder<Term> builder)
    {

        var tableName = nameof(Term).Pluralize();

        builder.HasKey(e => e.TrmId);
        builder.ToTable(tableName.UnCapitalize());
        builder.HasIndex(e => e.TermText).HasDatabaseName(tableName.CreateIndexName(true, nameof(Term.TermText)));
        builder.Property(e => e.TrmId).HasColumnName(nameof(Term.TrmId).UnCapitalize());
        builder.Property(e => e.TermText).HasColumnName(nameof(Term.TermText).UnCapitalize())
            .HasMaxLength(TermTextLength).UseCollation("SQL_Latin1_General_CP1_CS_AS");
        builder.Property(e => e.TermTypeId).HasColumnName(nameof(Term.TermTypeId).UnCapitalize());

        builder.HasOne(d => d.TermTypeNavigation).WithMany(p => p.Terms).HasForeignKey(d => d.TermTypeId)
            .HasConstraintName(tableName.CreateConstraintName(nameof(TermType)));
    }
}