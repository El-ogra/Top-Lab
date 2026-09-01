using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Users;

namespace TopLab.Application.Features.UsersAndPermissions.Commands.CreateUser;

public sealed class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<int>>
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _hasher;

    public CreateUserCommandHandler(IApplicationDbContext db, IPasswordHasher hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    public async Task<Result<int>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        if (_db.Set<User>().Any(u => u.UserName == request.UserName))
        {
            return Result<int>.Failure(Error.Conflict("اسم المستخدم موجود بالفعل"));
        }

        var permissionMap = new Dictionary<string, PermissionId>();
        foreach (var code in request.PermissionCodes)
        {
            var perm = _db.Set<Permission>().FirstOrDefault(p => p.Code == code);
            if (perm is null)
            {
                return Result<int>.Failure(Error.Validation($"رمز الصلاحية غير معروف: {code}"));
            }

            permissionMap[code] = perm.Id;
        }

        string passwordHash;
        string secondaryHash;
        try
        {
            passwordHash = _hasher.Hash(request.Password);
            secondaryHash = _hasher.Hash(request.SecondaryPassword);
        }
        catch
        {
            return Result<int>.Failure(Error.Unexpected("فشل تشفير كلمة المرور."));
        }

        int newId = _db.Set<User>().Any() ? _db.Set<User>().Max(u => u.Id.Value) + 1 : 1;

        var user = User.Create(
            UserId.Create(newId),
            request.UserName,
            passwordHash,
            secondaryHash,
            request.IsAbsolutePermission,
            request.DiscountLimitPercent,
            request.BlockPrintOnRemainingBalance,
            request.WorkStartTime,
            request.WorkEndTime,
            request.HasBreakPeriod,
            request.BreakDurationMinutes);

        foreach (var permissionId in permissionMap.Values)
        {
            user.GrantPermission(permissionId);
        }

        try
        {
            _db.Add(user);

            foreach (var grant in user.PermissionGrants)
            {
                _db.Add(grant);
            }

            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex) when (IsUniqueViolation(ex))
        {
            return Result<int>.Failure(Error.Conflict("اسم المستخدم موجود بالفعل"));
        }

        return Result<int>.Success(user.Id.Value);
    }

    private static bool IsUniqueViolation(Exception ex)
    {
        var msg = ex.Message;
        return msg.Contains("IX_Users_UserName") || msg.Contains("duplicate") || msg.Contains("UNIQUE");
    }
}
