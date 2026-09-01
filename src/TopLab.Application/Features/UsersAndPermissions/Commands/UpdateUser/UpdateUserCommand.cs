using MediatR;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.UsersAndPermissions.Commands.UpdateUser;

public sealed record UpdateUserCommand(
    int UserId,
    string UserName,
    bool IsAbsolutePermission,
    decimal DiscountLimitPercent,
    bool BlockPrintOnRemainingBalance,
    TimeOnly? WorkStartTime,
    TimeOnly? WorkEndTime,
    bool HasBreakPeriod,
    int? BreakDurationMinutes,
    string? Password,
    string? SecondaryPassword) : IRequest<Result>;
