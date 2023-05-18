using CommerceApi.Filters;

namespace CommerceApi.Models;

public class FormFileContent
{
    public FormFileContent([AllowedExtensions("*.jpg", "*.jpeg", "*.png")] IFormFile file)
    {
        File = file;
    }

    public IFormFile File { get; }

    public string FileName => File.FileName;

    public Stream GetFileStream() => File.OpenReadStream();

    public static async ValueTask<FormFileContent> BindAsync(HttpContext context)
    {
        var request = context.Request;
        if (!request.HasFormContentType)
        {
            return null;
        }

        var form = await request.ReadFormAsync();
        var file = form.Files?.ElementAtOrDefault(0);

        if (file is null)
        {
            return null;
        }

        var content = new FormFileContent(file);
        return content;
    }
}