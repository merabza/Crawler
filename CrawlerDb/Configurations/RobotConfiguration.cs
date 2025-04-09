using CrawlerDb.Models;
using DatabaseToolsShared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SystemToolsShared;

namespace CrawlerDb.Configurations;

public sealed class RobotConfiguration : IEntityTypeConfiguration<Robot>
{
    public void Configure(EntityTypeBuilder<Robot> builder)
    {
        var tableName = nameof(Robot).Pluralize();
        builder.ToTable(tableName.UnCapitalize());

        builder.HasKey(e => e.RbtId);
        builder.HasIndex(e => new { e.BatchPartId, e.SchemeId, e.HostId }).IsUnique();

        builder.Property(e => e.RobotsTxt).HasColumnType(ConfigurationHelper.ColumnTypeNText);

        builder.HasOne(d => d.BatchPartNavigation).WithMany(p => p.Robots).HasForeignKey(d => d.BatchPartId);
        builder.HasOne(d => d.SchemeNavigation).WithMany(p => p.Robots).HasForeignKey(d => d.SchemeId);
        builder.HasOne(d => d.HostNavigation).WithMany(p => p.Robots).HasForeignKey(d => d.HostId);
    }
}