using CrawlerDb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SystemToolsShared;

namespace CrawlerDb.Configurations;

public class TermConfiguration : IEntityTypeConfiguration<Term>
{
    public void Configure(EntityTypeBuilder<Term> builder)
    {

        var tableName = nameof(Term).Pluralize();

        builder.HasKey(e => e.TrmId);
        builder.ToTable(tableName.UnCapitalize());
        builder.HasIndex(e => e.TermText).HasDatabaseName(tableName.CreateIndexName(true, nameof(Term.TermText)))
            .IsUnique();
        builder.Property(e => e.TrmId).HasColumnName(nameof(Term.TrmId).UnCapitalize());
        builder.Property(e => e.TermText).HasColumnName(nameof(Term.TermText).UnCapitalize()).HasMaxLength(50);
        builder.Property(e => e.TermTypeId).HasColumnName(nameof(Term.TermTypeId).UnCapitalize());

        builder.HasOne(d => d.TermTypeNavigation).WithMany(p => p.Terms).HasForeignKey(d => d.TermTypeId)
            .HasConstraintName(tableName.CreateConstraintName(nameof(TermType)));

    }
}