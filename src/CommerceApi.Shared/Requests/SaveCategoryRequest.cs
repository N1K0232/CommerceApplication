namespace CommerceApi.Shared.Requests;

public class SaveCategoryRequest
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }
}