using CrawlerDb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SystemToolsShared;

namespace CrawlerDb.Configurations;

public sealed class UrlModelConfiguration : IEntityTypeConfiguration<UrlModel>
{
    public const int TermTextLength = 2048;

    public void Configure(EntityTypeBuilder<UrlModel> builder)
    {
        const string tableName = "Urls";
        builder.ToTable(tableName.UnCapitalize());

        builder.HasKey(e => e.UrlId);
        builder.HasIndex(e => new { e.UrlHashCode, e.HostId, e.ExtensionId, e.SchemeId });

        builder.Property(e => e.UrlName).HasMaxLength(TermTextLength);
        builder.Property(e => e.IsSiteMap).HasDefaultValue(0);
        builder.Property(e => e.IsAllowed).HasDefaultValue(0);

        builder.HasOne(d => d.HostNavigation).WithMany(p => p.Urls).HasForeignKey(d => d.HostId);
        builder.HasOne(d => d.ExtensionNavigation).WithMany(p => p.Urls).HasForeignKey(d => d.ExtensionId);
        builder.HasOne(d => d.SchemeNavigation).WithMany(p => p.Urls).HasForeignKey(d => d.SchemeId);
    }
}