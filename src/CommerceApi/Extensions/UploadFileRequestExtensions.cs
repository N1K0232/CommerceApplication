using CommerceApi.BusinessLayer.Models;
using CommerceApi.Requests;

namespace CommerceApi.Extensions;

public static class UploadFileRequestExtensions
{
    public static StreamFileContent ToStreamFileContent(this UploadImageRequest request)
    {
        var file = request.File;
        var content = new StreamFileContent(file.OpenReadStream(), file.FileName, file.Length, file.ContentType)
        {
            Title = request.Title,
            Description = request.Description
        };

        return content;
    }
}