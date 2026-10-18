using TopLab.Application.Features.SystemAndPrintSettings.Commands.UpdateReportSettings;
using TopLab.Application.Features.SystemAndPrintSettings.Queries.GetReportSettings;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Settings;
using Xunit;

namespace TopLab.Application.Tests.Features.SystemAndPrintSettings;

public class Phase9ReportSettingsTests
{
    private static UpdateReportSettingsCommand Valid() => new(
        PageMarginLeftCm: 1.0m,
        PageMarginBottomCm: 1.0m,
        ReportTopSpaceCm: 2.0m,
        PaperSize: PaperSize.A4,
        HeaderFooterMode: HeaderFooterMode.None,
        DoctorSignatureEnabled: false,
        HistorySortMode: HistorySortMode.ByLabCode,
        HistoryAutoDisplayEnabled: true,
        EnableAbnormalResultColor: true,
        AbnormalResultFontColor: "#FF0000",
        AbnormalResultBackgroundColor: "#FFFF00");

    [Fact]
    public void Validator_AcceptsNullColorsAndFalseFlag()
    {
        var v = new UpdateReportSettingsCommandValidator();
        var cmd = Valid() with { EnableAbnormalResultColor = false, AbnormalResultFontColor = null, AbnormalResultBackgroundColor = null };
        Assert.True(v.Validate(cmd).IsValid);
    }

    [Fact]
    public void Validator_RejectsOverlongColors()
    {
        var v = new UpdateReportSettingsCommandValidator();
        Assert.False(v.Validate(Valid() with { AbnormalResultFontColor = new string('A', 10) }).IsValid);
        Assert.False(v.Validate(Valid() with { AbnormalResultBackgroundColor = new string('A', 10) }).IsValid);
    }

    [Fact]
    public async Task Handler_CarriesNewFields()
    {
        var db = new FakeApplicationDbContext();
        db.ReportSettings.Add(ReportSettings.CreateDefault());
        var handler = new UpdateReportSettingsCommandHandler(db);
        var result = await handler.Handle(Valid(), CancellationToken.None);
        Assert.True(result.IsSuccess);
        Assert.True(db.ReportSettings[0].EnableAbnormalResultColor);
        Assert.Equal("#FF0000", db.ReportSettings[0].AbnormalResultFontColor);
        Assert.Equal("#FFFF00", db.ReportSettings[0].AbnormalResultBackgroundColor);
    }

    [Fact]
    public async Task Query_ReturnsNewFields()
    {
        var db = new FakeApplicationDbContext();
        var s = ReportSettings.CreateDefault();
        s.SetEnableAbnormalResultColor(true);
        s.SetAbnormalResultFontColor("#FF0000");
        s.SetAbnormalResultBackgroundColor("#FFFF00");
        db.ReportSettings.Add(s);
        var handler = new GetReportSettingsQueryHandler(db);
        var result = await handler.Handle(new GetReportSettingsQuery(), CancellationToken.None);
        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.EnableAbnormalResultColor);
        Assert.Equal("#FF0000", result.Value.AbnormalResultFontColor);
        Assert.Equal("#FFFF00", result.Value.AbnormalResultBackgroundColor);
    }
}
