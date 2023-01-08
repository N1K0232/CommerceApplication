using CommerceApi.DataAccessLayer.Entities.Common;

namespace CommerceApi.DataAccessLayer.Entities;

public class Image : BaseEntity
{
    public string FileName { get; set; }

    public string Path { get; set; }

    public long Length { get; set; }

    public string ContentType { get; set; }

    public string Description { get; set; }
}