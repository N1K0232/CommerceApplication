using CommerceApi.DataAccessLayer.Configurations.Common;
using CommerceApi.DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommerceApi.DataAccessLayer.Configurations;

public class InvoiceConfiguration : BaseEntityConfiguration<Invoice>
{
    public override void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.Property(i => i.Quantity).IsRequired();
        builder.Property(i => i.Price).IsRequired().HasPrecision(8, 2);
        builder.Property(i => i.TotalPrice).IsRequired().HasPrecision(8, 2);

        builder.HasOne(i => i.Product).WithMany(p => p.Invoices).HasForeignKey(i => i.ProductId);

        builder.ToTable("Invoices");
        base.Configure(builder);
    }
}