using CommerceApi.Shared.Enums;

namespace CommerceApi.Shared.Models.Requests;

public class UpdateOrderStatusRequest
{
    public Guid OrderId { get; set; }

    public OrderStatus Status { get; set; }
}