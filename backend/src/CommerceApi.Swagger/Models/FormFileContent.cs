using CommerceApi.Swagger.Filters;
using Microsoft.AspNetCore.Http;

namespace CommerceApi.Swagger.Models;

public class FormFileContent
{
    public FormFileContent(IFormFile file)
    {
        File = file;
    }

    [AllowedExtensions("*.jpg", "*.jpeg", "*.png")]
    public IFormFile File { get; }

    public static async ValueTask<FormFileContent?> BindAsync(HttpContext httpContext)
    {
        var httpRequest = httpContext.Request;
        if (!httpRequest.HasFormContentType)
        {
            return null;
        }

        var form = await httpRequest.ReadFormAsync().ConfigureAwait(false);
        if (form == null)
        {
            return null;
        }

        var file = form.Files[0];
        if (file == null)
        {
            return null;
        }

        var content = new FormFileContent(file);
        return content;
    }
}