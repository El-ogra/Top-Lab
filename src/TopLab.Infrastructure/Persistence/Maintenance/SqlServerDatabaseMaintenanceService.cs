using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Infrastructure.Persistence;

namespace TopLab.Infrastructure.Persistence.Maintenance;

/// <summary>
/// Physical SQL Server backup/restore and migration maintenance (settled
/// boundary §2.3-6, §2.3-8, §2.3-13). Backup/restore disk paths are interpreted
/// on the SQL Server machine — this is surfaced honestly to callers via the
/// helper text in the maintenance window and the handoff document.
/// </summary>
public sealed class SqlServerDatabaseMaintenanceService : IDatabaseMaintenanceService
{
    private const string DatabaseFileNamePrefix = "TopLab";
    private readonly IWorkstationConnectionSettingsProvider _connectionSettings;
    private readonly ApplicationDbContext _dbContext;

    public SqlServerDatabaseMaintenanceService(
        IWorkstationConnectionSettingsProvider connectionSettings,
        ApplicationDbContext dbContext)
    {
        _connectionSettings = connectionSettings;
        _dbContext = dbContext;
    }

    public async Task<Result<string>> BackupNowAsync(string destinationDirectory, CancellationToken cancellationToken = default)
    {
        try
        {
            var cs = ValidConnectionString();
            var databaseName = DatabaseNameOf(cs);
            var fileName = $"{DatabaseFileNamePrefix}_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
            var fullPath = Path.Combine(destinationDirectory, fileName);
            var command = $"BACKUP DATABASE [{databaseName}] TO DISK = '{fullPath.Replace("'", "''")}' WITH FORMAT";

            await ExecuteNonQueryAsync(cs, command, cancellationToken);

            return Result<string>.Success(fullPath);
        }
        catch (Exception ex) when (ex is SqlException or InvalidOperationException)
        {
            return Result<string>.Failure(Error.Unexpected(FriendlyMaintenanceMessage(ex)));
        }
    }

    public async Task<Result> RestoreAsync(string backupFilePath, CancellationToken cancellationToken = default)
    {
        try
        {
            var cs = ValidConnectionString();
            var databaseName = DatabaseNameOf(cs);
            var sourcePath = backupFilePath.Replace("'", "''");

            var safetyFileName = $"{DatabaseFileNamePrefix}_Safety_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
            var safetyPath = Path.Combine(Path.GetDirectoryName(backupFilePath) ?? string.Empty, safetyFileName);

            // Master connection so the database can be switched out from under us.
            var masterCs = new SqlConnectionStringBuilder(cs) { InitialCatalog = "master" };

            var preRestore = $"BACKUP DATABASE [{databaseName}] TO DISK = '{safetyPath.Replace("'", "''")}' WITH FORMAT";
            await ExecuteNonQueryRawAsync(masterCs.ConnectionString, preRestore, cancellationToken);

            var singleUser = $"ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE";
            await ExecuteNonQueryRawAsync(masterCs.ConnectionString, singleUser, cancellationToken);

            try
            {
                var restore = $"RESTORE DATABASE [{databaseName}] FROM DISK = '{sourcePath}' WITH REPLACE";
                await ExecuteNonQueryRawAsync(masterCs.ConnectionString, restore, cancellationToken);
            }
            finally
            {
                var multiUser = $"ALTER DATABASE [{databaseName}] SET MULTI_USER";
                await ExecuteNonQueryRawAsync(masterCs.ConnectionString, multiUser, cancellationToken);
            }

            return Result.Success();
        }
        catch (Exception ex) when (ex is SqlException or InvalidOperationException)
        {
            return Result.Failure(Error.Unexpected(FriendlyMaintenanceMessage(ex)));
        }
    }

    public async Task<Result<int>> ApplyPendingUpdatesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var pendingCount = _dbContext.Database.GetPendingMigrations().Count();
            await _dbContext.Database.MigrateAsync(cancellationToken);
            return Result<int>.Success(pendingCount);
        }
        catch (Exception ex) when (ex is SqlException or InvalidOperationException)
        {
            return Result<int>.Failure(Error.Unexpected(FriendlyMaintenanceMessage(ex)));
        }
    }

    public async Task<Result> CheckBackupPathAsync(string path, CancellationToken cancellationToken = default)
    {
        try
        {
            var cs = ValidConnectionString();
            var databaseName = DatabaseNameOf(cs);
            var probe = Path.Combine(path, $"{DatabaseFileNamePrefix}_check_probe_{Guid.NewGuid():N}.tmp");
            await ExecuteNonQueryAsync(
                cs,
                $"BACKUP DATABASE [{databaseName}] TO DISK = '{probe.Replace("'", "''")}' WITH FORMAT, INIT",
                cancellationToken);
            return Result.Success();
        }
        catch (Exception ex) when (ex is SqlException or InvalidOperationException)
        {
            return Result.Failure(Error.Unexpected(FriendlyMaintenanceMessage(ex)));
        }
    }

    private string ValidConnectionString()
    {
        var cs = _connectionSettings.GetEffectiveConnectionString();
        if (string.IsNullOrWhiteSpace(cs))
        {
            throw new InvalidOperationException("No SQL Server connection string is configured.");
        }

        return cs;
    }

    private static string DatabaseNameOf(string connectionString)
    {
        var builder = new SqlConnectionStringBuilder(connectionString);
        if (string.IsNullOrWhiteSpace(builder.InitialCatalog))
        {
            throw new InvalidOperationException("The configured connection string has no database name.");
        }

        return builder.InitialCatalog;
    }

    private static async Task ExecuteNonQueryAsync(string connectionString, string commandText, CancellationToken ct)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = commandText;
        await command.ExecuteNonQueryAsync(ct);
    }

    private static async Task ExecuteNonQueryRawAsync(string connectionString, string commandText, CancellationToken ct)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = commandText;
        await command.ExecuteNonQueryAsync(ct);
    }

    private static string FriendlyMaintenanceMessage(Exception ex)
        => ex is SqlException sql
            ? $"لا يمكن إجراء عملية الصيانة على قاعدة البيانات. تفاصيل: {sql.Message}"
            : $"لا يمكن إجراء عملية الصيانة على قاعدة البيانات. {ex.Message}";
}