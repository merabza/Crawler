using CrawlerDb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrawlerDb.Configurations;

public class ContentAnalysisConfiguration : IEntityTypeConfiguration<ContentAnalysis>
{
    public void Configure(EntityTypeBuilder<ContentAnalysis> builder)
    {
        builder.HasKey(e => e.CaId);
        builder.HasIndex(e => new { e.BatchPartId, e.UrlId }).IsUnique();

        builder.HasOne(d => d.BatchPartNavigation).WithMany(p => p.ContentsAnalysis).HasForeignKey(d => d.BatchPartId);

        builder.HasOne(d => d.UrlNavigation).WithMany(p => p.ContentsAnalysis).HasForeignKey(d => d.UrlId);
    }
}