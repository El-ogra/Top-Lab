using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.Attendance.Common;
using TopLab.Domain.Attendance;
using TopLab.Domain.Users;

namespace TopLab.Application.Features.Attendance.Queries.GetAttendanceRecords;

public sealed class GetAttendanceRecordsQueryHandler
    : IRequestHandler<GetAttendanceRecordsQuery, Result<IReadOnlyList<AttendanceRecordDto>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _dateTime;

    public GetAttendanceRecordsQueryHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider dateTime)
    {
        _db = db;
        _currentUser = currentUser;
        _dateTime = dateTime;
    }

    public Task<Result<IReadOnlyList<AttendanceRecordDto>>> Handle(
        GetAttendanceRecordsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAbsolutePermission)
        {
            return Task.FromResult(Result<IReadOnlyList<AttendanceRecordDto>>.Failure(
                Error.Forbidden("أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام")));
        }

        var today = DateOnly.FromDateTime(_dateTime.UtcNow);
        var from = request.From ?? today;
        var to = request.To ?? today;
        var fromStart = from.ToDateTime(TimeOnly.MinValue);
        var toEndExclusive = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var query = _db.Set<AttendanceRecord>().AsQueryable();
        if (request.UserId.HasValue)
        {
            query = query.Where(r => r.UserId.Value == request.UserId.Value);
        }

        var rows = query
            .Where(r => r.CheckInAtUtc >= fromStart && r.CheckInAtUtc < toEndExclusive)
            .OrderBy(r => r.CheckInAtUtc)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var userIds = rows.Select(r => r.UserId.Value).Distinct().ToList();
        var users = _db.Set<User>()
            .Where(u => userIds.Contains(u.Id.Value))
            .ToDictionary(u => u.Id.Value);

        IReadOnlyList<AttendanceRecordDto> items = rows
            .Select(r =>
            {
                var userName = users.TryGetValue(r.UserId.Value, out var user)
                    ? user.UserName
                    : string.Empty;
                return AttendanceRecordDto.FromRecord(r, userName);
            })
            .ToList();

        return Task.FromResult(Result<IReadOnlyList<AttendanceRecordDto>>.Success(items));
    }
}
