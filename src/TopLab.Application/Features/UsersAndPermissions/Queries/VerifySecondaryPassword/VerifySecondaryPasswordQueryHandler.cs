using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Users;

namespace TopLab.Application.Features.UsersAndPermissions.Queries.VerifySecondaryPassword;

public sealed class VerifySecondaryPasswordQueryHandler : IRequestHandler<VerifySecondaryPasswordQuery, Result<bool>>
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly ICurrentUserService _currentUser;

    public VerifySecondaryPasswordQueryHandler(
        IApplicationDbContext db,
        IPasswordHasher hasher,
        ICurrentUserService currentUser)
    {
        _db = db;
        _hasher = hasher;
        _currentUser = currentUser;
    }

    public Task<Result<bool>> Handle(VerifySecondaryPasswordQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated)
        {
            return Task.FromResult(Result<bool>.Failure(Error.Forbidden("غير مصرح")));
        }

        var user = _db.Set<User>().FirstOrDefault(u => u.Id.Value == _currentUser.UserId);

        if (user is null)
        {
            return Task.FromResult(Result<bool>.Failure(Error.Forbidden("غير مصرح")));
        }

        bool verified = _hasher.Verify(request.Password, user.InternalWindowsPasswordHash);
        return Task.FromResult(Result<bool>.Success(verified));
    }
}
