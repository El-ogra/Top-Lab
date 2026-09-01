using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.SystemAndPrintSettings.Commands.RestoreDatabase;

public sealed record RestoreDatabaseCommand(string BackupFilePath)
    : IRequest<Result>, IAuthorizedRequest
{
    public string RequiredPermissionCode => "EDIT_SYSTEM_SETTINGS";
}