using TopLab.Application.Common.Results;
using TopLab.Application.Features.SystemAndPrintSettings.Commands.BackupDatabaseNow;
using TopLab.Application.Tests.Common.Fakes;

namespace TopLab.Application.Tests.Features.SystemAndPrintSettings;

public sealed class BackupDatabaseNowCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidDestination_ReturnsTimestampedBakPath()
    {
        var maintenance = new FakeDatabaseMaintenanceService();
        var handler = new BackupDatabaseNowCommandHandler(maintenance);

        var result = await handler.Handle(new BackupDatabaseNowCommand(@"C:\Backups"), CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error?.Message);
        var fileName = Path.GetFileName(result.Value!);
        Assert.Matches(@"^TopLab_\d{8}_\d{6}\.bak$", fileName);
        Assert.Equal(@"C:\Backups", maintenance.LastBackupDirectory);
    }

    [Fact]
    public async Task Handle_MaintenanceFails_ReturnsFailure()
    {
        var maintenance = new FakeDatabaseMaintenanceService { FailBackup = true };
        var handler = new BackupDatabaseNowCommandHandler(maintenance);

        var result = await handler.Handle(new BackupDatabaseNowCommand(@"C:\Backups"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Unexpected, result.Error?.Type);
    }
}