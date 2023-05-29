namespace CommerceApi.Shared.Models.Requests;

public class SaveReviewRequest
{
    public Guid ProductId { get; set; }

    public string Text { get; set; } = null!;

    public int Score { get; set; }
}