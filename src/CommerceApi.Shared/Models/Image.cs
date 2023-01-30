using CommerceApi.Shared.Common;

namespace CommerceApi.Shared.Models;

public class Image : BaseModel
{
    public string? FileName { get; init; }

    public string? Path { get; init; }

    public long? Length { get; init; }

    public string? ContentType { get; init; }

    public string? Description { get; init; }
}