using MediatR;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.Attendance.Commands.EndBreak;

public sealed record EndBreakCommand() : IRequest<Result>;
