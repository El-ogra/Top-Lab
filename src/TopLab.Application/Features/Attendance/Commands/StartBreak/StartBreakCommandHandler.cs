using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.Attendance.Common;
using TopLab.Domain.Attendance;

namespace TopLab.Application.Features.Attendance.Commands.StartBreak;

public sealed class StartBreakCommandHandler : IRequestHandler<StartBreakCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _dateTime;

    public StartBreakCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider dateTime)
    {
        _db = db;
        _currentUser = currentUser;
        _dateTime = dateTime;
    }

    public async Task<Result> Handle(StartBreakCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated)
        {
            return Result.Failure(Error.Forbidden("أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام"));
        }

        var record = _db.Set<AttendanceRecord>()
            .FirstOrDefault(r => r.UserId.Value == _currentUser.UserId && r.CheckOutAtUtc == null);
        if (record is null)
        {
            return Result.Failure(Error.NotFound("لا يوجد تسجيل حضور مفتوح."));
        }

        try
        {
            record.StartBreak(_dateTime.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Conflict(DomainFailureTranslator.Translate(ex)));
        }

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
