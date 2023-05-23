using CommerceApi.DataAccessLayer.Configurations.Common;
using CommerceApi.DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommerceApi.DataAccessLayer.Configurations;

public class CartItemConfiguration : DeletableEntityConfiguration<CartItem>
{
    public override void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItems");
        builder.Property(c => c.UnitPrice).HasPrecision(8, 2).IsRequired();
        builder.Property(c => c.Quantity).IsRequired();

        builder.HasOne(c => c.Cart).WithMany(c => c.CartItems).HasForeignKey(c => c.CartId).IsRequired();
        builder.HasOne(c => c.Product).WithMany(p => p.CartItems).HasForeignKey(c => c.ProductId).IsRequired();

        base.Configure(builder);
    }
}