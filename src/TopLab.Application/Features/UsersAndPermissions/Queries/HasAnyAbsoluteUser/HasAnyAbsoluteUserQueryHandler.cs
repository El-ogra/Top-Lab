using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Users;

namespace TopLab.Application.Features.UsersAndPermissions.Queries.HasAnyAbsoluteUser;

public sealed class HasAnyAbsoluteUserQueryHandler : IRequestHandler<HasAnyAbsoluteUserQuery, Result<bool>>
{
    private readonly IApplicationDbContext _db;

    public HasAnyAbsoluteUserQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<bool>> Handle(HasAnyAbsoluteUserQuery request, CancellationToken cancellationToken)
    {
        bool has = _db.Set<User>().Any(u => u.IsActive && u.IsAbsolutePermission);
        return Task.FromResult(Result<bool>.Success(has));
    }
}
