using MediatR;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.Attendance.Commands.StartBreak;

public sealed record StartBreakCommand() : IRequest<Result>;
