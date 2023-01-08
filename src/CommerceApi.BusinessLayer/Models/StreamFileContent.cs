namespace CommerceApi.BusinessLayer.Models;

public record StreamFileContent(Stream Stream, string FileName, long Length, string ContentType, string Description = null);