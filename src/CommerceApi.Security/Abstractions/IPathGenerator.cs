namespace CommerceApi.Security.Abstractions
{
    public interface IPathGenerator
    {
        string? Generate(string? fileName);
    }
}