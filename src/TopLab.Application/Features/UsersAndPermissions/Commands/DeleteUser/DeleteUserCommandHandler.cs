using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Accounting;
using TopLab.Domain.Attendance;
using TopLab.Domain.Billing;
using TopLab.Domain.ExternalEntities;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.SentOutSamples;
using TopLab.Domain.Tests;
using TopLab.Domain.Users;

namespace TopLab.Application.Features.UsersAndPermissions.Commands.DeleteUser;

public sealed class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public DeleteUserCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = _db.Set<User>().FirstOrDefault(u => u.Id.Value == request.UserId);
        if (user is null)
        {
            return Result.Failure(Error.NotFound("المستخدم غير موجود"));
        }

        if (user.IsAbsolutePermission && user.IsActive)
        {
            int otherAbsoluteCount = _db.Set<User>().Count(u => u.IsAbsolutePermission && u.IsActive && u.Id.Value != request.UserId);
            if (otherAbsoluteCount == 0)
            {
                return Result.Failure(Error.Conflict("لا يمكن تعطيل آخر مدير نظام؛ يجب إنشاء بديل أولاً"));
            }
        }

        if (HasReferences(request.UserId))
        {
            return Result.Failure(Error.Conflict("لا يمكن حذف مستخدم له سجلات مرتبطة؛ استخدم التعطيل بدلاً من الحذف"));
        }

        var grants = _db.Set<UserPermissionGrant>().Where(g => g.UserId.Equals(user.Id)).ToList();
        foreach (var g in grants)
        {
            _db.Remove(g);
        }

        _db.Remove(user);
        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private bool HasReferences(int userId)
    {
        if (_db.Set<User>().Any(u => u.CreatedByUserId == userId || u.LastModifiedByUserId == userId))
        {
            return true;
        }

        if (SafeAny<Patient>(p => p.CreatedByUserId == userId || p.LastModifiedByUserId == userId))
        {
            return true;
        }

        if (SafeAny<Test>(t => t.CreatedByUserId == userId || t.LastModifiedByUserId == userId))
        {
            return true;
        }

        if (SafeAny<PatientTest>(pt => pt.CreatedByUserId == userId || pt.LastModifiedByUserId == userId
            || pt.EnteredByUserId == userId || pt.ReviewedByUserId == userId
            || pt.LastPrintedByUserId == userId || pt.DeliveredByUserId == userId))
        {
            return true;
        }

        if (SafeAny<PaymentOperation>(po => po.CreatedByUserId == userId || po.LastModifiedByUserId == userId || po.ReceivedByUserId == userId))
        {
            return true;
        }

        if (SafeAny<CashMovement>(cm => cm.CreatedByUserId == userId || cm.LastModifiedByUserId == userId || cm.PerformedByUserId == userId))
        {
            return true;
        }

        if (SafeAny<ExternalEntity>(e => e.CreatedByUserId == userId || e.LastModifiedByUserId == userId))
        {
            return true;
        }

        if (SafeAny<SentOutSample>(s => s.CreatedByUserId == userId || s.LastModifiedByUserId == userId))
        {
            return true;
        }

        if (SafeAny<AttendanceRecord>(a => a.UserId.Value == userId))
        {
            return true;
        }

        return false;
    }

    private bool SafeAny<T>(Func<T, bool> predicate) where T : class
    {
        try
        {
            return _db.Set<T>().Any(predicate);
        }
        catch
        {
            return false;
        }
    }
}
