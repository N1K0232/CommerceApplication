using System.Data.Common;
using System.Reflection;

namespace CommerceApi.DataAccessLayer.Extensions;

public static class DbDataReaderExtensions
{
    public static async Task<IList<T>> ToListAsync<T>(this DbDataReader reader) where T : class
    {
        var result = new List<T>();
        var properties = typeof(T).GetRuntimeProperties();

        var schema = await reader.GetColumnSchemaAsync().ConfigureAwait(false);
        var columnMapping = schema.Where(s => properties.Any(p => p.Name.ToLowerInvariant() == s.ColumnName.ToLowerInvariant()))
            .ToDictionary(k => k.ColumnName.ToLowerInvariant());

        if (reader.HasRows)
        {
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var item = Activator.CreateInstance<T>();
                foreach (var property in properties)
                {
                    var value = reader.GetValue(columnMapping[property.Name.ToLower()].ColumnOrdinal.Value);
                    property.SetValue(item, value == DBNull.Value ? null : value);
                }

                result.Add(item);
            }
        }

        return result;
    }
}