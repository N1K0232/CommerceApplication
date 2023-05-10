using CommerceApi.DataAccessLayer.Configurations.Common;
using CommerceApi.DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommerceApi.DataAccessLayer.Configurations;

public class ImageConfiguration : FileEntityConfiguration<Image>
{
    public override void Configure(EntityTypeBuilder<Image> builder)
    {
        builder.ToTable("Images");
        builder.Property(i => i.Title).HasMaxLength(100).IsRequired();
        builder.Property(i => i.Description).HasMaxLength(512).IsRequired(false);

        base.Configure(builder);
    }
}