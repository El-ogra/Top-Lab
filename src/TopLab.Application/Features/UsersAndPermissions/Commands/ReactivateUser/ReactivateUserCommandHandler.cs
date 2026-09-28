using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Users;

namespace TopLab.Application.Features.UsersAndPermissions.Commands.ReactivateUser;

public sealed class ReactivateUserCommandHandler : IRequestHandler<ReactivateUserCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public ReactivateUserCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(ReactivateUserCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated)
        {
            return Result.Failure(Error.Forbidden("أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام"));
        }
        var user = _db.Set<User>().FirstOrDefault(u => u.Id.Value == request.UserId);
        if (user is null)
        {
            return Result.Failure(Error.NotFound("المستخدم غير موجود"));
        }

        user.Reactivate();
        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
