using TopLab.Application.Features.SystemAndPrintSettings.Commands.UpdateReportSettings;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Queries.GetReportSettings;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Settings;
using Xunit;

namespace TopLab.Application.Tests.Features.PatientSearch;

/// <summary>
/// Slice 2 — settings-dependency verification (no write surface — documented).
/// Verifies the history-viewer settings round-trip that M-08's reads depend on
/// (FR-M22-015): HistorySortMode both values and HistoryAutoDisplayEnabled toggle,
/// written via M22's shipped UpdateReportSettingsCommand and read back. This module
/// adds no write commands.
/// </summary>
public class SettingsDependencyTests
{
    private static UpdateReportSettingsCommand CommandFor(
        HistorySortMode sortMode, bool autoDisplay) => new(
            PageMarginLeftCm: 2.5m,
            PageMarginBottomCm: 3.0m,
            ReportTopSpaceCm: 8.0m,
            PaperSize: PaperSize.A5,
            HeaderFooterMode: HeaderFooterMode.Words,
            DoctorSignatureEnabled: true,
            HistorySortMode: sortMode,
            HistoryAutoDisplayEnabled: autoDisplay);

    private static async Task<ReportSettingsDto> RoundTrip(UpdateReportSettingsCommand command)
    {
        var db = new FakeApplicationDbContext();
        db.ReportSettings.Add(ReportSettings.CreateDefault());

        var update = new UpdateReportSettingsCommandHandler(db);
        var write = await update.Handle(command, CancellationToken.None);
        Assert.True(write.IsSuccess);

        var read = new GetReportSettingsQueryHandler(db);
        var result = await read.Handle(new GetReportSettingsQuery(), CancellationToken.None);
        Assert.True(result.IsSuccess);
        return result.Value!;
    }

    [Fact]
    public async Task HistorySortMode_RoundTrips_BothValues()
    {
        var byLabCode = await RoundTrip(CommandFor(HistorySortMode.ByLabCode, true));
        Assert.Equal(HistorySortMode.ByLabCode, byLabCode.HistorySortMode);

        var byPatientName = await RoundTrip(CommandFor(HistorySortMode.ByPatientName, true));
        Assert.Equal(HistorySortMode.ByPatientName, byPatientName.HistorySortMode);
    }

    [Fact]
    public async Task HistoryAutoDisplayEnabled_Toggles_RoundTrip()
    {
        var on = await RoundTrip(CommandFor(HistorySortMode.ByLabCode, true));
        Assert.True(on.HistoryAutoDisplayEnabled);

        var off = await RoundTrip(CommandFor(HistorySortMode.ByLabCode, false));
        Assert.False(off.HistoryAutoDisplayEnabled);
    }
}