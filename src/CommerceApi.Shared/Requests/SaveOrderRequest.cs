using CommerceApi.Shared.Common;
using CommerceApi.Shared.Enums;

namespace CommerceApi.Shared.Requests;

public class SaveOrderRequest : BaseRequestModel
{
    public Guid ProductId { get; set; }

    public int OrderedQuantity { get; set; }

    public OrderStatus? Status { get; set; }
}