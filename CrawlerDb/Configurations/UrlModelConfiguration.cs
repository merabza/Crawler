using CrawlerDb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SystemToolsShared;

namespace CrawlerDb.Configurations;

public class UrlModelConfiguration : IEntityTypeConfiguration<UrlModel>
{
    public void Configure(EntityTypeBuilder<UrlModel> builder)
    {
        const string tableName = "Urls";

        builder.HasKey(e => e.UrlId);
        builder.ToTable(tableName.UnCapitalize());

        builder.HasIndex(e => new { e.UrlHashCode, e.HostId, e.ExtensionId, e.SchemeId }).HasDatabaseName(
            tableName.CreateIndexName(true, nameof(UrlModel.UrlHashCode), nameof(UrlModel.HostId),
                nameof(UrlModel.ExtensionId), nameof(UrlModel.SchemeId))).IsUnique();

        builder.Property(e => e.UrlId).HasColumnName(nameof(UrlModel.UrlId).UnCapitalize());
        builder.Property(e => e.UrlName).HasColumnName(nameof(UrlModel.UrlName).UnCapitalize()).HasMaxLength(2048);
        builder.Property(e => e.UrlHashCode).HasColumnName(nameof(UrlModel.UrlHashCode).UnCapitalize());
        builder.Property(e => e.HostId).HasColumnName(nameof(UrlModel.HostId).UnCapitalize());
        builder.Property(e => e.ExtensionId).HasColumnName(nameof(UrlModel.ExtensionId).UnCapitalize());
        builder.Property(e => e.SchemeId).HasColumnName(nameof(UrlModel.SchemeId).UnCapitalize());
        builder.Property(e => e.IsSiteMap).HasColumnName(nameof(UrlModel.IsSiteMap).UnCapitalize()).HasDefaultValue(0);

        builder.HasOne(d => d.HostNavigation).WithMany(p => p.Urls).HasForeignKey(d => d.HostId)
            .HasConstraintName(tableName.CreateConstraintName(nameof(HostModel)));
        builder.HasOne(d => d.ExtensionNavigation).WithMany(p => p.Urls).HasForeignKey(d => d.ExtensionId)
            .HasConstraintName(tableName.CreateConstraintName(nameof(ExtensionModel)));
        builder.HasOne(d => d.SchemeNavigation).WithMany(p => p.Urls).HasForeignKey(d => d.SchemeId)
            .HasConstraintName(tableName.CreateConstraintName(nameof(SchemeModel)));

    }
}