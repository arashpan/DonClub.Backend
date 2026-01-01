using Donclub.Domain.Common;

namespace Donclub.Domain.Users;

public enum MembershipLevel : byte
{
    Guest = 0,
    Member = 1,
    Vip = 2,
    Cip = 3
}

public class User : BaseEntity<long>, IAuditableEntity, ISoftDeletable
{
    public string UserName { get; set; } = default!;
    public string? DisplayName { get; set; }
    public string? Email { get; set; }

    public string PhoneNumber { get; set; } = default!;
    public bool PhoneNumberConfirmed { get; set; }

    public string? PasswordHash { get; set; }   // برای پروژه‌هایی که پسورد نیاز دارن
    public MembershipLevel MembershipLevel { get; set; } = MembershipLevel.Guest;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
	public string UserCode { get; set; } = default!; // کد 6 رقمی یکتا

	public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}

public class Role : BaseEntity<int>
{
    public string Name { get; set; } = default!;
}

public class UserRole
{
    public long UserId { get; set; }
    public int RoleId { get; set; }

    public User User { get; set; } = default!;
    public Role Role { get; set; } = default!;
}
