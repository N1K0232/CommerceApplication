using CommerceApi.Authentication.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommerceApi.Authentication.Configurations;

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable("Addresses");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).IsRequired().ValueGeneratedOnAdd().HasDefaultValueSql("newid()");

        builder.Property(a => a.Street).HasMaxLength(100).IsRequired();
        builder.Property(a => a.City).HasMaxLength(50).IsRequired();
        builder.Property(a => a.PostalCode).HasMaxLength(50).IsRequired();
        builder.Property(a => a.Country).HasMaxLength(50).IsRequired();

        builder.HasOne(a => a.User).WithMany(u => u.Addresses).HasForeignKey(a => a.UserId).IsRequired();
    }
}