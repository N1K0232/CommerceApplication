using CommerceApi.Shared.Common;

namespace CommerceApi.Shared.Models;

public class Category : BaseModel
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }
}