using System.Text.Json;
using System.Text.Json.Serialization;

namespace CommerceApi.ClientContext.Converters;

public class TimeOnlyConverter : JsonConverter<TimeOnly>
{
    public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return TimeOnly.Parse(value);
    }

    public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
    {
        var input = value.ToString("HH:mm:ss:fff");
        writer.WriteStringValue(input);
    }
}