namespace CommerceApi.DataAccessLayer.Entities.Common;

public abstract class BaseEntity
{
    public Guid Id { get; set; }

    public string SecurityStamp { get; set; }

    public string ConcurrencyStamp { get; set; }

    public DateOnly CreationDate { get; set; }

    public TimeOnly CreationTime { get; set; }

    public DateOnly? LastModificationDate { get; set; }

    public TimeOnly? LastModificationTime { get; set; }
}