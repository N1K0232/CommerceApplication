using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CommerceApi.DataAccessLayer.Converters;

public class DateOnlyConverter : ValueConverter<DateOnly, DateTime>
{
    public DateOnlyConverter() : base(dateOnly => dateOnly.ToDateTime(TimeOnly.MinValue),
                                      dateTime => DateOnly.FromDateTime(dateTime))
    {
    }
}