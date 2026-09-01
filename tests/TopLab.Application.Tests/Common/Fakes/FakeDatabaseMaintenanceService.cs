using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Tests.Common.Fakes;

public sealed class FakeDatabaseMaintenanceService : IDatabaseMaintenanceService
{
    public int MigrationsApplied { get; set; }
    public string? LastBackupDirectory { get; private set; }
    public string? LastRestoreFile { get; private set; }
    public string? LastCheckPath { get; private set; }
    public bool FailBackup { get; set; }
    public bool FailRestore { get; set; }
    public bool FailApply { get; set; }
    public bool FailCheck { get; set; }

    public Task<Result<string>> BackupNowAsync(string destinationDirectory, CancellationToken cancellationToken = default)
    {
        LastBackupDirectory = destinationDirectory;
        if (FailBackup)
        {
            return Task.FromResult(Result<string>.Failure(Error.Unexpected("تعذر إجراء النسخ الاحتياطي.")));
        }

        var fileName = $"TopLab_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
        return Task.FromResult(Result<string>.Success(Path.Combine(destinationDirectory, fileName)));
    }

    public Task<Result> RestoreAsync(string backupFilePath, CancellationToken cancellationToken = default)
    {
        LastRestoreFile = backupFilePath;
        return Task.FromResult(FailRestore
            ? Result.Failure(Error.Unexpected("تعذر استعادة قاعدة البيانات."))
            : Result.Success());
    }

    public Task<Result<int>> ApplyPendingUpdatesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(FailApply
            ? Result<int>.Failure(Error.Unexpected("تعذر تطبيق تحديثات قاعدة البيانات."))
            : Result<int>.Success(MigrationsApplied));
    }

    public Task<Result> CheckBackupPathAsync(string path, CancellationToken cancellationToken = default)
    {
        LastCheckPath = path;
        return Task.FromResult(FailCheck
            ? Result.Failure(Error.Unexpected("مسار النسخ الاحتياطي غير صالح."))
            : Result.Success());
    }
}