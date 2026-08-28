using TopLab.Domain.Common;
using TopLab.Domain.Common.Ids;

namespace TopLab.Domain.Users;

public sealed class Permission : Entity<PermissionId>
{
    public string Code { get; private set; } = default!;

    public string Description { get; private set; } = default!;

    private Permission()
    {
    }

    private Permission(PermissionId id, string code, string description)
        : base(id)
    {
        Code = code;
        Description = description;
    }

    public static Permission Create(PermissionId id, string code, string description)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Code is required.", nameof(code));
        }

        return new Permission(id, code.Trim(), description);
    }
}
