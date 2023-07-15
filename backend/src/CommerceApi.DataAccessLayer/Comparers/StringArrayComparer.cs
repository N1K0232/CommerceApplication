using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CommerceApi.DataAccessLayer.Comparers;

public class StringArrayComparer : ValueComparer<IEnumerable<string>>
{
    public StringArrayComparer()
        : base((firstList, secondList) => (firstList ?? Array.Empty<string>()).SequenceEqual(secondList ?? Array.Empty<string>()),
            list => list == null ? 0 : list.GetHashCode())
    {
    }
}