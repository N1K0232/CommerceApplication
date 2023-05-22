namespace CommerceApi.DataAccessLayer.Entities.Common;

public abstract class DeletableEntity : BaseEntity
{
    public bool IsDeleted { get; set; }

    public DateOnly? DeletedDate { get; set; }

    public TimeOnly? DeletedTime { get; set; }
}