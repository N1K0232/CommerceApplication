namespace CommerceApi.Shared.Models.Requests;

public class SaveInvoiceRequest
{
    public Guid ProductId { get; set; }

    public decimal Price { get; set; }

    public int Quantity { get; set; }
}