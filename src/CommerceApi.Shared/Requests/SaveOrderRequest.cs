using CommerceApi.Shared.Common;

namespace CommerceApi.Shared.Requests;

public class SaveOrderRequest : BaseRequestModel
{
    public Guid ProductId { get; set; }

    public int OrderedQuantity { get; set; }
}