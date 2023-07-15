using CommerceApi.DataAccessLayer.Configurations.Common;
using CommerceApi.DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommerceApi.DataAccessLayer.Configurations;

public class SupplierConfiguration : BaseEntityConfiguration<Supplier>
{
    public override void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.Property(s => s.CompanyName).HasMaxLength(100).IsRequired();
        builder.Property(s => s.ContactName).HasMaxLength(100).IsRequired();
        builder.Property(s => s.City).HasMaxLength(50).IsRequired();

        builder.ToTable("Suppliers");
        base.Configure(builder);
    }
}