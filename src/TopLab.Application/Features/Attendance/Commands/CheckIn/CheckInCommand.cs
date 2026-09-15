using MediatR;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.Attendance.Commands.CheckIn;

public sealed record CheckInCommand() : IRequest<Result<int>>;
