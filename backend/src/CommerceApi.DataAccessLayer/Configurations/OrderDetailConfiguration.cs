using CommerceApi.DataAccessLayer.Configurations.Common;
using CommerceApi.DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommerceApi.DataAccessLayer.Configurations;

public class OrderDetailConfiguration : DeletableEntityConfiguration<OrderDetail>
{
    public override void Configure(EntityTypeBuilder<OrderDetail> builder)
    {
        builder.Property(o => o.UnitPrice).HasPrecision(8, 2).IsRequired();
        builder.Property(o => o.Quantity).IsRequired();

        builder.HasOne(o => o.Order).WithMany(o => o.OrderDetails).HasForeignKey(o => o.OrderId).IsRequired();
        builder.HasOne(o => o.Product).WithMany(p => p.OrderDetails).HasForeignKey(o => o.ProductId).IsRequired();

        builder.ToTable("OrderDetails");
        base.Configure(builder);
    }
}