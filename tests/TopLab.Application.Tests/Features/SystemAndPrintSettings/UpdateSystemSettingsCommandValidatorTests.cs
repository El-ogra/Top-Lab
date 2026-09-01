using TopLab.Application.Features.SystemAndPrintSettings.Commands.UpdateSystemSettings;
using TopLab.Domain.Common.Enums;
using Xunit;

namespace TopLab.Application.Tests.Features.SystemAndPrintSettings;

public class UpdateSystemSettingsCommandValidatorTests
{
    private readonly UpdateSystemSettingsCommandValidator _validator = new();

    private static UpdateSystemSettingsCommand Valid() => new(
        DefaultAccountType: AccountType.Individual,
        SaveTreatingDoctorOnlyFromEntityWindow: false,
        EnablePatientNameSearchAssist: false,
        DisableAutoTitleInsertion: false,
        PrintFileExternalBarcode: false,
        PrintDateTimeOnTubeBarcode: false,
        PrintLabIdInsteadOfPatientId: false,
        AutoReviewAndComplete: false,
        PrintAccountInsteadOfDateOnReport: false,
        ResultScreenAccountDisplayMode: ResultScreenAccountDisplayMode.Hidden,
        DailyBackupEnabled: false,
        DailyBackupPath: null);

    [Fact]
    public void Valid_Passes()
    {
        var result = _validator.Validate(Valid());
        Assert.True(result.IsValid);
    }

    [Fact]
    public void VipDefaultAccountType_Fails()
    {
        var result = _validator.Validate(Valid() with { DefaultAccountType = AccountType.Vip });
        Assert.False(result.IsValid);
    }

    [Fact]
    public void BackupEnabledWithoutPath_Fails()
    {
        var result = _validator.Validate(Valid() with { DailyBackupEnabled = true, DailyBackupPath = null });
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateSystemSettingsCommand.DailyBackupPath));
    }
}