using CrawlerDb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SystemToolsShared;

namespace CrawlerDb.Configurations;

public class UrlGraphNodeConfiguration : IEntityTypeConfiguration<UrlGraphNode>
{
    public void Configure(EntityTypeBuilder<UrlGraphNode> builder)
    {

        var tableName = nameof(UrlGraphNode).Pluralize();

        builder.HasKey(e => e.UgnId);
        builder.ToTable(tableName.UnCapitalize());
        builder.HasIndex(e => new { e.BatchPartId, e.FromUrlId, e.GotUrlId }).HasDatabaseName(
            tableName.CreateIndexName(true, nameof(UrlGraphNode.BatchPartId), nameof(UrlGraphNode.FromUrlId),
                nameof(UrlGraphNode.GotUrlId))).IsUnique();

        builder.Property(e => e.UgnId).HasColumnName(nameof(UrlGraphNode.UgnId).UnCapitalize());
        builder.Property(e => e.BatchPartId).HasColumnName(nameof(UrlGraphNode.BatchPartId).UnCapitalize());
        builder.Property(e => e.FromUrlId).HasColumnName(nameof(UrlGraphNode.FromUrlId).UnCapitalize());
        builder.Property(e => e.GotUrlId).HasColumnName(nameof(UrlGraphNode.GotUrlId).UnCapitalize());

        builder.HasOne(d => d.BatchPartNavigation).WithMany(p => p.UrlGraphNodes).HasForeignKey(d => d.BatchPartId)
            .HasConstraintName(tableName.CreateConstraintName(nameof(BatchPart)));

        builder.HasOne(d => d.FromUrlNavigation).WithMany(p => p.UrlGraphNodesFrom).HasForeignKey(d => d.FromUrlId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName(tableName.CreateConstraintName(nameof(UrlGraphNode), nameof(UrlGraphNode.FromUrlId)));

        builder.HasOne(d => d.GotUrlNavigation).WithMany(p => p.UrlGraphNodesGot).HasForeignKey(d => d.GotUrlId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName(tableName.CreateConstraintName(nameof(UrlGraphNode), nameof(UrlGraphNode.GotUrlId)));

    }
}