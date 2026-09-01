using TopLab.Application.Features.SystemAndPrintSettings.Queries.GetSystemSettings;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Settings;
using Xunit;

namespace TopLab.Application.Tests.Features.SystemAndPrintSettings;

public class GetSystemSettingsQueryHandlerTests
{
    [Fact]
    public async Task GetSystemSettings_ReturnsSeededDefaults()
    {
        var db = new FakeApplicationDbContext();
        db.SystemSettings.Add(SystemSettings.CreateDefault());

        var handler = new GetSystemSettingsQueryHandler(db);
        var result = await handler.Handle(new GetSystemSettingsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(TopLab.Domain.Common.Enums.AccountType.Individual, result.Value!.DefaultAccountType);
        Assert.False(result.Value.SaveTreatingDoctorOnlyFromEntityWindow);
        Assert.False(result.Value.EnablePatientNameSearchAssist);
        Assert.False(result.Value.DisableAutoTitleInsertion);
        Assert.False(result.Value.PrintFileExternalBarcode);
        Assert.False(result.Value.PrintDateTimeOnTubeBarcode);
        Assert.False(result.Value.PrintLabIdInsteadOfPatientId);
        Assert.False(result.Value.AutoReviewAndComplete);
        Assert.False(result.Value.PrintAccountInsteadOfDateOnReport);
        Assert.Equal(TopLab.Domain.Common.Enums.ResultScreenAccountDisplayMode.Hidden, result.Value.ResultScreenAccountDisplayMode);
        Assert.False(result.Value.DailyBackupEnabled);
        Assert.Null(result.Value.DailyBackupPath);
    }

    [Fact]
    public async Task GetSystemSettings_MissingRow_ReturnsUnexpected()
    {
        var db = new FakeApplicationDbContext();
        var handler = new GetSystemSettingsQueryHandler(db);
        var result = await handler.Handle(new GetSystemSettingsQuery(), CancellationToken.None);
        Assert.False(result.IsSuccess);
        Assert.Equal(TopLab.Application.Common.Results.ErrorType.Unexpected, result.Error!.Type);
    }
}