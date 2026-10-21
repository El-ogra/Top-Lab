using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.SystemAndPrintSettings.Commands.UpdatePrintResultPassword;

/// <summary>
/// STAGE-01 (baseline 1.2.2.4.2 / 1.2.2.4.2.1): sets the print-result
/// password requirement. The plain password never reaches the database — the
/// handler stores only its hash.
/// </summary>
public sealed record UpdatePrintResultPasswordCommand(
    bool Enabled,
    string? Password)
    : IRequest<Result>, IAuthorizedRequest
{
    public string RequiredPermissionCode => "EDIT_SYSTEM_SETTINGS";
}
