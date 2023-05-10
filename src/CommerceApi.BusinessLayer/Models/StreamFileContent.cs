namespace CommerceApi.BusinessLayer.Models;

public record StreamFileContent(Stream Stream, string FileName, long Length, string ContentType)
{
    public string Title { get; set; }

    public string Description { get; set; }
}