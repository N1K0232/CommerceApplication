using CommerceApi.DataAccessLayer.Entities.Common;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommerceApi.DataAccessLayer.Configurations.Common;

public abstract class FileEntityConfiguration<T> : BaseEntityConfiguration<T> where T : FileEntity
{
    public override void Configure(EntityTypeBuilder<T> builder)
    {
        builder.Property(e => e.FileName).HasMaxLength(256);
        builder.Property(e => e.Path).HasMaxLength(512).IsRequired();
        builder.Property(e => e.Length).IsRequired();

        builder.Property(e => e.ContentType).HasMaxLength(100).IsRequired(false);
        builder.Property(e => e.Extension).HasMaxLength(100).IsRequired();

        builder.Property(e => e.DownloadFileName).HasMaxLength(512).IsRequired();
        builder.Property(e => e.DownloadPath).HasMaxLength(256).IsRequired();

        builder.HasIndex(e => e.FileName).IsUnique();
        builder.HasIndex(e => e.Path).IsUnique();
        builder.HasIndex(e => e.DownloadFileName).IsUnique();
        builder.HasIndex(e => e.DownloadPath).IsUnique();

        base.Configure(builder);
    }
}