using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Users;

namespace TopLab.Application.Features.UsersAndPermissions.Commands.SaveUserPermissions;

public sealed class SaveUserPermissionsCommandHandler : IRequestHandler<SaveUserPermissionsCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public SaveUserPermissionsCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(SaveUserPermissionsCommand request, CancellationToken cancellationToken)
    {
        var user = _db.Set<User>().FirstOrDefault(u => u.Id.Value == request.UserId);
        if (user is null)
        {
            return Result.Failure(Error.NotFound("المستخدم غير موجود"));
        }

        var permissionMap = new Dictionary<string, Permission>();
        foreach (var code in request.PermissionCodes)
        {
            var perm = _db.Set<Permission>().FirstOrDefault(p => p.Code == code);
            if (perm is null)
            {
                return Result.Failure(Error.Validation($"رمز الصلاحية غير معروف: {code}"));
            }

            permissionMap[code] = perm;
        }

        var existingGrants = _db.Set<UserPermissionGrant>().Where(g => g.UserId.Equals(user.Id)).ToList();

        user.ClearPermissions();
        foreach (var grant in existingGrants)
        {
            _db.Remove(grant);
        }

        foreach (var perm in permissionMap.Values)
        {
            user.GrantPermission(perm.Id);
        }

        foreach (var grant in user.PermissionGrants)
        {
            _db.Add(grant);
        }

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
