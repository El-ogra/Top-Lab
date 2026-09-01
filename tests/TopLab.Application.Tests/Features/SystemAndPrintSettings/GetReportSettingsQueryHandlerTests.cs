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
    public void ReportSettingsDto_HasNoColorOrImageMembers()
    {
        var props = typeof(TopLab.Application.Features.SystemAndPrintSettings.Common.ReportSettingsDto)
            .GetProperties()
            .Select(p => p.Name)
            .ToList();
        Assert.DoesNotContain(props, p => p.Contains("Color", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(props, p => p.Contains("Image", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void AllFeatureDtos_HaveNoColorOrImageMembers()
    {
        var dtoTypes = typeof(TopLab.Application.Features.SystemAndPrintSettings.Common.SystemSettingsDto).Assembly
            .GetTypes()
            .Where(t => t.Namespace == typeof(TopLab.Application.Features.SystemAndPrintSettings.Common.SystemSettingsDto).Namespace
                && t.IsPublic
                && t.Name.EndsWith("Dto", StringComparison.Ordinal))
            .ToList();

        Assert.NotEmpty(dtoTypes);
        foreach (var type in dtoTypes)
        {
            var props = type.GetProperties().Select(p => p.Name).ToList();
            Assert.DoesNotContain(props, p => p.Contains("Color", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(props, p => p.Contains("Image", StringComparison.OrdinalIgnoreCase));
        }
    }
}