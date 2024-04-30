//using CrawlerDb.Models;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata.Builders;
//using SystemToolsShared;

//namespace CrawlerDb.Configurations;

//public class UrlAllowModelConfiguration : IEntityTypeConfiguration<UrlAllowModel>
//{
//    public void Configure(EntityTypeBuilder<UrlAllowModel> builder)
//    {
//        const string tableName = "UrlAllows";

//        builder.HasKey(e => e.UaId);
//        builder.ToTable(tableName.UnCapitalize());

//        builder.HasIndex(e => new { e.HostId, e.PatternText })
//            .HasDatabaseName(tableName.CreateIndexName(true, nameof(UrlAllowModel.HostId),
//                nameof(UrlAllowModel.PatternText))).IsUnique();

//        builder.Property(e => e.UaId).HasColumnName(nameof(UrlAllowModel.UaId).UnCapitalize());
//        builder.Property(e => e.HostId).HasColumnName(nameof(UrlModel.HostId).UnCapitalize());
//        builder.Property(e => e.PatternText).HasColumnName(nameof(UrlAllowModel.PatternText).UnCapitalize())
//            .HasMaxLength(2048);
//        builder.Property(e => e.IsAllowed).HasColumnName(nameof(UrlAllowModel.IsAllowed).UnCapitalize())
//            .HasDefaultValue(0);

//        builder.HasOne(d => d.HostNavigation).WithMany(p => p.UrlAllows).HasForeignKey(d => d.HostId)
//            .HasConstraintName(tableName.CreateConstraintName(nameof(HostModel)));
//    }
//}