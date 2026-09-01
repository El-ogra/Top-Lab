using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.SystemAndPrintSettings.Commands.ApplyDatabaseUpdates;

/// <summary>
/// Outcome of the system-initialization command: how many migrations were applied
/// and how many seed rows were re-inserted (insert-if-missing only).
/// </summary>
public sealed record ApplyDatabaseUpdatesOutcome(int MigrationsApplied, int SeedRowsInserted);

public sealed record ApplyDatabaseUpdatesCommand()
    : IRequest<Result<ApplyDatabaseUpdatesOutcome>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => "EDIT_SYSTEM_SETTINGS";
}