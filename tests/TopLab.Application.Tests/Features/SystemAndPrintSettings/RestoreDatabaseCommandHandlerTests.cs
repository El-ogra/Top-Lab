using TopLab.Application.Common.Results;
using TopLab.Application.Features.SystemAndPrintSettings.Commands.RestoreDatabase;
using TopLab.Application.Tests.Common.Fakes;

namespace TopLab.Application.Tests.Features.SystemAndPrintSettings;

public sealed class RestoreDatabaseCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidBackupFile_ReturnsSuccess()
    {
        var maintenance = new FakeDatabaseMaintenanceService();
        var handler = new RestoreDatabaseCommandHandler(maintenance);

        var result = await handler.Handle(new RestoreDatabaseCommand(@"C:\Backups\TopLab_20260902.bak"), CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error?.Message);
        Assert.Equal(@"C:\Backups\TopLab_20260902.bak", maintenance.LastRestoreFile);
    }

    [Fact]
    public async Task Handle_MaintenanceFails_ReturnsFailure()
    {
        var maintenance = new FakeDatabaseMaintenanceService { FailRestore = true };
        var handler = new RestoreDatabaseCommandHandler(maintenance);

        var result = await handler.Handle(new RestoreDatabaseCommand(@"C:\Backups\TopLab_20260902.bak"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Unexpected, result.Error?.Type);
    }
}