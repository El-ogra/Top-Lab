using MediatR;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.Attendance.Commands.CheckOut;

public sealed record CheckOutCommand() : IRequest<Result>;
