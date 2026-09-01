using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.SystemAndPrintSettings.Commands.BackupDatabaseNow;

public sealed class BackupDatabaseNowCommandHandler : IRequestHandler<BackupDatabaseNowCommand, Result<string>>
{
    private readonly IDatabaseMaintenanceService _maintenance;

    public BackupDatabaseNowCommandHandler(IDatabaseMaintenanceService maintenance)
    {
        _maintenance = maintenance;
    }

    public Task<Result<string>> Handle(BackupDatabaseNowCommand request, CancellationToken cancellationToken)
        => _maintenance.BackupNowAsync(request.DestinationDirectory, cancellationToken);
}