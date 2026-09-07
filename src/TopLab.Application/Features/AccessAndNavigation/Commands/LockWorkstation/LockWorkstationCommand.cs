using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.AccessAndNavigation.Common;

namespace TopLab.Application.Features.AccessAndNavigation.Commands.LockWorkstation;

/// <summary>
/// Clears the in-memory current session without touching the database. Gated on
/// <c>EDIT_SYSTEM_SETTINGS</c> (matching the M-17/M-22 permission pattern) so a
/// workstation-locking UI action follows the same authorization rule as the
/// system-settings writes it will eventually run alongside. Mirrors the M-17
/// <c>SignOutCommand</c> precedent of having no validator because the command
/// has no fields to validate.
/// </summary>
public sealed record LockWorkstationCommand
    : IRequest<Result>, IAuthorizedRequest
{
    public string RequiredPermissionCode => AccessAndNavigationAccessPolicy.EditSystemSettings;
}