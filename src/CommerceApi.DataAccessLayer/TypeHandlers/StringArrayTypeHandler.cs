using System.Data;
using Dapper;

namespace CommerceApi.DataAccessLayer.TypeHandlers;

internal class StringArrayTypeHandler : SqlMapper.TypeHandler<string[]>
{
    private const string defaultSeparator = ";";

    private readonly string _separator;

    public StringArrayTypeHandler(string separator = null)
    {
        _separator = separator ?? defaultSeparator;
    }

    public override string[] Parse(object value)
    {
        var values = value.ToString().Split(_separator, StringSplitOptions.RemoveEmptyEntries);
        return values;
    }

    public override void SetValue(IDbDataParameter parameter, string[] value)
    {
        parameter.Value = string.Join(_separator, value);
    }
}