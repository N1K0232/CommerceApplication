using CommerceApi.Authentication.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommerceApi.Authentication.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasDefaultValueSql("newid()").ValueGeneratedOnAdd();

        builder.Property(t => t.Name).HasMaxLength(256).IsRequired();
        builder.Property(t => t.ConnectionString).HasMaxLength(4000).IsRequired();

        builder.Property(t => t.StorageConnectionString).HasMaxLength(4000).IsRequired(false);
        builder.Property(t => t.ContainerName).HasMaxLength(256).IsRequired(false);

        builder.HasIndex(t => t.Name).IsUnique();
    }
}