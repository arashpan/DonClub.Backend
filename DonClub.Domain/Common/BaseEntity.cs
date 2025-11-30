namespace Donclub.Domain.Common;

public abstract class BaseEntity<TId>
{
    public TId Id { get; set; } = default!;
}

public interface IAuditableEntity
{
    DateTime CreatedAtUtc { get; set; }
    DateTime? UpdatedAtUtc { get; set; }
}

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAtUtc { get; set; }
}
