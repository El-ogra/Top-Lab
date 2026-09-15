using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.Attendance.Common;

namespace TopLab.Application.Features.Attendance.Queries.GetAttendanceRecords;

public sealed record GetAttendanceRecordsQuery(
    int? UserId,
    DateOnly? From,
    DateOnly? To,
    int Page,
    int PageSize) : IRequest<Result<IReadOnlyList<AttendanceRecordDto>>>;
