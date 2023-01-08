namespace CommerceApi.BusinessLayer.Models;

public record StreamFileContent
{
    public StreamFileContent()
    {
    }

    public StreamFileContent(Stream stream, string fileName, long length, string contentType, string description = null)
    {
        Stream = stream;
        FileName = fileName;
        Length = length;
        ContentType = contentType;
        Description = description;
    }


    public Stream Stream { get; init; }

    public string FileName { get; init; }

    public long Length { get; init; }

    public string ContentType { get; init; }

    public string Description { get; init; }
}