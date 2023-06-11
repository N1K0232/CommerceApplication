using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommerceApi.DataAccessLayer.Configurations;

public class DataProtectionKeyConfiguration : IEntityTypeConfiguration<DataProtectionKey>
{
    public void Configure(EntityTypeBuilder<DataProtectionKey> builder)
    {
        builder.ToTable("DataProtectionKeys");
        builder.HasKey(k => k.Id);
        builder.Property(k => k.Id).UseIdentityColumn(1, 1);

        builder.Property(k => k.FriendlyName).HasColumnType("NVARCHAR(MAX)").IsRequired(false);
        builder.Property(k => k.Xml).HasColumnType("NVARCHAR(MAX)").IsRequired(false);
    }
}