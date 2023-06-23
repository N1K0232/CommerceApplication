using CommerceApi.DataAccessLayer.Entities.Common;

namespace CommerceApi.DataAccessLayer.Entities;

public class Question : BaseEntity
{
    public Guid ProductId { get; set; }

    public Guid UserId { get; set; }

    public string Text { get; set; }

    public DateTime Date { get; set; }

    public bool IsPublished { get; set; }

    public Product Product { get; set; }
}