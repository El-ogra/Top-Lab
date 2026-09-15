using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Attendance;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Users;

namespace TopLab.Application.Features.Attendance.Commands.CheckIn;

public sealed class CheckInCommandHandler : IRequestHandler<CheckInCommand, Result<int>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _dateTime;

    public CheckInCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider dateTime)
    {
        _db = db;
        _currentUser = currentUser;
        _dateTime = dateTime;
    }

    public async Task<Result<int>> Handle(CheckInCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated)
        {
            return Result<int>.Failure(Error.Forbidden("أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام"));
        }

        var userId = _currentUser.UserId;

        if (_db.Set<AttendanceRecord>().Any(r => r.UserId.Value == userId && r.CheckOutAtUtc == null))
        {
            return Result<int>.Failure(Error.Conflict("يوجد تسجيل حضور مفتوح لهذا المستخدم."));
        }

        var user = _db.Set<User>().FirstOrDefault(u => u.Id.Value == userId);
        var now = _dateTime.UtcNow;

        var record = AttendanceRecord.Create(
            AttendanceRecordId.Create(0),
            UserId.Create(userId),
            now,
            AttendanceCalculator.LatenessMinutes(user?.WorkStartTime, now));

        _db.Add(record);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(record.Id.Value);
    }
}
