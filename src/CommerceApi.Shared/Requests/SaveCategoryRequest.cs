namespace CommerceApi.Shared.Requests;

public class SaveCategoryRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}