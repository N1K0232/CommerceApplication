using CommerceApi.Authentication.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommerceApi.Authentication.Configurations;

public class AuthenticationUserConfiguration : IEntityTypeConfiguration<AuthenticationUser>
{
    public void Configure(EntityTypeBuilder<AuthenticationUser> builder)
    {
        builder.Property(user => user.FirstName).HasMaxLength(256).IsRequired();
        builder.Property(user => user.LastName).HasMaxLength(256).IsRequired();
        builder.Property(user => user.DateOfBirth).IsRequired();

        builder.Property(user => user.RefreshToken).HasMaxLength(512).IsRequired(false);
        builder.Property(user => user.RefreshTokenExpirationDate).IsRequired(false);
    }
}