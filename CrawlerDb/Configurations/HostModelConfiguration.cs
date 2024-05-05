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

        builder.HasKey(e => e.HostId);
        builder.ToTable(tableName.UnCapitalize());
        builder.HasIndex(e => e.HostName).HasDatabaseName(tableName.CreateIndexName(true, nameof(HostModel.HostName)))
            .IsUnique();
        builder.Property(e => e.HostId).HasColumnName(nameof(HostModel.HostId).UnCapitalize());
        builder.Property(e => e.HostName).HasColumnName(nameof(HostModel.HostName).UnCapitalize())
            .HasMaxLength(HostNameLength);
        builder.Property(e => e.HostProhibited).HasColumnName(nameof(HostModel.HostProhibited).UnCapitalize())
            .HasDefaultValue(0);
    }
}