using CrawlerDb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SystemToolsShared;

namespace CrawlerDb.Configurations;

public class RobotConfiguration : IEntityTypeConfiguration<Robot>
{
    public void Configure(EntityTypeBuilder<Robot> builder)
    {

        var tableName = nameof(Robot).Pluralize();

        builder.HasKey(e => e.RbtId);
        builder.ToTable(tableName.UnCapitalize());

        builder.HasIndex(e => new { e.BatchPartId, e.SchemeId, e.HostId }).HasDatabaseName(tableName.CreateIndexName(
            true,
            nameof(Robot.BatchPartId), nameof(Robot.SchemeId), nameof(Robot.HostId))).IsUnique();

        builder.Property(e => e.RbtId).HasColumnName(nameof(Robot.RbtId).UnCapitalize());
        builder.Property(e => e.BatchPartId).HasColumnName(nameof(Robot.BatchPartId).UnCapitalize());
        builder.Property(e => e.HostId).HasColumnName(nameof(Robot.HostId).UnCapitalize());
        builder.Property(e => e.SchemeId).HasColumnName(nameof(Robot.SchemeId).UnCapitalize());
        builder.Property(e => e.RobotsTxt).HasColumnName(nameof(Robot.RobotsTxt).UnCapitalize())
            .HasColumnType(ConfigurationHelper.ColumnTypeNText);


        builder.HasOne(d => d.BatchPartNavigation).WithMany(p => p.Robots).HasForeignKey(d => d.BatchPartId)
            .HasConstraintName(tableName.CreateConstraintName(nameof(BatchPart)));
        builder.HasOne(d => d.SchemeNavigation).WithMany(p => p.Robots).HasForeignKey(d => d.SchemeId)
            .HasConstraintName(tableName.CreateConstraintName(nameof(SchemeModel)));
        builder.HasOne(d => d.HostNavigation).WithMany(p => p.Robots).HasForeignKey(d => d.HostId)
            .HasConstraintName(tableName.CreateConstraintName(nameof(HostModel)));

    }
}