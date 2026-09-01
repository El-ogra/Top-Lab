using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.UsersAndPermissions.Common;
using TopLab.Domain.Users;

namespace TopLab.Application.Features.UsersAndPermissions.Commands.SignIn;

public sealed class SignInCommandHandler : IRequestHandler<SignInCommand, Result<CurrentUserSessionDto>>
{
    private static readonly Error InvalidCredentialsError = Error.Forbidden("اسم المستخدم أو كلمة المرور غير صحيحة");
    private static readonly Error InactiveUserError = Error.Forbidden("المستخدم غير مفعل");

    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _dateTime;

    public SignInCommandHandler(
        IApplicationDbContext db,
        IPasswordHasher hasher,
        ICurrentUserService currentUser,
        IDateTimeProvider dateTime)
    {
        _db = db;
        _hasher = hasher;
        _currentUser = currentUser;
        _dateTime = dateTime;
    }

    public async Task<Result<CurrentUserSessionDto>> Handle(SignInCommand request, CancellationToken cancellationToken)
    {
        var user = _db.Set<User>().FirstOrDefault(u => u.UserName == request.UserName);

        if (user is null)
        {
            return Result<CurrentUserSessionDto>.Failure(InvalidCredentialsError);
        }

        if (!_hasher.Verify(request.Password, user.PasswordHash))
        {
            return Result<CurrentUserSessionDto>.Failure(InvalidCredentialsError);
        }

        if (!user.IsActive)
        {
            return Result<CurrentUserSessionDto>.Failure(InactiveUserError);
        }

        user.RecordLogin(_dateTime.UtcNow);

        var grantedCodes = ResolveGrantedCodes(user);

        await _db.SaveChangesAsync(cancellationToken);

        _currentUser.SetSession(user.Id.Value, user.UserName, user.IsAbsolutePermission, grantedCodes);

        var dto = new CurrentUserSessionDto(
            user.Id.Value,
            user.UserName,
            user.IsAbsolutePermission,
            grantedCodes.ToList(),
            user.LastLoginAtUtc);

        return Result<CurrentUserSessionDto>.Success(dto);
    }

    private IReadOnlyList<string> ResolveGrantedCodes(User user)
    {
        if (user.IsAbsolutePermission)
        {
            // Absolute users bypass checks, but we still resolve granted codes for session completeness.
            // Permissions are stored as grants; for absolute users we return whatever is granted plus
            // the session's IsAbsolute flag will bypass. Alternatively return all catalog codes.
            // To keep consistent with plan: populate with granted permission codes resolved by joining grants.
        }

        var userPermissionIds = _db.Set<UserPermissionGrant>()
            .Where(g => g.UserId.Equals(user.Id))
            .Select(g => g.PermissionId)
            .ToList();

        if (userPermissionIds.Count == 0)
        {
            return Array.Empty<string>();
        }

        var codes = _db.Set<Permission>()
            .Where(p => userPermissionIds.Contains(p.Id))
            .Select(p => p.Code)
            .ToList();

        return codes;
    }
}
