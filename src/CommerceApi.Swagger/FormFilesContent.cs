using Microsoft.AspNetCore.Http;

namespace CommerceApi.Swagger;

public class FormFilesContent
{
    public FormFilesContent(IEnumerable<IFormFile> files)
    {
        Files = files;
    }

    public IEnumerable<IFormFile> Files { get; }

    public IEnumerable<Stream> GetFileStreams()
    {
        var streams = new List<Stream>();
        var files = Files;

        foreach (var file in files)
        {
            streams.Add(file.OpenReadStream());
        }

        return streams;
    }

    public static async ValueTask<FormFilesContent?> BindAsync(HttpContext context)
    {
        var request = context.Request;
        if (!request.HasFormContentType)
        {
            return null;
        }

        var form = await request.ReadFormAsync().ConfigureAwait(false);
        var files = form.Files;

        var hasItems = files?.Any() ?? false;
        if (!hasItems)
        {
            return null;
        }

        var content = new FormFilesContent(files);
        return content;
    }
}