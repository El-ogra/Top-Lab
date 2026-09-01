using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.SystemAndPrintSettings.Commands.RestoreDatabase;

public sealed class RestoreDatabaseCommandHandler : IRequestHandler<RestoreDatabaseCommand, Result>
{
    private readonly IDatabaseMaintenanceService _maintenance;

    public RestoreDatabaseCommandHandler(IDatabaseMaintenanceService maintenance)
    {
        _maintenance = maintenance;
    }

    public Task<Result> Handle(RestoreDatabaseCommand request, CancellationToken cancellationToken)
        => _maintenance.RestoreAsync(request.BackupFilePath, cancellationToken);
}