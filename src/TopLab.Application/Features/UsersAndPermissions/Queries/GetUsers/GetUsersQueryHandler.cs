using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.UsersAndPermissions.Common;
using TopLab.Domain.Users;

namespace TopLab.Application.Features.UsersAndPermissions.Queries.GetUsers;

public sealed class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, Result<IReadOnlyList<UserSummaryDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetUsersQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<IReadOnlyList<UserSummaryDto>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = _db.Set<User>()
            .Select(u => new UserSummaryDto(
                u.Id.Value,
                u.UserName,
                u.IsAbsolutePermission,
                u.IsActive,
                u.LastLoginAtUtc))
            .ToList();

        return Task.FromResult(Result<IReadOnlyList<UserSummaryDto>>.Success(users));
    }
}
