using TopLab.Application.Features.SystemAndPrintSettings.Commands.UpdateReportSettings;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Settings;
using Xunit;

namespace TopLab.Application.Tests.Features.SystemAndPrintSettings;

public class UpdateReportSettingsCommandHandlerTests
{
    private static UpdateReportSettingsCommand DefaultCommand() => new(
        PageMarginLeftCm: 2.5m,
        PageMarginBottomCm: 3.0m,
        ReportTopSpaceCm: 8.0m,
        PaperSize: PaperSize.A5,
        HeaderFooterMode: HeaderFooterMode.Words,
        DoctorSignatureEnabled: true,
        HistorySortMode: HistorySortMode.ByPatientName,
        HistoryAutoDisplayEnabled: false,
        HeaderColor: "#FF0000",
        FooterColor: "#00FF00",
        HeaderImagePath: "Header.png",
        FooterImagePath: "Footer.png",
        NormalRangeMarker: "*");

    [Fact]
    public async Task UpdateReportSettings_RoundTrips()
    {
        var db = new FakeApplicationDbContext();
        db.ReportSettings.Add(ReportSettings.CreateDefault());

        var handler = new UpdateReportSettingsCommandHandler(db);
        var result = await handler.Handle(DefaultCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2.5m, db.ReportSettings[0].PageMarginLeftCm);
        Assert.Equal(3.0m, db.ReportSettings[0].PageMarginBottomCm);
        Assert.Equal(8.0m, db.ReportSettings[0].ReportTopSpaceCm);
        Assert.Equal(PaperSize.A5, db.ReportSettings[0].PaperSize);
        Assert.Equal(HeaderFooterMode.Words, db.ReportSettings[0].HeaderFooterMode);
        Assert.True(db.ReportSettings[0].DoctorSignatureEnabled);
        Assert.Equal(HistorySortMode.ByPatientName, db.ReportSettings[0].HistorySortMode);
        Assert.False(db.ReportSettings[0].HistoryAutoDisplayEnabled);
        Assert.Equal("#FF0000", db.ReportSettings[0].HeaderColor);
        Assert.Equal("#00FF00", db.ReportSettings[0].FooterColor);
        Assert.Equal("Header.png", db.ReportSettings[0].HeaderImagePath);
        Assert.Equal("Footer.png", db.ReportSettings[0].FooterImagePath);
        Assert.Equal("*", db.ReportSettings[0].NormalRangeMarker);
    }

    [Fact]
    public async Task UpdateReportSettings_Over8TopSpace_LeavesRowUnchanged()
    {
        var db = new FakeApplicationDbContext();
        var existing = ReportSettings.CreateDefault();
        db.ReportSettings.Add(existing);

        var cmd = DefaultCommand() with { ReportTopSpaceCm = 9m };
        var handler = new UpdateReportSettingsCommandHandler(db);

        await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(cmd, CancellationToken.None));
        Assert.Equal(2.0m, db.ReportSettings[0].ReportTopSpaceCm);
    }

    [Fact]
    public async Task UpdateReportSettings_MissingRow_ReturnsUnexpected()
    {
        var db = new FakeApplicationDbContext();
        var handler = new UpdateReportSettingsCommandHandler(db);
        var result = await handler.Handle(DefaultCommand(), CancellationToken.None);
        Assert.False(result.IsSuccess);
        Assert.Equal(TopLab.Application.Common.Results.ErrorType.Unexpected, result.Error!.Type);
    }
}