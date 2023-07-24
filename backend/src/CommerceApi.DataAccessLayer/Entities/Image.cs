using CommerceApi.DataAccessLayer.Entities.Common;

namespace CommerceApi.DataAccessLayer.Entities;

public class Image : BaseEntity
{
    public string Title { get; set; }

    public string Description { get; set; }

    public string DownloadFileName { get; set; }

    public string ContentType { get; set; }

    public string Extension { get; set; }

    public long Length { get; set; }
}