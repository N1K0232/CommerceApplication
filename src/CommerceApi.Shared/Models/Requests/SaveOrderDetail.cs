namespace CommerceApi.Shared.Models.Requests;

public class SaveOrderDetail
{
    public Guid OrderId { get; set; }

    public Guid ProductId { get; set; }

    public int Quantity { get; set; }
}