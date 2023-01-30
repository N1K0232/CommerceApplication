using CommerceApi.Shared.Common;

namespace CommerceApi.Shared.Requests;

public class SaveSupplierRequest : BaseRequestModel
{
    public string CompanyName { get; set; } = null!;

    public string ContactName { get; set; } = null!;

    public string City { get; set; } = null!;
}