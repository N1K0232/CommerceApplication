using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TinyHelpers.Extensions;

namespace CommerceApi.DataAccessLayer.Converters;

public class StringArrayConverter : ValueConverter<IEnumerable<string>, string>
{
    public StringArrayConverter() : this(";")
    {
    }

    public StringArrayConverter(string separator)
        : base(list => list.HasItems() ? string.Join(separator, list) : null,
               value => value.HasItems() ? value.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries) : Enumerable.Empty<string>())
    {
    }
}