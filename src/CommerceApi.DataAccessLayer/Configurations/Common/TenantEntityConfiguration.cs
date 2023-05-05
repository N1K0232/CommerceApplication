using CommerceApi.DataAccessLayer.Entities.Common;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommerceApi.DataAccessLayer.Configurations.Common;

public abstract class TenantEntityConfiguration<T> : DeletableEntityConfiguration<T> where T : TenantEntity
{
    public override void Configure(EntityTypeBuilder<T> builder)
    {
        builder.Property(e => e.TenantId).IsRequired();
        base.Configure(builder);
    }
}