using CommerceApi.Shared.Common;

namespace CommerceApi.Shared.Requests;

public class SaveSupplierRequest : BaseRequestModel
{
    public string CompanyName { get; set; }

    public string ContactName { get; set; }

    public string City { get; set; }
}