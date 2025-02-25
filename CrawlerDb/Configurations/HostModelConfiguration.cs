using CrawlerDb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SystemToolsShared;

namespace CrawlerDb.Configurations;

public class HostModelConfiguration : IEntityTypeConfiguration<HostModel>
{
    public const int HostNameLength = 253;

    public void Configure(EntityTypeBuilder<HostModel> builder)
    {
        const string tableName = "Hosts";
        builder.ToTable(tableName.UnCapitalize());

        builder.HasKey(e => e.HostId);
        builder.HasIndex(e => e.HostName).IsUnique();

        builder.Property(e => e.HostName).HasMaxLength(HostNameLength);
        builder.Property(e => e.HostProhibited).HasDefaultValue(0);
    }
}