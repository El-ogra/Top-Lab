using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.Attendance.Common;

namespace TopLab.Application.Features.Attendance.Queries.GetUserAttendanceSummary;

public sealed record GetUserAttendanceSummaryQuery(
    int UserId,
    DateOnly? From,
    DateOnly? To) : IRequest<Result<UserAttendanceSummaryDto>>;
