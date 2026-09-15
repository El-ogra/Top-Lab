using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.Attendance.Common;
using TopLab.Domain.Attendance;
using TopLab.Domain.Users;

namespace TopLab.Application.Features.Attendance.Queries.GetUserAttendanceSummary;

public sealed class GetUserAttendanceSummaryQueryHandler
    : IRequestHandler<GetUserAttendanceSummaryQuery, Result<UserAttendanceSummaryDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _dateTime;

    public GetUserAttendanceSummaryQueryHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider dateTime)
    {
        _db = db;
        _currentUser = currentUser;
        _dateTime = dateTime;
    }

    public Task<Result<UserAttendanceSummaryDto>> Handle(
        GetUserAttendanceSummaryQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAbsolutePermission)
        {
            return Task.FromResult(Result<UserAttendanceSummaryDto>.Failure(
                Error.Forbidden("أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام")));
        }

        var user = _db.Set<User>().FirstOrDefault(u => u.Id.Value == request.UserId);
        if (user is null)
        {
            return Task.FromResult(Result<UserAttendanceSummaryDto>.Failure(
                Error.NotFound("المستخدم غير موجود.")));
        }

        var today = DateOnly.FromDateTime(_dateTime.UtcNow);
        var from = request.From ?? today;
        var to = request.To ?? today;
        var fromStart = from.ToDateTime(TimeOnly.MinValue);
        var toEndExclusive = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var records = _db.Set<AttendanceRecord>()
            .Where(r => r.UserId.Value == request.UserId
                && r.CheckInAtUtc >= fromStart && r.CheckInAtUtc < toEndExclusive)
            .ToList();

        var daysPresent = records
            .Select(r => DateOnly.FromDateTime(r.CheckInAtUtc))
            .Distinct()
            .Count();

        var totalWorked = records.Sum(r => r.CheckOutAtUtc is null
            ? 0
            : AttendanceCalculator.WorkedMinutes(
                r.CheckInAtUtc,
                r.CheckOutAtUtc.Value,
                r.BreakStartAtUtc,
                r.BreakEndAtUtc));
        var totalLateness = records.Sum(r => r.LatenessMinutes ?? 0);
        var totalOvertime = records.Sum(r => r.OvertimeMinutes ?? 0);

        return Task.FromResult(Result<UserAttendanceSummaryDto>.Success(new UserAttendanceSummaryDto(
            user.Id.Value,
            user.UserName,
            from,
            to,
            daysPresent,
            records.Count,
            totalWorked,
            totalLateness,
            totalOvertime)));
    }
}
