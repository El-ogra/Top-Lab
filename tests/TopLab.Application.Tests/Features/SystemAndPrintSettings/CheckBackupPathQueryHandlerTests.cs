using TopLab.Application.Common.Results;
using TopLab.Application.Features.SystemAndPrintSettings.Queries.CheckBackupPath;
using TopLab.Application.Tests.Common.Fakes;

namespace TopLab.Application.Tests.Features.SystemAndPrintSettings;

public sealed class CheckBackupPathQueryHandlerTests
{
    [Fact]
    public async Task Handle_ValidPath_ReturnsSuccess()
    {
        var maintenance = new FakeDatabaseMaintenanceService();
        var handler = new CheckBackupPathQueryHandler(maintenance);

        var result = await handler.Handle(new CheckBackupPathQuery(@"C:\Backups"), CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error?.Message);
        Assert.Equal(@"C:\Backups", maintenance.LastCheckPath);
    }

    [Fact]
    public async Task Handle_InvalidPath_ReturnsFailure()
    {
        var maintenance = new FakeDatabaseMaintenanceService { FailCheck = true };
        var handler = new CheckBackupPathQueryHandler(maintenance);

        var result = await handler.Handle(new CheckBackupPathQuery(@"Z:\NoSuch"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Unexpected, result.Error?.Type);
    }
}