using TopLab.Domain.Common.Enums;
using TopLab.Domain.Settings;
using Xunit;

namespace TopLab.Domain.Tests.Settings;

public class SystemSettingsTests
{
    [Fact]
    public void SetDefaultAccountType_Valid_Updates()
    {
        var s = SystemSettings.CreateDefault();
        s.SetDefaultAccountType(AccountType.LabToLab);
        Assert.Equal(AccountType.LabToLab, s.DefaultAccountType);
    }

    [Fact]
    public void SetDefaultAccountType_Vip_Throws()
    {
        var s = SystemSettings.CreateDefault();
        Assert.Throws<ArgumentException>(() => s.SetDefaultAccountType(AccountType.Vip));
    }

    [Fact]
    public void SetGeneralFlags_AllEight_Updates()
    {
        var s = SystemSettings.CreateDefault();
        s.SetGeneralFlags(
            true,
            false,
            true,
            false,
            true,
            false,
            true,
            false);
        Assert.True(s.SaveTreatingDoctorOnlyFromEntityWindow);
        Assert.False(s.EnablePatientNameSearchAssist);
        Assert.True(s.DisableAutoTitleInsertion);
        Assert.False(s.PrintFileExternalBarcode);
        Assert.True(s.PrintDateTimeOnTubeBarcode);
        Assert.False(s.PrintLabIdInsteadOfPatientId);
        Assert.True(s.AutoReviewAndComplete);
        Assert.False(s.PrintAccountInsteadOfDateOnReport);
    }

    [Fact]
    public void SetResultScreenAccountDisplayMode_Updates()
    {
        var s = SystemSettings.CreateDefault();
        s.SetResultScreenAccountDisplayMode(ResultScreenAccountDisplayMode.Detailed);
        Assert.Equal(ResultScreenAccountDisplayMode.Detailed, s.ResultScreenAccountDisplayMode);
    }

    [Fact]
    public void SetDailyBackup_EnabledWithValidPath_Updates()
    {
        var s = SystemSettings.CreateDefault();
        s.SetDailyBackup(true, @"C:\backups");
        Assert.True(s.DailyBackupEnabled);
        Assert.Equal(@"C:\backups", s.DailyBackupPath);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void SetDailyBackup_EnabledNoPath_Throws(string? path)
    {
        var s = SystemSettings.CreateDefault();
        Assert.Throws<ArgumentException>(() => s.SetDailyBackup(true, path));
        Assert.False(s.DailyBackupEnabled);
    }

    [Fact]
    public void SetDailyBackup_EnabledOverlongPath_Throws()
    {
        var s = SystemSettings.CreateDefault();
        var path = new string('x', 301);
        Assert.Throws<ArgumentException>(() => s.SetDailyBackup(true, path));
        Assert.False(s.DailyBackupEnabled);
    }

    [Fact]
    public void SetDailyBackup_Disabled_KeepsStoredPath()
    {
        var s = SystemSettings.CreateDefault();
        s.SetDailyBackup(true, @"C:\backups");
        s.SetDailyBackup(false, null);
        Assert.False(s.DailyBackupEnabled);
        Assert.Equal(@"C:\backups", s.DailyBackupPath);
    }
}