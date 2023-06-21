using CommerceApi.DataAccessLayer.Configurations.Common;
using CommerceApi.DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommerceApi.DataAccessLayer.Configurations;

public class ProductConfiguration : DeletableEntityConfiguration<Product>
{
    public override void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.Property(p => p.Name).HasMaxLength(256).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(4000).IsRequired();

        builder.Property(p => p.IdentificationCode).HasColumnType("NVARCHAR(MAX)").IsRequired();
        builder.Property(p => p.Key).HasColumnType("NVARCHAR(MAX)").IsRequired();

        builder.Property(p => p.Quantity).IsRequired();
        builder.Property(p => p.Price).HasPrecision(8, 2).IsRequired();

        builder.Property(p => p.DiscountPercentage).IsRequired(false);
        builder.Property(p => p.HasDiscount).IsRequired();

        builder.Property(p => p.ShippingCost).HasPrecision(5, 2).IsRequired(false);
        builder.Property(p => p.HasShipping).IsRequired();
        builder.Property(p => p.AverageScore).IsRequired(false);

        builder.HasOne(p => p.Category).WithMany(c => c.Products).HasForeignKey(p => p.CategoryId).IsRequired();
        builder.HasOne(p => p.Constructor).WithMany(c => c.Products).HasForeignKey(p => p.ConstructorId).IsRequired();
        builder.HasOne(p => p.Supplier).WithMany(s => s.Products).HasForeignKey(p => p.SupplierId).IsRequired();

        builder.ToTable("Products");
        base.Configure(builder);
    }
}