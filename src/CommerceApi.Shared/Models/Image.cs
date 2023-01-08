using CommerceApi.Shared.Common;

namespace CommerceApi.Shared.Models;

public class Image : BaseModel
{
    public string FileName { get; set; }

    public string Path { get; set; }

    public long Length { get; set; }

    public string ContentType { get; set; }

    public string Description { get; set; }
}