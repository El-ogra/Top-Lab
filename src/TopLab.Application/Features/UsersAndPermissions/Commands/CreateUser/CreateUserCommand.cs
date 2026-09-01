using MediatR;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.UsersAndPermissions.Commands.CreateUser;

public sealed record CreateUserCommand(
    string UserName,
    string Password,
    string SecondaryPassword,
    bool IsAbsolutePermission,
    decimal DiscountLimitPercent,
    bool BlockPrintOnRemainingBalance,
    TimeOnly? WorkStartTime,
    TimeOnly? WorkEndTime,
    bool HasBreakPeriod,
    int? BreakDurationMinutes,
    IReadOnlyList<string> PermissionCodes) : IRequest<Result<int>>;
