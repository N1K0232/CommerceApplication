using CommerceApi.DataAccessLayer.Entities.Common;

namespace CommerceApi.DataAccessLayer.Entities;

public class Review : BaseEntity
{
    public Guid UserId { get; set; }

    public Guid ProductId { get; set; }

    public string Text { get; set; }

    public int Score { get; set; }

    public DateTime ReviewDate { get; set; }

    public virtual Product Product { get; set; }
}