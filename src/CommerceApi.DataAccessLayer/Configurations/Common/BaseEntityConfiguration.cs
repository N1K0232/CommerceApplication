using CommerceApi.DataAccessLayer.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommerceApi.DataAccessLayer.Configurations.Common;

public abstract class BaseEntityConfiguration<T> : IEntityTypeConfiguration<T> where T : BaseEntity
{
    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).IsRequired().ValueGeneratedOnAdd().HasDefaultValueSql("newid()");

        builder.Property(x => x.CreationDate).IsRequired().ValueGeneratedOnAdd().HasDefaultValueSql("getutcdate()");
        builder.Property(x => x.LastModificationDate).IsRequired(false).ValueGeneratedOnUpdate();
    }
}