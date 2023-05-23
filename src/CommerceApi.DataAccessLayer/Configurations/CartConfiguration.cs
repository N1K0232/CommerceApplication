using CommerceApi.DataAccessLayer.Configurations.Common;
using CommerceApi.DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommerceApi.DataAccessLayer.Configurations;

public class CartConfiguration : DeletableEntityConfiguration<Cart>
{
    public override void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("Carts");
        builder.Property(c => c.UserId).IsRequired();

        base.Configure(builder);
    }
}