using OperationResults;

namespace CommerceApi.Extensions;

public static class FormFileExtensions
{
    public static StreamFileContent ToStreamFileContent(this IFormFile file)
    {
        var content = new StreamFileContent(file.OpenReadStream(), file.ContentType, file.FileName);
        return content;
    }
}