using System.Text.Json;
using TopLab.Application.Features.ReportProduction.Common;
using TopLab.Application.Features.ResultsEntry.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Settings;

namespace TopLab.Infrastructure.Printing;

/// <summary>
/// Maps print envelopes and export payloads onto the shared
/// <see cref="ReportDocumentContent"/> model (Arabic labels only).
/// </summary>
public static class ReportContentBuilder
{
    public static ReportDocumentContent FromEnvelope(
        ReportPrintEnvelope envelope,
        ReportSettings reportSettings,
        SystemSettings systemSettings,
        string? labName = null,
        string? labAddress = null,
        string? labPhone = null,
        string? softwareBar = null,
        ITestDisplayNameResolver? nameResolver = null)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        ArgumentNullException.ThrowIfNull(reportSettings);
        ArgumentNullException.ThrowIfNull(systemSettings);

        nameResolver ??= new TestDisplayNameResolver();

        var preferLabId = systemSettings.PrintLabIdInsteadOfPatientId;
        var drawHeader = reportSettings.HeaderFooterMode != HeaderFooterMode.None;
        var drawFooter = reportSettings.HeaderFooterMode != HeaderFooterMode.None;
        var doctorSignature = reportSettings.DoctorSignatureEnabled;

        return envelope.ReportKind switch
        {
            ReportPrintEnvelope.Combined => FromCombined(
                Deserialize<CombinedReportDto>(envelope),
                reportSettings, preferLabId, drawHeader, drawFooter, doctorSignature,
                labName, labAddress, labPhone, softwareBar, nameResolver),
            ReportPrintEnvelope.Blank => FromBlank(
                Deserialize<BlankReportDto>(envelope),
                reportSettings, preferLabId, drawHeader, drawFooter, doctorSignature,
                labName, labAddress, labPhone, softwareBar),
            ReportPrintEnvelope.History => FromHistory(
                Deserialize<PatientHistoryDto>(envelope),
                reportSettings, preferLabId, drawHeader, drawFooter, doctorSignature,
                labName, labAddress, labPhone, softwareBar, nameResolver),
            _ => throw new InvalidOperationException($"Unknown report kind '{envelope.ReportKind}'.")
        };
    }

    public static ReportDocumentContent FromPatientExport(
        PatientReportPdfData data,
        ReportSettings reportSettings,
        SystemSettings systemSettings,
        string? labName = null,
        string? labAddress = null,
        string? labPhone = null,
        string? softwareBar = null)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(reportSettings);
        ArgumentNullException.ThrowIfNull(systemSettings);

        var preferLabId = systemSettings.PrintLabIdInsteadOfPatientId;
        var drawHeader = reportSettings.HeaderFooterMode != HeaderFooterMode.None;
        var drawFooter = reportSettings.HeaderFooterMode != HeaderFooterMode.None;

        var sections = new List<ReportSection>();
        foreach (var line in data.Lines)
        {
            var body = new List<string>
            {
                $"{line.TestName}: {line.ResultValue ?? "-"}"
            };

            if (line.FrozenRange is { } range)
            {
                body.Add($"المدى: {range.MinValue} - {range.MaxValue}");
                if (line.ResultFlag == (int)ResultFlag.Low && !string.IsNullOrWhiteSpace(range.LowComment))
                {
                    body.Add(range.LowComment!);
                }

                if (line.ResultFlag == (int)ResultFlag.High && !string.IsNullOrWhiteSpace(range.HighComment))
                {
                    body.Add(range.HighComment!);
                }
            }

            body.AddRange(line.ProfileItems);
            if (!string.IsNullOrWhiteSpace(line.CultureSummary))
            {
                body.Add(line.CultureSummary!);
            }

            sections.Add(ReportSection.FromLines(line.TestName, body));
        }

        return new ReportDocumentContent(
            ReportTitle: "تقرير نتائج المريض",
            PatientFullName: data.PatientFullName,
            PatientId: data.PatientId,
            LabId: data.LabId,
            PreferLabId: preferLabId,
            LabName: labName,
            LabAddress: labAddress,
            LabPhone: labPhone,
            SoftwareBar: softwareBar,
            TreatingDoctorName: null,
            ReferralEntityName: null,
            Sex: null,
            AgeText: null,
            Sections: sections,
            DrawHeader: drawHeader,
            DrawFooter: drawFooter,
            DoctorSignatureEnabled: reportSettings.DoctorSignatureEnabled,
            ReportDateText: DateTime.Now.ToString("yyyy/MM/dd"),
            ReportNumberText: null);
    }

    public static ReportDocumentContent FromProfileReport(
        TopLab.Application.Features.ProfileResults.Common.ProfileReportDto dto,
        ReportSettings reportSettings,
        SystemSettings systemSettings,
        string? labName = null,
        string? labAddress = null,
        string? labPhone = null,
        string? softwareBar = null)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ArgumentNullException.ThrowIfNull(reportSettings);
        ArgumentNullException.ThrowIfNull(systemSettings);

        var preferLabId = systemSettings.PrintLabIdInsteadOfPatientId;
        var drawHeader = reportSettings.HeaderFooterMode != HeaderFooterMode.None;
        var drawFooter = reportSettings.HeaderFooterMode != HeaderFooterMode.None;

        var body = new List<string>();
        foreach (var profile in dto.Lines)
        {
            var unit = string.IsNullOrWhiteSpace(profile.Unit) ? string.Empty : $" {profile.Unit}";
            body.Add($"{profile.AnalyteName}: {profile.ResultValue ?? "-"}{unit}");
            if (profile.FrozenRange is { } fr)
            {
                body.Add($"المدى: {fr.MinValue} - {fr.MaxValue}");
                if (profile.Flag == (int)ResultFlag.Low && !string.IsNullOrWhiteSpace(fr.LowComment))
                {
                    body.Add(fr.LowComment!);
                }

                if (profile.Flag == (int)ResultFlag.High && !string.IsNullOrWhiteSpace(fr.HighComment))
                {
                    body.Add(fr.HighComment!);
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(dto.Comment))
        {
            body.Add(dto.Comment!);
        }

        return new ReportDocumentContent(
            ReportTitle: dto.ProfileName,
            PatientFullName: dto.PatientFullName,
            PatientId: dto.PatientId,
            LabId: dto.LabId,
            PreferLabId: preferLabId,
            LabName: labName,
            LabAddress: labAddress,
            LabPhone: labPhone,
            SoftwareBar: softwareBar,
            TreatingDoctorName: null,
            ReferralEntityName: null,
            Sex: null,
            AgeText: null,
            Sections: new[] { ReportSection.FromLines(null, body) },
            DrawHeader: drawHeader,
            DrawFooter: drawFooter,
            DoctorSignatureEnabled: reportSettings.DoctorSignatureEnabled,
            ReportDateText: DateTime.Now.ToString("yyyy/MM/dd"),
            ReportNumberText: null);
    }

    private static ReportDocumentContent FromCombined(
        CombinedReportDto dto,
        ReportSettings settings,
        bool preferLabId,
        bool drawHeader,
        bool drawFooter,
        bool doctorSignature,
        string? labName,
        string? labAddress,
        string? labPhone,
        string? softwareBar,
        ITestDisplayNameResolver nameResolver)
    {
        var sections = new List<ReportSection>();
        string? lastGroupName = null;
        foreach (var line in dto.Lines)
        {
            // W-02 S8 (WP-13): group sub-title before the first line of each group.
            if (settings.PrintGroupSubTitle
                && !string.IsNullOrWhiteSpace(line.TestGroupName)
                && line.TestGroupName != lastGroupName)
            {
                lastGroupName = line.TestGroupName;
                sections.Add(ReportSection.FromLines($"المجموعة: {line.TestGroupName}", new List<string>()));
            }

            var testName = nameResolver.ResolveReportName(line);
            var body = new List<string>
            {
                $"التحليل: {testName}",
                $"النتيجة: {line.ResultValue ?? "-"}"
            };

            // W-02 S8 (WP-13): off-lab note travels only with its own line.
            if (line.IsTakenOutsideLab)
            {
                body.Add("العينة أُخذت خارج المعمل");
            }

            if (!string.IsNullOrWhiteSpace(line.FrozenRangeText))
            {
                body.Add($"المدى: {line.FrozenRangeText}");
            }

            // WP-07: only the matching out-of-range comment is printed.
            if (line.ResultFlag == (int)ResultFlag.Low && !string.IsNullOrWhiteSpace(line.LowComment))
            {
                body.Add(line.LowComment!);
            }

            if (line.ResultFlag == (int)ResultFlag.High && !string.IsNullOrWhiteSpace(line.HighComment))
            {
                body.Add(line.HighComment!);
            }

            // W-02 S8 (WP-13): test comments print after the result.
            if (line.TestComments is { Count: > 0 })
            {
                body.AddRange(line.TestComments);
            }

            foreach (var profile in line.ProfileLines)
            {
                var unit = string.IsNullOrWhiteSpace(profile.Unit) ? string.Empty : $" {profile.Unit}";
                body.Add($"{profile.AnalyteName}: {profile.ResultValue ?? "-"}{unit}");
                if (profile.FrozenRange is { } fr)
                {
                    body.Add($"المدى: {fr.MinValue} - {fr.MaxValue}");
                    if (profile.Flag == (int)ResultFlag.Low && !string.IsNullOrWhiteSpace(fr.LowComment))
                    {
                        body.Add(fr.LowComment!);
                    }

                    if (profile.Flag == (int)ResultFlag.High && !string.IsNullOrWhiteSpace(fr.HighComment))
                    {
                        body.Add(fr.HighComment!);
                    }
                }
            }

            if (line.Culture is { } culture)
            {
                var cultureSection = new ReportCultureSection(
                    culture.Sample,
                    culture.OrganismA,
                    culture.OrganismB,
                    culture.OrganismC,
                    culture.CultureCondition,
                    culture.ColonyCount,
                    culture.MicroscopyPusCells,
                    culture.MicroscopyRedBloodCells,
                    culture.MicroscopyEpithelialCells,
                    culture.MicroscopyCrystals,
                    culture.MicroscopyFungi,
                    culture.MicroscopyOthersOne,
                    culture.MicroscopyOthersTwo,
                    culture.MicroscopyOthersThree,
                    culture.MicroscopyIsDirect,
                    (culture.SensitivityRows ?? Enumerable.Empty<TopLab.Application.Features.ReportProduction.Common.CultureReportRowDto>())
                        .Select(r => new ReportCultureSensitivityRow(
                            r.AntibioticName,
                            SensitivityLabel(r.SensitivityCategory),
                            r.InhibitionZoneMm,
                            r.ScientificName))
                        .ToList());
                body.AddRange(cultureSection.BuildLines());
                body.AddRange(cultureSection.BuildMicroscopyLines());
                var grid = cultureSection.BuildSensitivityGrid();
                if (grid.Count > 0)
                {
                    // A culture line owns its sensitivity table as a separate section.
                    sections.Add(ReportSection.FromLines(null, body));
                    sections.Add(new ReportSection(
                        "جدول الحساسية",
                        Array.Empty<string>(),
                        new ReportGrid(
                            new[] { "المضاد", "الفئة", "منطقة التثبيط (مم)", "الاسم العلمي" },
                            grid)));
                    body = new List<string>();
                }
            }

            sections.Add(ReportSection.FromLines(null, body));
        }

        return new ReportDocumentContent(
            ReportTitle: "تقرير التحاليل المركّب",
            PatientFullName: dto.PatientFullName,
            PatientId: dto.PatientId,
            LabId: dto.LabId,
            PreferLabId: preferLabId,
            LabName: labName,
            LabAddress: labAddress,
            LabPhone: labPhone,
            SoftwareBar: softwareBar,
            TreatingDoctorName: null,
            ReferralEntityName: null,
            Sex: null,
            AgeText: null,
            Sections: sections,
            DrawHeader: drawHeader,
            DrawFooter: drawFooter,
            DoctorSignatureEnabled: doctorSignature,
            ReportDateText: DateTime.Now.ToString("yyyy/MM/dd"),
            ReportNumberText: null);
    }

    private static ReportDocumentContent FromBlank(
        BlankReportDto dto,
        ReportSettings settings,
        bool preferLabId,
        bool drawHeader,
        bool drawFooter,
        bool doctorSignature,
        string? labName,
        string? labAddress,
        string? labPhone,
        string? softwareBar)
    {
        return new ReportDocumentContent(
            ReportTitle: "تقرير فارغ",
            PatientFullName: dto.PatientFullName,
            PatientId: dto.PatientId,
            LabId: dto.LabId,
            PreferLabId: preferLabId,
            LabName: labName,
            LabAddress: labAddress,
            LabPhone: labPhone,
            SoftwareBar: softwareBar,
            TreatingDoctorName: dto.TreatingDoctorName,
            ReferralEntityName: dto.ReferralEntityName,
            Sex: dto.Sex,
            AgeText: $"{dto.AgeValue} {dto.AgeUnit}",
            Sections: Array.Empty<ReportSection>(),
            DrawHeader: drawHeader,
            DrawFooter: drawFooter,
            DoctorSignatureEnabled: doctorSignature,
            ReportDateText: DateTime.Now.ToString("yyyy/MM/dd"),
            ReportNumberText: null);
    }

    private static ReportDocumentContent FromHistory(
        PatientHistoryDto dto,
        ReportSettings settings,
        bool preferLabId,
        bool drawHeader,
        bool drawFooter,
        bool doctorSignature,
        string? labName,
        string? labAddress,
        string? labPhone,
        string? softwareBar,
        ITestDisplayNameResolver nameResolver)
    {
        var rows = dto.Entries.Select(e =>
        {
            var name = nameResolver.ResolveHistoryName(new CombinedReportLineDto(
                e.PatientTestId, e.TestId, e.TestName, e.TestCode, e.ResultKind,
                e.ResultValue, e.ResultFlag, null, Array.Empty<ProfileReportLineDto>(), null,
                e.LowComment, e.HighComment));

            var comment = e.ResultFlag == (int)ResultFlag.Low
                ? e.LowComment
                : e.ResultFlag == (int)ResultFlag.High ? e.HighComment : null;

            return (IReadOnlyList<string>)new[]
            {
                e.EnteredAtUtc?.ToString("yyyy/MM/dd", System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
                name,
                e.ResultValue ?? "-",
                e.IsReviewed ? "نعم" : "لا",
                comment ?? string.Empty
            };
        }).ToList();

        var grid = new ReportGrid(
            new[] { "التاريخ", "التحليل", "النتيجة", "معتمد", "التعليق" },
            rows);

        return new ReportDocumentContent(
            ReportTitle: "تقرير التاريخ",
            PatientFullName: dto.PatientFullName,
            PatientId: dto.PatientId,
            LabId: dto.LabId,
            PreferLabId: preferLabId,
            LabName: labName,
            LabAddress: labAddress,
            LabPhone: labPhone,
            SoftwareBar: softwareBar,
            TreatingDoctorName: null,
            ReferralEntityName: null,
            Sex: null,
            AgeText: null,
            Sections: new[] { new ReportSection(null, Array.Empty<string>(), grid) },
            DrawHeader: drawHeader,
            DrawFooter: drawFooter,
            DoctorSignatureEnabled: doctorSignature,
            ReportDateText: DateTime.Now.ToString("yyyy/MM/dd"),
            ReportNumberText: null);
    }

    private static T Deserialize<T>(ReportPrintEnvelope envelope) where T : class
    {
        return JsonSerializer.Deserialize<T>(envelope.ReportJson)
            ?? throw new InvalidOperationException("Report payload is empty.");
    }

    /// <summary>
    /// W-02 S11 (WP-14): SD-4 English labels, reused verbatim from
    /// <c>CultureEntryViewModel.SensitivityOptions</c> (registered as reuse —
    /// the stage plan supplies no Arabic for these five values).
    /// </summary>
    private static string SensitivityLabel(int? category) => category switch
    {
        0 => "Sensitive",
        1 => "Intermediate",
        2 => "Low Sensitivity",
        3 => "Resistant",
        _ => "Unspecified"
    };
}
