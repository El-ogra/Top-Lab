using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.SystemAndPrintSettings.Commands.BackupDatabaseNow;

public sealed record BackupDatabaseNowCommand(string DestinationDirectory)
    : IRequest<Result<string>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => "EDIT_SYSTEM_SETTINGS";
}