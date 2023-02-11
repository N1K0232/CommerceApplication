using System.Text.Json.Serialization;

namespace CommerceApi.Shared.Common;

public class ListResult<T> where T : BaseModel
{
    [JsonConstructor]
    public ListResult()
    {
    }

    public ListResult(IEnumerable<T>? content)
    {
        Content = content;
        TotalCount = content?.LongCount() ?? 0L;
        HasNextPage = false;
    }

    public ListResult(IEnumerable<T>? content, long totalCount, bool hasNextPage = false)
    {
        Content = content;
        TotalCount = totalCount;
        HasNextPage = hasNextPage;
    }


    public IEnumerable<T>? Content { get; init; }

    public long TotalCount { get; init; }

    public bool HasNextPage { get; init; }
}