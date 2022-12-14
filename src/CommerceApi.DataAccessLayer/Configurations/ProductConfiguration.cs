using CommerceApi.DataAccessLayer.Configurations.Common;
using CommerceApi.DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommerceApi.DataAccessLayer.Configurations;

public class ProductConfiguration : DeletableEntityConfiguration<Product>
{
    public override void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.Property(p => p.Name).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(2000).IsRequired();
        builder.Property(p => p.Specifications).HasMaxLength(2000).IsRequired();

        builder.Property(p => p.Brand).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Model).HasMaxLength(100).IsRequired();

        builder.Property(p => p.Condition).HasConversion<string>().HasMaxLength(10).IsRequired();
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(10).IsRequired();

        builder.Property(p => p.Quantity).IsRequired();
        builder.Property(p => p.Price).HasPrecision(5, 2).IsRequired();

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .IsRequired();

        builder.HasOne(p => p.Supplier)
            .WithMany(s => s.Products)
            .HasForeignKey(p => p.SupplierId)
            .IsRequired();

        base.Configure(builder);
    }
}