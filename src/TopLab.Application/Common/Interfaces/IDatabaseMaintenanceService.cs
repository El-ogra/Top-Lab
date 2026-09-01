using TopLab.Application.Common.Results;

namespace TopLab.Application.Common.Interfaces;

/// <summary>
/// Port for physical SQL Server backup/restore and migration maintenance
/// (settled boundary §2.3-6). Implemented by the SQL-backed maintenance service.
/// Backup/restore paths are interpreted on the SQL Server machine.
/// </summary>
public interface IDatabaseMaintenanceService
{
    Task<Result<string>> BackupNowAsync(string destinationDirectory, CancellationToken cancellationToken = default);

    Task<Result> RestoreAsync(string backupFilePath, CancellationToken cancellationToken = default);

    Task<Result<int>> ApplyPendingUpdatesAsync(CancellationToken cancellationToken = default);

    Task<Result> CheckBackupPathAsync(string path, CancellationToken cancellationToken = default);
}