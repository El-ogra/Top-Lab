using TopLab.Application.Features.SystemAndPrintSettings.Commands.UpdateReportItemTemplates;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Settings;
using Xunit;

namespace TopLab.Application.Tests.Features.SystemAndPrintSettings;

/// <summary>
/// GAP-015 (Phase 8): the command carries the per-item layout fields through
/// the handler into the persisted rows.
/// </summary>
public class UpdateReportItemTemplatesCommandLayoutHandlerTests
{
    private static ReportItemTemplateDto Dto(
        string key,
        string label,
        decimal? lineSpacing = null,
        decimal? leftOffsetCm = null,
        string? markerSymbol = null)
        => new(key, label, null, true, null, false, lineSpacing, leftOffsetCm, markerSymbol);

    [Fact]
    public async Task MissingRow_CarriesLayoutFields_IntoTheCreatedRow()
    {
        var db = new FakeApplicationDbContext();
        var handler = new UpdateReportItemTemplatesCommandHandler(db);

        var result = await handler.Handle(
            new UpdateReportItemTemplatesCommand(
                [Dto(ReportItemKeys.TestName, "التحليل", 1.75m, 2.25m, "•")]),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var row = Assert.Single(db.ReportItemTemplates);
        Assert.Equal(1.75m, row.LineSpacing);
        Assert.Equal(2.25m, row.LeftOffsetCm);
        Assert.Equal("•", row.MarkerSymbol);
    }

    [Fact]
    public async Task ExistingRow_UpdatesLayoutFields_AndClearsThemWhenNull()
    {
        var db = new FakeApplicationDbContext();
        db.ReportItemTemplates.Add(ReportItemTemplate.Create(
            ReportItemKeys.TestName, "قديم", lineSpacing: 1m, leftOffsetCm: 1m, markerSymbol: "*"));
        var handler = new UpdateReportItemTemplatesCommandHandler(db);

        var result = await handler.Handle(
            new UpdateReportItemTemplatesCommand(
                [Dto(ReportItemKeys.TestName, "جديد", 3.5m, 0.5m, "▸")]),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var row = Assert.Single(db.ReportItemTemplates);
        Assert.Equal(3.5m, row.LineSpacing);
        Assert.Equal(0.5m, row.LeftOffsetCm);
        Assert.Equal("▸", row.MarkerSymbol);

        var cleared = await handler.Handle(
            new UpdateReportItemTemplatesCommand([Dto(ReportItemKeys.TestName, "جديد")]),
            CancellationToken.None);

        Assert.True(cleared.IsSuccess);
        Assert.Null(row.LineSpacing);
        Assert.Null(row.LeftOffsetCm);
        Assert.Null(row.MarkerSymbol);
    }

    [Fact]
    public async Task NullLayoutFields_ArePersistedAsNull()
    {
        var db = new FakeApplicationDbContext();
        var handler = new UpdateReportItemTemplatesCommandHandler(db);

        var result = await handler.Handle(
            new UpdateReportItemTemplatesCommand([Dto(ReportItemKeys.ResultValue, "النتيجة")]),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var row = Assert.Single(db.ReportItemTemplates);
        Assert.Null(row.LineSpacing);
        Assert.Null(row.LeftOffsetCm);
        Assert.Null(row.MarkerSymbol);
    }
}