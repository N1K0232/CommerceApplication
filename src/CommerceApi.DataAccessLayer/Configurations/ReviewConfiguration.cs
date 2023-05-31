using CommerceApi.DataAccessLayer.Configurations.Common;
using CommerceApi.DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommerceApi.DataAccessLayer.Configurations;

public class ReviewConfiguration : BaseEntityConfiguration<Review>
{
    public override void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.Property(r => r.Text).HasColumnType("NVARCHAR(MAX)").IsRequired();
        builder.Property(r => r.Score).IsRequired();
        builder.Property(r => r.ReviewDate).IsRequired().ValueGeneratedOnAdd().HasDefaultValueSql("getutcdate()");

        builder.Property(r => r.UserId).IsRequired();
        builder.HasOne(r => r.Product).WithMany(p => p.Reviews).HasForeignKey(r => r.ProductId).IsRequired();

        builder.ToTable("Reviews");
        base.Configure(builder);
    }
}