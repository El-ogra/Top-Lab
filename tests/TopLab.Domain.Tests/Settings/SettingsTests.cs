using TopLab.Domain.Settings;
using Xunit;

namespace TopLab.Domain.Tests.Settings;

public class SettingsTests
{
    [Fact]
    public void SystemSettings_CreateDefault()
    {
        var s = SystemSettings.CreateDefault();
        Assert.Equal(1, s.Id);
        Assert.False(s.PrintLabIdInsteadOfPatientId);
    }

    [Fact]
    public void ReportSettings_CreateDefault()
    {
        var r = ReportSettings.CreateDefault();
        Assert.Equal(1, r.Id);
        Assert.Equal(2.0m, r.ReportTopSpaceCm);
    }

    [Fact]
    public void ReportSettings_TopSpace_Exceeds_Throws()
    {
        var r = ReportSettings.CreateDefault();
        Assert.Throws<ArgumentException>(() => r.SetTopSpace(9m));
        r.SetTopSpace(8m);
        Assert.Equal(8m, r.ReportTopSpaceCm);
    }
}
