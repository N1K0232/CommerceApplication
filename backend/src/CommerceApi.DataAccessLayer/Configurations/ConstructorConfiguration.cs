using CommerceApi.DataAccessLayer.Configurations.Common;
using CommerceApi.DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommerceApi.DataAccessLayer.Configurations;

public class ConstructorConfiguration : BaseEntityConfiguration<Constructor>
{
    public override void Configure(EntityTypeBuilder<Constructor> builder)
    {
        builder.Property(c => c.Name).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Street).HasMaxLength(100).IsRequired();
        builder.Property(c => c.City).HasMaxLength(100).IsRequired();
        builder.Property(c => c.PostalCode).HasMaxLength(20).IsRequired();
        builder.Property(c => c.WebSiteUrl).HasColumnType("NVARCHAR(MAX)").IsRequired(false);

        builder.ToTable("Constructors");
        base.Configure(builder);
    }
}