namespace CommerceApi.ClientContext;

public class ClientContextOptions
{
    private const string DefaultTimeZoneHeader = "X-TimeZone";

    public string TimeZoneHeader { get; set; } = DefaultTimeZoneHeader;

    public TimeZoneInfo DefaultTimeZone { get; set; } = TimeZoneInfo.Utc;
}