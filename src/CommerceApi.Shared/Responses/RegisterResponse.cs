using System.Text.Json.Serialization;

namespace CommerceApi.Shared.Responses;

public class RegisterResponse
{
    [JsonConstructor]
    public RegisterResponse()
    {
    }

    public RegisterResponse(bool succeeded, IEnumerable<string>? errors)
    {
        Succeeded = succeeded;
        Errors = errors;
    }


    public bool Succeeded { get; init; }

    public IEnumerable<string>? Errors { get; init; }
}