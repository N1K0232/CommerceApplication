namespace CommerceApi.Shared.Models.Requests;

public class FileChunk
{
    public string FileHandle { get; set; } = null!;

    public string Data { get; set; } = null!;

    public long StartAt { get; set; }
}