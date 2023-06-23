using CommerceApi.DataAccessLayer.Configurations.Common;
using CommerceApi.DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommerceApi.DataAccessLayer.Configurations;

public class QuestionConfiguration : BaseEntityConfiguration<Question>
{
    public override void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.Property(q => q.UserId).IsRequired();
        builder.Property(q => q.Text).HasMaxLength(4000).IsRequired();
        builder.Property(q => q.Date).IsRequired();
        builder.Property(q => q.IsPublished).IsRequired();

        builder.HasOne(q => q.Product).WithMany(p => p.Questions).HasForeignKey(q => q.ProductId).IsRequired();

        builder.ToTable("Questions");
        base.Configure(builder);
    }
}