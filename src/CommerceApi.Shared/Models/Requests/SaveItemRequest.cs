namespace CommerceApi.Shared.Models.Requests;

public class SaveItemRequest
{
    public Guid CartId { get; set; }

    public Guid ProductId { get; set; }

    public int Quantity { get; set; }
}