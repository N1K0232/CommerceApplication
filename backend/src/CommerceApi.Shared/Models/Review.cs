using CommerceApi.Shared.Models.Common;

namespace CommerceApi.Shared.Models;

public class Review : BaseObject
{
    public User User { get; set; } = null!;

    public string Text { get; set; } = null!;

    public int Score { get; set; }
}