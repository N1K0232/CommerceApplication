namespace CommerceApi.Security;

public static class PathGenerator
{
    public static string Generate(string fileName)
    {
        var now = DateTime.UtcNow;
        var year = now.Year.ToString("0000");
        var month = now.Month.ToString("00");
        var day = now.Day.ToString("00");

        return Path.Combine(year, month, day, fileName);
    }
}