namespace CommerceApi.Shared.Common;

public class ListResult<T> where T : BaseModel
{
    public IEnumerable<T> Content { get; set; }

    public int TotalCount { get; set; }

    public bool HasNextPage { get; set; }
}