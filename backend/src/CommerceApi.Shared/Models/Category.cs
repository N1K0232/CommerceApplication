using CommerceApi.Shared.Models.Common;

namespace CommerceApi.Shared.Models;

public class Category : BaseObject
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }
}