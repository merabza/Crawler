using CrawlerDb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrawlerDb.Configurations;

public sealed class UrlGraphNodeConfiguration : IEntityTypeConfiguration<UrlGraphNode>
{
    public void Configure(EntityTypeBuilder<UrlGraphNode> builder)
    {
        builder.HasKey(e => e.UgnId);
        builder.HasIndex(e => new { e.BatchPartId, e.FromUrlId, e.GotUrlId }).IsUnique();

        builder.HasOne(d => d.BatchPartNavigation).WithMany(p => p.UrlGraphNodes).HasForeignKey(d => d.BatchPartId);

        builder.HasOne(d => d.FromUrlNavigation).WithMany(p => p.UrlGraphNodesFrom).HasForeignKey(d => d.FromUrlId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.HasOne(d => d.GotUrlNavigation).WithMany(p => p.UrlGraphNodesGot).HasForeignKey(d => d.GotUrlId)
            .OnDelete(DeleteBehavior.ClientSetNull);
    }
}