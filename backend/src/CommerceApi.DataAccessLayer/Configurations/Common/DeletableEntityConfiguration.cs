using CommerceApi.DataAccessLayer.Entities.Common;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommerceApi.DataAccessLayer.Configurations.Common;

public abstract class DeletableEntityConfiguration<T> : BaseEntityConfiguration<T> where T : DeletableEntity
{
    public override void Configure(EntityTypeBuilder<T> builder)
    {
        builder.Property(e => e.IsDeleted).IsRequired();
        builder.Property(e => e.DeletedDate).IsRequired(false);
        builder.Property(e => e.DeletedTime).IsRequired(false);

        base.Configure(builder);
    }
}