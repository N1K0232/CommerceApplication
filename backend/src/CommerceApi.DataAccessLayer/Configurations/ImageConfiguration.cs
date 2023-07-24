using CommerceApi.DataAccessLayer.Configurations.Common;
using CommerceApi.DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommerceApi.DataAccessLayer.Configurations;

public class ImageConfiguration : BaseEntityConfiguration<Image>
{
    public override void Configure(EntityTypeBuilder<Image> builder)
    {
        builder.Property(i => i.Title).HasMaxLength(100).IsRequired();
        builder.Property(i => i.Description).HasMaxLength(512).IsRequired(false);

        builder.Property(i => i.DownloadFileName).HasMaxLength(512).IsRequired();
        builder.Property(i => i.Extension).HasMaxLength(25).IsRequired();
        builder.Property(i => i.ContentType).HasMaxLength(25).IsRequired(false);

        builder.ToTable("Images");
        base.Configure(builder);
    }
}