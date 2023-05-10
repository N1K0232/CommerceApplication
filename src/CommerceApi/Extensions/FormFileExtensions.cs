using CommerceApi.BusinessLayer.Models;
using CommerceApi.Models;

namespace CommerceApi.Extensions;

public static class FormFileExtensions
{
    public static IEnumerable<string> ToFileNames(this IEnumerable<IFormFile> files)
    {
        var fileNames = new List<string>(files.Count());
        foreach (var file in files)
        {
            fileNames.Add(file.FileName);
        }

        return fileNames;
    }

    public static IEnumerable<Stream> ToFileStreams(this IEnumerable<IFormFile> files)
    {
        var fileStreams = new List<Stream>(files.Count());
        foreach (var file in files)
        {
            fileStreams.Add(file.OpenReadStream());
        }

        return fileStreams;
    }

    public static StreamFileContent ToStreamFileContent(this FormFileContent content)
    {
        var file = content.File;

        var fileContent = new StreamFileContent(file.OpenReadStream(), file.FileName, file.Length, file.ContentType);
        return fileContent;
    }
}