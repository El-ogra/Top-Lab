using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Users;

namespace TopLab.Application.Features.UsersAndPermissions.Commands.ChangeOwnPassword;

public sealed class ChangeOwnPasswordCommandHandler : IRequestHandler<ChangeOwnPasswordCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly ICurrentUserService _currentUser;

    public ChangeOwnPasswordCommandHandler(
        IApplicationDbContext db,
        IPasswordHasher hasher,
        ICurrentUserService currentUser)
    {
        _db = db;
        _hasher = hasher;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(ChangeOwnPasswordCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated)
        {
            return Result.Failure(Error.NotFound("المستخدم غير موجود"));
        }

        var user = _db.Set<User>().FirstOrDefault(u => u.Id.Value == _currentUser.UserId);
        if (user is null)
        {
            return Result.Failure(Error.NotFound("المستخدم غير موجود"));
        }

        if (!_hasher.Verify(request.CurrentPassword, user.PasswordHash))
        {
            return Result.Failure(Error.Validation("كلمة المرور الحالية غير صحيحة"));
        }

        user.ChangePasswordHash(_hasher.Hash(request.NewPassword));
        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
