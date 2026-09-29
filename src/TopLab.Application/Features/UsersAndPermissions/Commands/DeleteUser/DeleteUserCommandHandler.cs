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
    private readonly ICurrentUserService _currentUser;

    public DeleteUserCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
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

        if (user.IsAbsolutePermission && !_currentUser.IsAbsolutePermission)
        {
            return Result.Failure(Error.Forbidden("أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام"));
        }

        if (user.IsAbsolutePermission && user.IsActive)
        {
            int otherAbsoluteCount = _db.Set<User>().Count(u => u.IsAbsolutePermission && u.IsActive && u.Id.Value != request.UserId);
            if (otherAbsoluteCount == 0)
            {
                return Result.Failure(Error.Conflict("لا يمكن تعطيل آخر مدير نظام؛ يجب إنشاء بديل أولاً"));
            }
        }

        if (HasReferences(request.UserId, out var referenceError))
        {
            if (referenceError is not null)
            {
                return Result.Failure(referenceError);
            }
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

    private bool HasReferences(int userId, out Error? failure)
    {
        failure = null;
        try
        {
            if (_db.Set<User>().Any(u => u.CreatedByUserId == userId || u.LastModifiedByUserId == userId))
            {
                return true;
            }

            if (_db.Set<Patient>().Any(p => p.CreatedByUserId == userId || p.LastModifiedByUserId == userId))
            {
                return true;
            }

            if (_db.Set<Test>().Any(t => t.CreatedByUserId == userId || t.LastModifiedByUserId == userId))
            {
                return true;
            }

            if (_db.Set<PatientTest>().Any(pt => pt.CreatedByUserId == userId || pt.LastModifiedByUserId == userId
                || pt.EnteredByUserId == userId || pt.ReviewedByUserId == userId
                || pt.LastPrintedByUserId == userId || pt.DeliveredByUserId == userId))
            {
                return true;
            }

            if (_db.Set<PaymentOperation>().Any(po => po.CreatedByUserId == userId || po.LastModifiedByUserId == userId || po.ReceivedByUserId == userId))
            {
                return true;
            }

            if (_db.Set<CashMovement>().Any(cm => cm.CreatedByUserId == userId || cm.LastModifiedByUserId == userId || cm.PerformedByUserId == userId))
            {
                return true;
            }

            if (_db.Set<ExternalEntity>().Any(e => e.CreatedByUserId == userId || e.LastModifiedByUserId == userId))
            {
                return true;
            }

            if (_db.Set<SentOutSample>().Any(s => s.CreatedByUserId == userId || s.LastModifiedByUserId == userId))
            {
                return true;
            }

            if (_db.Set<AttendanceRecord>().Any(a => a.UserId.Value == userId))
            {
                return true;
            }

            return false;
        }
        catch (Exception)
        {
            failure = Error.Unexpected("تعذر التحقق من السجلات المرتبطة. لا يمكن حذف المستخدم.");
            return true;
        }
    }
}
