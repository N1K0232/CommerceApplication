using CommerceApi.BusinessLayer.Models;
using CommerceApi.Requests;

namespace CommerceApi.Extensions;

public static class UploadFileRequestExtensions
{
    public static StreamFileContent ToStreamFileContent(this UploadImageRequest request)
    {
        var content = new StreamFileContent
        (
            request.File.OpenReadStream(),
            request.File.FileName,
            request.File.Length,
            request.File.ContentType,
            request.Description
        );

        return content;
    }
}