using CommerceApi.DataAccessLayer.Configurations.Common;
using CommerceApi.DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommerceApi.DataAccessLayer.Configurations;

public class SupplierConfiguration : BaseEntityConfiguration<Supplier>
{
    public override void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("Suppliers");
        builder.Property(s => s.CompanyName).HasMaxLength(256).IsRequired();
        builder.Property(s => s.ContactName).HasMaxLength(256).IsRequired();
        builder.Property(s => s.City).HasMaxLength(100).IsRequired();

        base.Configure(builder);
    }
}