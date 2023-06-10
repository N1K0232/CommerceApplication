using CommerceApi.Shared.Models.Common;

namespace CommerceApi.Shared.Models;

public class Image : BaseObject
{
    public string FileName { get; set; } = null!;

    public string Path { get; set; } = null!;

    public string Title { get; set; } = null!;

    public long Length { get; set; }

    public string ContentType { get; set; } = null!;

    public string Extension { get; set; } = null!;

    public string DownloadFileName { get; set; } = null!;

    public string DownloadPath { get; set; } = null!;

    public string? Description { get; set; }
}