using CommerceApi.DataAccessLayer.Entities.Common;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommerceApi.DataAccessLayer.Configurations.Common;

public abstract class DeletableEntityConfiguration<T> : BaseEntityConfiguration<T> where T : DeletableEntity
{
    public override void Configure(EntityTypeBuilder<T> builder)
    {
        builder.Property(x => x.IsDeleted).IsRequired();
        builder.Property(x => x.DeletedDate).IsRequired(false);

        base.Configure(builder);
    }
}