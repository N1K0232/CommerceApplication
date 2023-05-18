using CommerceApi.Shared.Enums;
using CommerceApi.Shared.Models.Common;

namespace CommerceApi.Shared.Models;

public class Order : BaseObject
{
    public string User { get; set; } = null!;

    public DateTime Date { get; set; }

    public TimeSpan Time { get; set; }

    public OrderStatus Status { get; set; }
}