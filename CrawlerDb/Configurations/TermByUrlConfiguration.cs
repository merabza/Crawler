using CrawlerDb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SystemToolsShared;

namespace CrawlerDb.Configurations;

public class TermByUrlConfiguration : IEntityTypeConfiguration<TermByUrl>
{
    public void Configure(EntityTypeBuilder<TermByUrl> builder)
    {
        const string tableName = "TermsByUrls";

        builder.HasKey(e => e.TbuId);
        builder.ToTable(tableName.UnCapitalize());
        builder.HasIndex(e => new { e.BatchPartId, e.UrlId, e.Position }).HasDatabaseName(tableName.CreateIndexName(
            true,
            nameof(TermByUrl.BatchPartId), nameof(TermByUrl.UrlId), nameof(TermByUrl.Position))).IsUnique();
        builder.Property(e => e.TbuId).HasColumnName(nameof(TermByUrl.TbuId).UnCapitalize());
        builder.Property(e => e.BatchPartId).HasColumnName(nameof(TermByUrl.BatchPartId).UnCapitalize());
        builder.Property(e => e.UrlId).HasColumnName(nameof(TermByUrl.UrlId).UnCapitalize());
        builder.Property(e => e.TermId).HasColumnName(nameof(TermByUrl.TermId).UnCapitalize());
        builder.Property(e => e.Position).HasColumnName(nameof(TermByUrl.Position).UnCapitalize());

        builder.HasOne(d => d.UrlNavigation).WithMany(p => p.TermsByUrls).HasForeignKey(d => d.UrlId)
            .HasConstraintName(tableName.CreateConstraintName(nameof(UrlModel)));
        builder.HasOne(d => d.TermNavigation).WithMany(p => p.TermsByUrls).HasForeignKey(d => d.TermId)
            .HasConstraintName(tableName.CreateConstraintName(nameof(Term)));
        builder.HasOne(d => d.BatchPartNavigation).WithMany(p => p.TermsByUrls).HasForeignKey(d => d.BatchPartId)
            .HasConstraintName(tableName.CreateConstraintName(nameof(BatchPart)));
    }
}