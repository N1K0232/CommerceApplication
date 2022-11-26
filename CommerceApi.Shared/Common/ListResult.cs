namespace CommerceApi.Shared.Common;

public class ListResult<T> where T : BaseModel
{
    public ListResult(IEnumerable<T> content)
    {
        Content = content;
        TotalCount = content?.Count() ?? 0;
        HasNextPage = false;
    }
    public ListResult(IEnumerable<T> content, int totalCount, bool hasNextPage = false)
    {
        Content = content;
        TotalCount = totalCount;
        HasNextPage = hasNextPage;
    }

    public IEnumerable<T> Content { get; set; }

    public int TotalCount { get; set; }

    public bool HasNextPage { get; set; }
}