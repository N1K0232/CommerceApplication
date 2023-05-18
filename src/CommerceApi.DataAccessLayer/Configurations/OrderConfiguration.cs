using CommerceApi.DataAccessLayer.Configurations.Common;
using CommerceApi.DataAccessLayer.Entities;
using CommerceApi.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommerceApi.DataAccessLayer.Configurations;

public class OrderConfiguration : DeletableEntityConfiguration<Order>
{
    public override void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.Property(o => o.UserId).IsRequired();
        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(o => o.Date).ValueGeneratedOnAdd().HasDefaultValueSql("getutcdate()").IsRequired();
        builder.Property(o => o.Time).ValueGeneratedOnAdd().IsRequired();

        builder.HasQueryFilter(o => o.Status != OrderStatus.Canceled);

        base.Configure(builder);
    }
}