using CommerceApi.Shared.Common;

namespace CommerceApi.Shared.Requests;

public class SaveCategoryRequest : BaseRequestModel
{
    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;
}