using CommerceApi.Authentication.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommerceApi.Authentication.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).IsRequired().ValueGeneratedOnAdd().HasDefaultValueSql("newid()");

        builder.Property(u => u.FirstName).HasMaxLength(256).IsRequired();
        builder.Property(u => u.LastName).HasMaxLength(256).IsRequired(false);
        builder.Property(u => u.DateOfBirth).IsRequired(false);

        builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
        builder.Property(u => u.NormalizedEmail).HasMaxLength(256).IsRequired();

        builder.Property(u => u.UserName).HasMaxLength(256).IsRequired();
        builder.Property(u => u.NormalizedUserName).HasMaxLength(256).IsRequired();

        builder.Property(u => u.PasswordHash).HasColumnType("NVARCHAR(MAX)").IsRequired();
        builder.Property(u => u.SecurityStamp).HasColumnType("NVARCHAR(MAX)").IsRequired();
        builder.Property(u => u.ConcurrencyStamp).HasColumnType("NVARCHAR(MAX)").IsRequired();

        builder.Property(u => u.RegistrationDate).HasDefaultValueSql("getutcdate()").IsRequired();

        builder.Property(u => u.RefreshToken).HasMaxLength(512).IsRequired(false);
        builder.Property(u => u.RefreshTokenExpirationDate).IsRequired(false);
    }
}