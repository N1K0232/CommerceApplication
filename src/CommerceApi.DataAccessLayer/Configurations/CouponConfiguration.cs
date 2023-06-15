using CommerceApi.DataAccessLayer.Configurations.Common;
using CommerceApi.DataAccessLayer.Entities;
using CommerceApi.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommerceApi.DataAccessLayer.Configurations;

public class CouponConfiguration : DeletableEntityConfiguration<Coupon>
{
    public override void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.Property(c => c.Code).HasColumnType("NVARCHAR(MAX)").IsRequired();
        builder.Property(c => c.DiscountPercentage).IsRequired();
        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(c => c.StartDate).IsRequired();
        builder.Property(c => c.ExpirationDate).IsRequired();
        builder.Property(c => c.IsValid).IsRequired();

        builder.Property(c => c.UserId);
        builder.HasQueryFilter(c => c.IsValid && (c.Status != CouponStatus.Expired || c.Status != CouponStatus.Deleted));

        builder.ToTable("Coupons");
        base.Configure(builder);
    }
}