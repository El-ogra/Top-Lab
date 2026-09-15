using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.Attendance.Common;
using TopLab.Domain.Attendance;
using TopLab.Domain.Users;

namespace TopLab.Application.Features.Attendance.Commands.CheckOut;

public sealed class CheckOutCommandHandler : IRequestHandler<CheckOutCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _dateTime;

    public CheckOutCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider dateTime)
    {
        _db = db;
        _currentUser = currentUser;
        _dateTime = dateTime;
    }

    public async Task<Result> Handle(CheckOutCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated)
        {
            return Result.Failure(Error.Forbidden("أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام"));
        }

        var userId = _currentUser.UserId;

        var record = _db.Set<AttendanceRecord>()
            .FirstOrDefault(r => r.UserId.Value == userId && r.CheckOutAtUtc == null);
        if (record is null)
        {
            return Result.Failure(Error.NotFound("لا يوجد تسجيل حضور مفتوح."));
        }

        var user = _db.Set<User>().FirstOrDefault(u => u.Id.Value == userId);
        var now = _dateTime.UtcNow;

        try
        {
            record.CheckOut(now, AttendanceCalculator.OvertimeMinutes(user?.WorkEndTime, now));
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Conflict(DomainFailureTranslator.Translate(ex)));
        }

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
