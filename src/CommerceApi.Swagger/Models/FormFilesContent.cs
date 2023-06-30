using Microsoft.AspNetCore.Http;

namespace CommerceApi.Swagger.Models;

public class FormFilesContent
{
    public FormFilesContent()
    {
    }

    public IEnumerable<IFormFile> Files { get; internal set; } = null!;

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

        if (files == null || !files.Any())
        {
            return null;
        }

        var content = new FormFilesContent { Files = files };
        return content;
    }
}