using System.Text.Json.Serialization;

namespace CommerceApi.Shared.Common;

public class ListResult<T> where T : BaseModel
{
    [JsonConstructor]
    public ListResult()
    {
    }

    public ListResult(IEnumerable<T>? content) : this(content, content?.Count() ?? 0, false)
    {
    }

    public ListResult(IEnumerable<T>? content, int totalCount, bool hasNextPage = false)
    {
        Content = content;
        TotalCount = totalCount;
        HasNextPage = hasNextPage;
    }


    public IEnumerable<T>? Content { get; init; }

    public int TotalCount { get; init; }

    public bool HasNextPage { get; init; }
}