using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Users;

namespace TopLab.Application.Features.UsersAndPermissions.Commands.UpdateUser;

public sealed class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly ICurrentUserService _currentUser;

    public UpdateUserCommandHandler(IApplicationDbContext db, IPasswordHasher hasher, ICurrentUserService currentUser)
    {
        _db = db;
        _hasher = hasher;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
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

        if (request.IsAbsolutePermission && !_currentUser.IsAbsolutePermission)
        {
            return Result.Failure(Error.Forbidden("أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام"));
        }

        if (user.IsAbsolutePermission && !_currentUser.IsAbsolutePermission)
        {
            return Result.Failure(Error.Forbidden("أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام"));
        }

        if (_db.Set<User>().Any(u => u.UserName == request.UserName && u.Id.Value != request.UserId))
        {
            return Result.Failure(Error.Conflict("اسم المستخدم موجود بالفعل"));
        }

        bool willBeAbsolute = request.IsAbsolutePermission;
        bool wasAbsolute = user.IsAbsolutePermission;
        bool wasActive = user.IsActive;

        if (wasAbsolute && wasActive && !willBeAbsolute)
        {
            int otherAbsoluteCount = _db.Set<User>().Count(u => u.IsAbsolutePermission && u.IsActive && u.Id.Value != request.UserId);
            if (otherAbsoluteCount == 0)
            {
                return Result.Failure(Error.Conflict("لا يمكن تعطيل آخر مدير نظام؛ يجب إنشاء بديل أولاً"));
            }
        }

        user.ChangeUserName(request.UserName);
        user.SetAbsolutePermission(request.IsAbsolutePermission);
        user.SetPolicy(request.DiscountLimitPercent, request.BlockPrintOnRemainingBalance);
        user.SetWorkingHours(request.WorkStartTime, request.WorkEndTime);
        user.SetBreakPeriod(request.HasBreakPeriod, request.BreakDurationMinutes);

        if (!string.IsNullOrEmpty(request.Password))
        {
            var hash = _hasher.Hash(request.Password);
            user.ChangePasswordHash(hash);
        }

        if (!string.IsNullOrEmpty(request.SecondaryPassword))
        {
            var hash = _hasher.Hash(request.SecondaryPassword);
            user.ChangeInternalWindowsPasswordHash(hash);
        }

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex) when (IsUniqueViolation(ex))
        {
            return Result.Failure(Error.Conflict("اسم المستخدم موجود بالفعل"));
        }

        return Result.Success();
    }

    private static bool IsUniqueViolation(Exception ex)
    {
        var msg = ex.Message;
        return msg.Contains("IX_Users_UserName") || msg.Contains("duplicate") || msg.Contains("UNIQUE");
    }
}
