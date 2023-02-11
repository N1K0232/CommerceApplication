using CommerceApi.Shared.Common;

namespace CommerceApi.Shared.Requests;

public class SaveSupplierRequest : BaseRequestModel
{
    public string CompanyName { get; set; } = string.Empty;

    public string ContactName { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;
}