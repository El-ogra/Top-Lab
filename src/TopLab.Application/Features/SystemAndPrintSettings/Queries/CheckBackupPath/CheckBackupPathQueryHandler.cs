using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.SystemAndPrintSettings.Queries.CheckBackupPath;

public sealed class CheckBackupPathQueryHandler : IRequestHandler<CheckBackupPathQuery, Result>
{
    private readonly IDatabaseMaintenanceService _maintenance;

    public CheckBackupPathQueryHandler(IDatabaseMaintenanceService maintenance)
    {
        _maintenance = maintenance;
    }

    public Task<Result> Handle(CheckBackupPathQuery request, CancellationToken cancellationToken)
        => _maintenance.CheckBackupPathAsync(request.Path, cancellationToken);
}