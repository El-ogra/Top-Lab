using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.UsersAndPermissions.Common;
using TopLab.Domain.Users;

namespace TopLab.Application.Features.UsersAndPermissions.Queries.GetUserById;

public sealed class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserDetailDto>>
{
    private readonly IApplicationDbContext _db;

    public GetUserByIdQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<UserDetailDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = _db.Set<User>().FirstOrDefault(u => u.Id.Value == request.UserId);
        if (user is null)
        {
            return Task.FromResult(Result<UserDetailDto>.Failure(Error.NotFound("المستخدم غير موجود")));
        }

        var permissionIds = _db.Set<UserPermissionGrant>()
            .Where(g => g.UserId.Equals(user.Id))
            .Select(g => g.PermissionId)
            .ToList();

        var codes = permissionIds.Count == 0
            ? new List<string>()
            : _db.Set<Permission>().Where(p => permissionIds.Contains(p.Id)).Select(p => p.Code).ToList();

        var dto = new UserDetailDto(
            user.Id.Value,
            user.UserName,
            user.IsAbsolutePermission,
            user.IsActive,
            user.LastLoginAtUtc,
            user.DiscountLimitPercent,
            user.BlockPrintOnRemainingBalance,
            user.WorkStartTime,
            user.WorkEndTime,
            user.HasBreakPeriod,
            user.BreakDurationMinutes,
            codes);

        return Task.FromResult(Result<UserDetailDto>.Success(dto));
    }
}
