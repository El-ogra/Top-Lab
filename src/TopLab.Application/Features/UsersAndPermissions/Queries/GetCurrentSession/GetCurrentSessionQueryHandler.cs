using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.UsersAndPermissions.Common;
using TopLab.Domain.Users;

namespace TopLab.Application.Features.UsersAndPermissions.Queries.GetCurrentSession;

public sealed class GetCurrentSessionQueryHandler : IRequestHandler<GetCurrentSessionQuery, Result<CurrentUserSessionDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetCurrentSessionQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public Task<Result<CurrentUserSessionDto>> Handle(GetCurrentSessionQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated)
        {
            return Task.FromResult(Result<CurrentUserSessionDto>.Failure(Error.Forbidden("غير مصرح")));
        }

        var user = _db.Set<User>().FirstOrDefault(u => u.Id.Value == _currentUser.UserId);

        DateTime? lastLogin = user?.LastLoginAtUtc;

        var grantedCodes = ResolveGrantedCodes(_currentUser.UserId);

        var dto = new CurrentUserSessionDto(
            _currentUser.UserId,
            _currentUser.UserName,
            _currentUser.IsAbsolutePermission,
            grantedCodes,
            lastLogin);

        return Task.FromResult(Result<CurrentUserSessionDto>.Success(dto));
    }

    private IReadOnlyList<string> ResolveGrantedCodes(int userId)
    {
        var userIdObj = TopLab.Domain.Common.Ids.UserId.Create(userId);
        var permissionIds = _db.Set<UserPermissionGrant>()
            .Where(g => g.UserId.Equals(userIdObj))
            .Select(g => g.PermissionId)
            .ToList();

        if (permissionIds.Count == 0)
        {
            return Array.Empty<string>();
        }

        var codes = _db.Set<Permission>()
            .Where(p => permissionIds.Contains(p.Id))
            .Select(p => p.Code)
            .ToList();

        return codes;
    }
}
