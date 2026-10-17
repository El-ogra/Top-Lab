using TopLab.Application.Features.SystemAndPrintSettings.Queries.GetReportSettings;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Settings;
using Xunit;

namespace TopLab.Application.Tests.Features.SystemAndPrintSettings;

public class GetReportSettingsQueryHandlerTests
{
    [Fact]
    public async Task GetReportSettings_ReturnsSeededDefaults_NoColorFields()
    {
        var db = new FakeApplicationDbContext();
        db.ReportSettings.Add(ReportSettings.CreateDefault());

        var handler = new GetReportSettingsQueryHandler(db);
        var result = await handler.Handle(new GetReportSettingsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1.0m, result.Value!.PageMarginLeftCm);
        Assert.Equal(1.0m, result.Value.PageMarginBottomCm);
        Assert.Equal(2.0m, result.Value.ReportTopSpaceCm);
        Assert.Equal(TopLab.Domain.Common.Enums.PaperSize.A4, result.Value.PaperSize);
        Assert.Equal(TopLab.Domain.Common.Enums.HeaderFooterMode.None, result.Value.HeaderFooterMode);
        Assert.False(result.Value.DoctorSignatureEnabled);
        Assert.Equal(TopLab.Domain.Common.Enums.HistorySortMode.ByLabCode, result.Value.HistorySortMode);
        Assert.True(result.Value.HistoryAutoDisplayEnabled);
    }

    [Fact]
    public void ReportSettingsDto_HasColorAndImageMembers()
    {
        var props = typeof(TopLab.Application.Features.SystemAndPrintSettings.Common.ReportSettingsDto)
            .GetProperties()
            .Select(p => p.Name)
            .ToList();
        Assert.Contains(props, p => p == "HeaderColor");
        Assert.Contains(props, p => p == "FooterColor");
        Assert.Contains(props, p => p == "HeaderImagePath");
        Assert.Contains(props, p => p == "FooterImagePath");
        Assert.Contains(props, p => p == "NormalRangeMarker");
    }

    [Fact]
    public async Task GetReportSettings_ReturnsColorImageAndMarkerFields()
    {
        var db = new FakeApplicationDbContext();
        var existing = ReportSettings.CreateDefault();
        existing.SetHeaderColor("#FF0000");
        existing.SetFooterColor("#00FF00");
        existing.SetHeaderImagePath("Header.png");
        existing.SetFooterImagePath("Footer.png");
        existing.SetNormalRangeMarker("*");
        db.ReportSettings.Add(existing);

        var handler = new GetReportSettingsQueryHandler(db);
        var result = await handler.Handle(new GetReportSettingsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("#FF0000", result.Value!.HeaderColor);
        Assert.Equal("#00FF00", result.Value.FooterColor);
        Assert.Equal("Header.png", result.Value.HeaderImagePath);
        Assert.Equal("Footer.png", result.Value.FooterImagePath);
        Assert.Equal("*", result.Value.NormalRangeMarker);
    }
}