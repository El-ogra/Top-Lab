using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.SystemAndPrintSettings.Commands.UpdateDatabaseServerSettings;

public sealed record UpdateDatabaseServerSettingsCommand(
    string Server,
    string Database,
    bool IntegratedSecurity,
    string Login,
    string Password)
    : IRequest<Result>, IAuthorizedRequest
{
    public string RequiredPermissionCode => "EDIT_SYSTEM_SETTINGS";
}