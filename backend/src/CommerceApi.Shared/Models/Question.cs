using CommerceApi.Shared.Models.Common;

namespace CommerceApi.Shared.Models;

public class Question : BaseObject
{
    public Product Product { get; set; } = null!;

    public User User { get; set; } = null!;

    public string Text { get; set; } = null!;

    public DateTime Date { get; set; }

    public bool IsPublished { get; set; }
}