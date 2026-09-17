using MediatR;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.UsersAndPermissions.Commands.ChangeOwnPassword;

/// <summary>
/// Self-change-password command (S-02 Slice 1, D7 — the single authorised new
/// backend artifact of workstream S-02). Any authenticated user may execute it;
/// like the Deactivate/Reactivate commands it does not implement
/// <c>IAuthorizedRequest</c>, so no permission gate applies.
/// </summary>
public sealed record ChangeOwnPasswordCommand(
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword) : IRequest<Result>;
