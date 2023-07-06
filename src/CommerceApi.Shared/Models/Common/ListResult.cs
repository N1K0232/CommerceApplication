using System.Text.Json.Serialization;

namespace CommerceApi.Shared.Models.Common;

public class ListResult<T> where T : BaseObject
{
    [JsonConstructor]
    public ListResult()
    {
    }

    public ListResult(IEnumerable<T>? content) : this(content, content?.Count() ?? 0L, 1L, false)
    {
    }

    public ListResult(IEnumerable<T>? content, long totalCount, long totalPages, bool hasNextPage = false)
    {
        Content = content;
        TotalCount = totalCount;
        TotalPages = totalPages;
        HasNextPage = hasNextPage;
    }


    public IEnumerable<T>? Content { get; init; }

    public long TotalCount { get; init; }

    public long TotalPages { get; init; }

    public bool HasNextPage { get; init; }
}