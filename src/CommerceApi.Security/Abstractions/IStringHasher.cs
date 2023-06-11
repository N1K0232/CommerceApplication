namespace CommerceApi.Security.Abstractions
{
    public interface IStringHasher
    {
        string? GetString(string? input);
    }
}