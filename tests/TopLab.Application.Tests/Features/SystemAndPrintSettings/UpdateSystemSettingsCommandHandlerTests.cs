using TopLab.Application.Features.SystemAndPrintSettings.Commands.UpdateSystemSettings;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Settings;
using Xunit;

namespace TopLab.Application.Tests.Features.SystemAndPrintSettings;

public class UpdateSystemSettingsCommandHandlerTests
{
    private static UpdateSystemSettingsCommand DefaultCommand() => new(
        DefaultAccountType: AccountType.Contracts,
        SaveTreatingDoctorOnlyFromEntityWindow: true,
        EnablePatientNameSearchAssist: false,
        DisableAutoTitleInsertion: true,
        PrintFileExternalBarcode: false,
        PrintDateTimeOnTubeBarcode: true,
        PrintLabIdInsteadOfPatientId: false,
        AutoReviewAndComplete: true,
        PrintAccountInsteadOfDateOnReport: false,
        ResultScreenAccountDisplayMode: ResultScreenAccountDisplayMode.Summary,
        DailyBackupEnabled: false,
        DailyBackupPath: null);

    [Fact]
    public async Task UpdateSystemSettings_RoundTrips()
    {
        var db = new FakeApplicationDbContext();
        db.SystemSettings.Add(SystemSettings.CreateDefault());

        var handler = new UpdateSystemSettingsCommandHandler(db);
        var result = await handler.Handle(DefaultCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(AccountType.Contracts, db.SystemSettings[0].DefaultAccountType);
        Assert.True(db.SystemSettings[0].SaveTreatingDoctorOnlyFromEntityWindow);
        Assert.True(db.SystemSettings[0].DisableAutoTitleInsertion);
        Assert.True(db.SystemSettings[0].PrintDateTimeOnTubeBarcode);
        Assert.True(db.SystemSettings[0].AutoReviewAndComplete);
        Assert.Equal(ResultScreenAccountDisplayMode.Summary, db.SystemSettings[0].ResultScreenAccountDisplayMode);
    }

    [Fact]
    public async Task UpdateSystemSettings_BackupDisabled_KeepsStoredPath()
    {
        var db = new FakeApplicationDbContext();
        var existing = SystemSettings.CreateDefault();
        existing.SetDailyBackup(true, @"C:\backups");
        db.SystemSettings.Add(existing);

        var cmd = DefaultCommand() with { DailyBackupEnabled = false, DailyBackupPath = null };
        var handler = new UpdateSystemSettingsCommandHandler(db);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(db.SystemSettings[0].DailyBackupEnabled);
        Assert.Equal(@"C:\backups", db.SystemSettings[0].DailyBackupPath);
    }

    [Fact]
    public async Task UpdateSystemSettings_MissingRow_ReturnsUnexpected()
    {
        var db = new FakeApplicationDbContext();
        var handler = new UpdateSystemSettingsCommandHandler(db);
        var result = await handler.Handle(DefaultCommand(), CancellationToken.None);
        Assert.False(result.IsSuccess);
        Assert.Equal(TopLab.Application.Common.Results.ErrorType.Unexpected, result.Error!.Type);
    }
}