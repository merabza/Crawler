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
        builder.ToTable(tableName.UnCapitalize());

        builder.HasKey(e => e.TbuId);
        builder.HasIndex(e => new { e.BatchPartId, e.UrlId, e.Position }).IsUnique();

        builder.HasOne(d => d.UrlNavigation).WithMany(p => p.TermsByUrls).HasForeignKey(d => d.UrlId);
        builder.HasOne(d => d.TermNavigation).WithMany(p => p.TermsByUrls).HasForeignKey(d => d.TermId);
        builder.HasOne(d => d.BatchPartNavigation).WithMany(p => p.TermsByUrls).HasForeignKey(d => d.BatchPartId);
    }
}