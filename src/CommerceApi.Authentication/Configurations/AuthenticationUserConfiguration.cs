using CommerceApi.Authentication.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommerceApi.Authentication.Configurations;

public class AuthenticationUserConfiguration : IEntityTypeConfiguration<AuthenticationUser>
{
    public void Configure(EntityTypeBuilder<AuthenticationUser> builder)
    {
        builder.HasKey(user => user.Id);
        builder.Property(user => user.Id).IsRequired().ValueGeneratedOnAdd().HasDefaultValueSql("newid()");

        builder.Property(user => user.FirstName).HasMaxLength(256).IsRequired();
        builder.Property(user => user.LastName).HasMaxLength(256).IsRequired(false);
        builder.Property(user => user.DateOfBirth).IsRequired(false);

        builder.Property(user => user.Street).HasMaxLength(100).IsRequired(false);
        builder.Property(user => user.City).HasMaxLength(50).IsRequired(false);
        builder.Property(user => user.PostalCode).HasMaxLength(50).IsRequired(false);
        builder.Property(user => user.Country).HasMaxLength(50).IsRequired(false);

        builder.Property(user => user.Email).HasMaxLength(256).IsRequired();
        builder.Property(user => user.NormalizedEmail).HasMaxLength(256).IsRequired();

        builder.Property(user => user.UserName).HasMaxLength(256).IsRequired();
        builder.Property(user => user.NormalizedUserName).HasMaxLength(256).IsRequired();

        builder.Property(user => user.PasswordHash).HasColumnType("NVARCHAR(MAX)").IsRequired();
        builder.Property(user => user.SecurityStamp).HasColumnType("NVARCHAR(MAX)").IsRequired();
        builder.Property(user => user.ConcurrencyStamp).HasColumnType("NVARCHAR(MAX)").IsRequired();

        builder.Property(user => user.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(user => user.RegistrationDate).HasDefaultValueSql("getutcdate()").IsRequired();

        builder.Property(user => user.RefreshToken).HasMaxLength(512).IsRequired(false);
        builder.Property(user => user.RefreshTokenExpirationDate).IsRequired(false);
    }
}