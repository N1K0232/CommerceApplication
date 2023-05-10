namespace CommerceApi.DataAccessLayer.Entities.Common;

public abstract class FileEntity : BaseEntity
{
    public string FileName { get; set; }

    public string Path { get; set; }

    public long Length { get; set; }

    public string ContentType { get; set; }

    public string DownloadFileName { get; set; }
}