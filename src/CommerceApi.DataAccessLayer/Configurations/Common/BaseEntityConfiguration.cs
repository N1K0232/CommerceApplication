using CommerceApi.DataAccessLayer.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommerceApi.DataAccessLayer.Configurations.Common;

public abstract class BaseEntityConfiguration<T> : IEntityTypeConfiguration<T> where T : BaseEntity
{
    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).IsRequired().ValueGeneratedOnAdd().HasDefaultValueSql("newid()");

        builder.Property(e => e.SecurityStamp).HasColumnType("NVARCHAR(MAX)").IsRequired().ValueGeneratedOnAdd();
        builder.Property(e => e.ConcurrencyStamp).HasColumnType("NVARCHAR(MAX)").IsRequired().ValueGeneratedOnAdd();

        builder.Property(e => e.CreationDate).IsRequired().ValueGeneratedOnAdd().HasDefaultValueSql("getutcdate()");
        builder.Property(e => e.CreationTime).IsRequired().ValueGeneratedOnAdd();

        builder.Property(e => e.LastModificationDate).IsRequired(false).ValueGeneratedOnUpdate();
        builder.Property(e => e.LastModificationTime).IsRequired(false).ValueGeneratedOnUpdate();
    }
}