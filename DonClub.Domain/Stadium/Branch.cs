using Donclub.Domain.Common;

namespace Donclub.Domain.Stadium;

public class Branch : BaseEntity<int>, IAuditableEntity
{
    public string Name { get; set; } = default!;
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    public ICollection<Room> Rooms { get; set; } = new List<Room>();
}

public class Room : BaseEntity<int>, IAuditableEntity
{
    public int BranchId { get; set; }
    public string Name { get; set; } = default!;
    public int? Capacity { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    public Branch Branch { get; set; } = default!;
}
