using CommerceApi.DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommerceApi.DataAccessLayer.Configurations;

public class OrderDetailConfiguration : IEntityTypeConfiguration<OrderDetail>
{
    public void Configure(EntityTypeBuilder<OrderDetail> builder)
    {
        builder.ToTable("OrderDetails");
        builder.HasKey(o => new { o.OrderId, o.ProductId });

        builder.HasOne(o => o.Order)
            .WithMany(o => o.OrderDetails)
            .HasForeignKey(o => o.OrderId)
            .IsRequired();

        builder.HasOne(o => o.Product)
            .WithMany(p => p.OrderDetails)
            .HasForeignKey(o => o.ProductId)
            .IsRequired();

        builder.Property(o => o.OrderedQuantity).IsRequired();
        builder.Property(o => o.Price).HasPrecision(5, 2).IsRequired();
    }
}