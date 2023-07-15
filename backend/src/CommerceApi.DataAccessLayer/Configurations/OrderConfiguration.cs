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
        builder.Property(o => o.UserId).IsRequired();
        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.Property(o => o.IdentityNumber).UseIdentityColumn(1, 1).IsRequired();
        builder.Property(o => o.IdentificationNumber).HasMaxLength(50).IsRequired();
        builder.Property(o => o.IdentificationCode).HasColumnType("NVARCHAR(MAX)").IsRequired();

        builder.Property(o => o.Date).ValueGeneratedOnAdd().HasDefaultValueSql("getutcdate()").IsRequired();
        builder.Property(o => o.Time).ValueGeneratedOnAdd().IsRequired();

        builder.HasQueryFilter(o => o.Status != OrderStatus.Canceled);

        builder.ToTable("Orders");
        base.Configure(builder);
    }
}