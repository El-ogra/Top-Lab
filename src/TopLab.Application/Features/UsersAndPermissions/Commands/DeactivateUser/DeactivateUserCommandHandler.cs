using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Users;

namespace TopLab.Application.Features.UsersAndPermissions.Commands.DeactivateUser;

public sealed class DeactivateUserCommandHandler : IRequestHandler<DeactivateUserCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public DeactivateUserCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = _db.Set<User>().FirstOrDefault(u => u.Id.Value == request.UserId);
        if (user is null)
        {
            return Result.Failure(Error.NotFound("المستخدم غير موجود"));
        }

        if (!user.IsActive)
        {
            return Result.Success();
        }

        if (user.IsAbsolutePermission)
        {
            int otherAbsoluteCount = _db.Set<User>().Count(u => u.IsAbsolutePermission && u.IsActive && u.Id.Value != request.UserId);
            if (otherAbsoluteCount == 0)
            {
                return Result.Failure(Error.Conflict("لا يمكن تعطيل آخر مدير نظام؛ يجب إنشاء بديل أولاً"));
            }
        }

        user.Deactivate();
        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
