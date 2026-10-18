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
        ITestDisplayNameResolver? nameResolver = null,
        string? resolvedHeaderImagePath = null,
        string? resolvedFooterImagePath = null,
        IReadOnlyCollection<ReportItemTemplate>? itemTemplates = null,
        string? reportList = null)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        ArgumentNullException.ThrowIfNull(reportSettings);
        ArgumentNullException.ThrowIfNull(systemSettings);

        nameResolver ??= new TestDisplayNameResolver();

        var preferLabId = systemSettings.PrintLabIdInsteadOfPatientId;
        var drawHeader = reportSettings.HeaderFooterMode != HeaderFooterMode.None;
        var drawFooter = reportSettings.HeaderFooterMode != HeaderFooterMode.None;
        var doctorSignature = reportSettings.DoctorSignatureEnabled;
        var marker = reportSettings.NormalRangeMarker;

        return envelope.ReportKind switch
        {
            ReportPrintEnvelope.Combined => FromCombined(
                Deserialize<CombinedReportDto>(envelope),
                reportSettings, preferLabId, drawHeader, drawFooter, doctorSignature,
                labName, labAddress, labPhone, softwareBar, nameResolver, marker,
                resolvedHeaderImagePath, resolvedFooterImagePath, itemTemplates, reportList),
            ReportPrintEnvelope.Blank => FromBlank(
                Deserialize<BlankReportDto>(envelope),
                reportSettings, preferLabId, drawHeader, drawFooter, doctorSignature,
                labName, labAddress, labPhone, softwareBar, resolvedHeaderImagePath, resolvedFooterImagePath),
            ReportPrintEnvelope.History => FromHistory(
                Deserialize<PatientHistoryDto>(envelope),
                reportSettings, preferLabId, drawHeader, drawFooter, doctorSignature,
                labName, labAddress, labPhone, softwareBar, nameResolver, marker,
                resolvedHeaderImagePath, resolvedFooterImagePath, itemTemplates, reportList),
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
        string? softwareBar = null,
        string? resolvedHeaderImagePath = null,
        string? resolvedFooterImagePath = null,
        IReadOnlyCollection<ReportItemTemplate>? itemTemplates = null,
        string? reportList = null)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(reportSettings);
        ArgumentNullException.ThrowIfNull(systemSettings);

        var preferLabId = systemSettings.PrintLabIdInsteadOfPatientId;
        var drawHeader = reportSettings.HeaderFooterMode != HeaderFooterMode.None;
        var drawFooter = reportSettings.HeaderFooterMode != HeaderFooterMode.None;
        var marker = reportSettings.NormalRangeMarker;
        var templates = BuildTemplateMap(itemTemplates);

        var sections = new List<ReportSection>();
        foreach (var line in data.Lines)
        {
            var body = new List<ReportContentLine>();

            var testLine = Template(templates, ReportItemKeys.PatientExportTestLine);
            if (IsVisible(testLine, reportList))
            {
                body.Add(Layout(testLine, testLine.FreeTextOverride
                    ? testLine.Label
                    : $"{line.TestName}: {line.ResultValue ?? "-"}"));
            }

            if (line.FrozenRange is { } range)
            {
                var reference = Template(templates, ReportItemKeys.ReferenceRange);
                if (IsVisible(reference, reportList))
                {
                    body.Add(Layout(reference, $"{ReferencePrefix(reference, marker)} {range.MinValue} - {range.MaxValue}"));
                }

                if (line.ResultFlag == (int)ResultFlag.Low && !string.IsNullOrWhiteSpace(range.LowComment))
                {
                    var low = Template(templates, ReportItemKeys.LowComment);
                    if (IsVisible(low, reportList))
                    {
                        body.Add(Layout(low, FreeTextOrValue(low, range.LowComment!)));
                    }
                }

                if (line.ResultFlag == (int)ResultFlag.High && !string.IsNullOrWhiteSpace(range.HighComment))
                {
                    var high = Template(templates, ReportItemKeys.HighComment);
                    if (IsVisible(high, reportList))
                    {
                        body.Add(Layout(high, FreeTextOrValue(high, range.HighComment!)));
                    }
                }
            }

            var profileItems = Template(templates, ReportItemKeys.PatientExportProfileItems);
            if (IsVisible(profileItems, reportList) && line.ProfileItems.Count > 0)
            {
                if (profileItems.FreeTextOverride)
                {
                    body.Add(Layout(profileItems, profileItems.Label));
                }
                else
                {
                    body.AddRange(line.ProfileItems.Select(item => Layout(profileItems, item)));
                }
            }

            if (!string.IsNullOrWhiteSpace(line.CultureSummary))
            {
                var cultureSummary = Template(templates, ReportItemKeys.PatientExportCultureSummary);
                if (IsVisible(cultureSummary, reportList))
                {
                    body.Add(Layout(cultureSummary, FreeTextOrValue(cultureSummary, line.CultureSummary!)));
                }
            }

            sections.Add(ReportSection.FromLayoutLines(line.TestName, body));
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
            ReportNumberText: null,
            NormalRangeMarker: marker,
            HeaderImagePath: resolvedHeaderImagePath,
            FooterImagePath: resolvedFooterImagePath);
    }

    public static ReportDocumentContent FromProfileReport(
        TopLab.Application.Features.ProfileResults.Common.ProfileReportDto dto,
        ReportSettings reportSettings,
        SystemSettings systemSettings,
        string? labName = null,
        string? labAddress = null,
        string? labPhone = null,
        string? softwareBar = null,
        string? resolvedHeaderImagePath = null,
        string? resolvedFooterImagePath = null,
        IReadOnlyCollection<ReportItemTemplate>? itemTemplates = null,
        string? reportList = null)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ArgumentNullException.ThrowIfNull(reportSettings);
        ArgumentNullException.ThrowIfNull(systemSettings);

        var preferLabId = systemSettings.PrintLabIdInsteadOfPatientId;
        var drawHeader = reportSettings.HeaderFooterMode != HeaderFooterMode.None;
        var drawFooter = reportSettings.HeaderFooterMode != HeaderFooterMode.None;
        var marker = reportSettings.NormalRangeMarker;
        var templates = BuildTemplateMap(itemTemplates);

        var body = new List<ReportContentLine>();
        foreach (var profile in dto.Lines)
        {
            var unit = string.IsNullOrWhiteSpace(profile.Unit) ? string.Empty : $" {profile.Unit}";
            var analyte = Template(templates, ReportItemKeys.ProfileAnalyte);
            if (IsVisible(analyte, reportList))
            {
                body.Add(Layout(analyte, analyte.FreeTextOverride
                    ? analyte.Label
                    : $"{profile.AnalyteName}: {profile.ResultValue ?? "-"}{unit}"));
            }

            if (profile.FrozenRange is { } fr)
            {
                var reference = Template(templates, ReportItemKeys.ProfileReferenceRange);
                if (IsVisible(reference, reportList))
                {
                    body.Add(Layout(reference, $"{ReferencePrefix(reference, marker)} {fr.MinValue} - {fr.MaxValue}"));
                }

                if (profile.Flag == (int)ResultFlag.Low && !string.IsNullOrWhiteSpace(fr.LowComment))
                {
                    var low = Template(templates, ReportItemKeys.ProfileLowComment);
                    if (IsVisible(low, reportList))
                    {
                        body.Add(Layout(low, FreeTextOrValue(low, fr.LowComment!)));
                    }
                }

                if (profile.Flag == (int)ResultFlag.High && !string.IsNullOrWhiteSpace(fr.HighComment))
                {
                    var high = Template(templates, ReportItemKeys.ProfileHighComment);
                    if (IsVisible(high, reportList))
                    {
                        body.Add(Layout(high, FreeTextOrValue(high, fr.HighComment!)));
                    }
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(dto.Comment))
        {
            var comment = Template(templates, ReportItemKeys.ProfileReportComment);
            if (IsVisible(comment, reportList))
            {
                body.Add(Layout(comment, FreeTextOrValue(comment, dto.Comment!)));
            }
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
            Sections: new[] { ReportSection.FromLayoutLines(null, body) },
            DrawHeader: drawHeader,
            DrawFooter: drawFooter,
            DoctorSignatureEnabled: reportSettings.DoctorSignatureEnabled,
            ReportDateText: DateTime.Now.ToString("yyyy/MM/dd"),
            ReportNumberText: null,
            NormalRangeMarker: marker,
            HeaderImagePath: resolvedHeaderImagePath,
            FooterImagePath: resolvedFooterImagePath);
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
        ITestDisplayNameResolver nameResolver,
        string? marker,
        string? resolvedHeaderImagePath,
        string? resolvedFooterImagePath,
        IReadOnlyCollection<ReportItemTemplate>? itemTemplates,
        string? reportList)
    {
        var templates = BuildTemplateMap(itemTemplates);
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
                var groupSubtitle = Template(templates, ReportItemKeys.GroupSubtitle);
                if (IsVisible(groupSubtitle, reportList))
                {
                    var subtitleText = $"{groupSubtitle.Label}: {line.TestGroupName}";
                    sections.Add(ReportSection.FromLayoutLines(
                        subtitleText,
                        Array.Empty<ReportContentLine>(),
                        Layout(groupSubtitle, subtitleText)));
                }
            }

            var testName = nameResolver.ResolveReportName(line);
            var body = new List<ReportContentLine>();

            var testNameItem = Template(templates, ReportItemKeys.TestName);
            if (IsVisible(testNameItem, reportList))
            {
                body.Add(Layout(testNameItem, $"{testNameItem.Label}: {testName}"));
            }

            var resultValueItem = Template(templates, ReportItemKeys.ResultValue);
            if (IsVisible(resultValueItem, reportList))
            {
                body.Add(Layout(resultValueItem, $"{resultValueItem.Label}: {line.ResultValue ?? "-"}"));
            }

            // W-02 S8 (WP-13): off-lab note travels only with its own line.
            if (line.IsTakenOutsideLab)
            {
                var offLabNoteItem = Template(templates, ReportItemKeys.OffLabNote);
                if (IsVisible(offLabNoteItem, reportList))
                {
                    body.Add(Layout(offLabNoteItem, offLabNoteItem.Label));
                }
            }

            if (!string.IsNullOrWhiteSpace(line.FrozenRangeText))
            {
                var referenceItem = Template(templates, ReportItemKeys.ReferenceRange);
                if (IsVisible(referenceItem, reportList))
                {
                    body.Add(Layout(referenceItem, $"{ReferencePrefix(referenceItem, marker)} {line.FrozenRangeText}"));
                }
            }

            // WP-07: only the matching out-of-range comment is printed.
            if (line.ResultFlag == (int)ResultFlag.Low && !string.IsNullOrWhiteSpace(line.LowComment))
            {
                var lowCommentItem = Template(templates, ReportItemKeys.LowComment);
                if (IsVisible(lowCommentItem, reportList))
                {
                    body.Add(Layout(lowCommentItem, FreeTextOrValue(lowCommentItem, line.LowComment!)));
                }
            }

            if (line.ResultFlag == (int)ResultFlag.High && !string.IsNullOrWhiteSpace(line.HighComment))
            {
                var highCommentItem = Template(templates, ReportItemKeys.HighComment);
                if (IsVisible(highCommentItem, reportList))
                {
                    body.Add(Layout(highCommentItem, FreeTextOrValue(highCommentItem, line.HighComment!)));
                }
            }

            // W-02 S8 (WP-13): test comments print after the result.
            if (line.TestComments is { Count: > 0 })
            {
                var testCommentItem = Template(templates, ReportItemKeys.TestComment);
                if (IsVisible(testCommentItem, reportList))
                {
                    if (testCommentItem.FreeTextOverride)
                    {
                        body.Add(Layout(testCommentItem, testCommentItem.Label));
                    }
                    else
                    {
                        body.AddRange(line.TestComments.Select(comment => Layout(testCommentItem, comment)));
                    }
                }
            }

            foreach (var profile in line.ProfileLines)
            {
                var unit = string.IsNullOrWhiteSpace(profile.Unit) ? string.Empty : $" {profile.Unit}";
                var profileAnalyteItem = Template(templates, ReportItemKeys.ProfileAnalyte);
                if (IsVisible(profileAnalyteItem, reportList))
                {
                    body.Add(Layout(profileAnalyteItem, profileAnalyteItem.FreeTextOverride
                        ? profileAnalyteItem.Label
                        : $"{profile.AnalyteName}: {profile.ResultValue ?? "-"}{unit}"));
                }

                if (profile.FrozenRange is { } fr)
                {
                    var profileReferenceItem = Template(templates, ReportItemKeys.ProfileReferenceRange);
                    if (IsVisible(profileReferenceItem, reportList))
                    {
                        body.Add(Layout(profileReferenceItem, $"{ReferencePrefix(profileReferenceItem, marker)} {fr.MinValue} - {fr.MaxValue}"));
                    }

                    if (profile.Flag == (int)ResultFlag.Low && !string.IsNullOrWhiteSpace(fr.LowComment))
                    {
                        var profileLowItem = Template(templates, ReportItemKeys.ProfileLowComment);
                        if (IsVisible(profileLowItem, reportList))
                        {
                            body.Add(Layout(profileLowItem, FreeTextOrValue(profileLowItem, fr.LowComment!)));
                        }
                    }

                    if (profile.Flag == (int)ResultFlag.High && !string.IsNullOrWhiteSpace(fr.HighComment))
                    {
                        var profileHighItem = Template(templates, ReportItemKeys.ProfileHighComment);
                        if (IsVisible(profileHighItem, reportList))
                        {
                            body.Add(Layout(profileHighItem, FreeTextOrValue(profileHighItem, fr.HighComment!)));
                        }
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

                var cultureLineItem = Template(templates, ReportItemKeys.CultureLine);
                if (IsVisible(cultureLineItem, reportList))
                {
                    body.AddRange(cultureSection.BuildLines().Select(text => Layout(cultureLineItem, text)));
                }

                var microscopyLineItem = Template(templates, ReportItemKeys.MicroscopyLine);
                if (IsVisible(microscopyLineItem, reportList))
                {
                    body.AddRange(cultureSection.BuildMicroscopyLines().Select(text => Layout(microscopyLineItem, text)));
                }

                var grid = cultureSection.BuildSensitivityGrid();
                var gridItem = Template(templates, ReportItemKeys.CultureSensitivityGrid);
                if (grid.Count > 0 && IsVisible(gridItem, reportList))
                {
                    // A culture line owns its sensitivity table as a separate section.
                    sections.Add(ReportSection.FromLayoutLines(null, body));
                    sections.Add(new ReportSection(
                        gridItem.Label,
                        Array.Empty<ReportContentLine>(),
                        new ReportGrid(
                            new[] { "المضاد", "الفئة", "منطقة التثبيط (مم)", "الاسم العلمي" },
                            grid)));
                    body = new List<ReportContentLine>();
                }
            }

            sections.Add(ReportSection.FromLayoutLines(null, body));
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
            ReportNumberText: null,
            NormalRangeMarker: marker,
            HeaderImagePath: resolvedHeaderImagePath,
            FooterImagePath: resolvedFooterImagePath);
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
        string? softwareBar,
        string? resolvedHeaderImagePath,
        string? resolvedFooterImagePath)
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
            ReportNumberText: null,
            NormalRangeMarker: null,
            HeaderImagePath: resolvedHeaderImagePath,
            FooterImagePath: resolvedFooterImagePath);
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
        ITestDisplayNameResolver nameResolver,
        string? marker,
        string? resolvedHeaderImagePath,
        string? resolvedFooterImagePath,
        IReadOnlyCollection<ReportItemTemplate>? itemTemplates,
        string? reportList)
    {
        var templates = BuildTemplateMap(itemTemplates);

        var columns = new (string Key, string Label)[]
        {
            (ReportItemKeys.HistoryDate, Template(templates, ReportItemKeys.HistoryDate).Label),
            (ReportItemKeys.HistoryTest, Template(templates, ReportItemKeys.HistoryTest).Label),
            (ReportItemKeys.HistoryResult, Template(templates, ReportItemKeys.HistoryResult).Label),
            (ReportItemKeys.HistoryReviewed, Template(templates, ReportItemKeys.HistoryReviewed).Label),
            (ReportItemKeys.HistoryComment, Template(templates, ReportItemKeys.HistoryComment).Label)
        };
        var visibleColumns = columns
            .Where(c => IsVisible(Template(templates, c.Key), reportList))
            .ToList();

        ReportGrid? grid = null;
        if (visibleColumns.Count > 0)
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

                var values = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    [ReportItemKeys.HistoryDate] = e.EnteredAtUtc?.ToString("yyyy/MM/dd", System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
                    [ReportItemKeys.HistoryTest] = name,
                    [ReportItemKeys.HistoryResult] = e.ResultValue ?? "-",
                    [ReportItemKeys.HistoryReviewed] = e.IsReviewed ? "نعم" : "لا",
                    [ReportItemKeys.HistoryComment] = comment ?? string.Empty
                };

                return (IReadOnlyList<string>)visibleColumns.Select(c => values[c.Key]).ToList();
            }).ToList();

            grid = new ReportGrid(visibleColumns.Select(c => c.Label).ToList(), rows);
        }

        IReadOnlyList<ReportSection> sections = grid is null
            ? Array.Empty<ReportSection>()
            : new[] { new ReportSection(null, Array.Empty<ReportContentLine>(), grid) };

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
            Sections: sections,
            DrawHeader: drawHeader,
            DrawFooter: drawFooter,
            DoctorSignatureEnabled: doctorSignature,
            ReportDateText: DateTime.Now.ToString("yyyy/MM/dd"),
            ReportNumberText: null,
            NormalRangeMarker: marker,
            HeaderImagePath: resolvedHeaderImagePath,
            FooterImagePath: resolvedFooterImagePath);
    }

    private static Dictionary<string, ReportItemTemplate> BuildTemplateMap(
        IReadOnlyCollection<ReportItemTemplate>? itemTemplates)
    {
        var map = new Dictionary<string, ReportItemTemplate>(StringComparer.Ordinal);
        if (itemTemplates is not null)
        {
            foreach (var template in itemTemplates)
            {
                map[template.ItemKey] = template;
            }
        }

        return map;
    }

    private static ReportItemTemplate Template(
        IReadOnlyDictionary<string, ReportItemTemplate> templates,
        string key)
    {
        if (templates.TryGetValue(key, out var template))
        {
            return template;
        }

        return ReportItemTemplateCatalog.GetDefault(key);
    }

    private static bool IsVisible(ReportItemTemplate template, string? reportList)
    {
        if (!template.ShowHide)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(template.ListMembership)
            && !string.IsNullOrWhiteSpace(reportList))
        {
            return string.Equals(template.ListMembership, reportList, StringComparison.Ordinal);
        }

        return true;
    }

    private static string ReferencePrefix(ReportItemTemplate template, string? marker)
    {
        if (!string.IsNullOrWhiteSpace(template.ReferenceValueExpression))
        {
            return template.ReferenceValueExpression;
        }

        return string.IsNullOrWhiteSpace(marker) ? "المدى:" : marker;
    }

    private static string FreeTextOrValue(ReportItemTemplate template, string value)
        => template.FreeTextOverride ? template.Label : value;

    /// <summary>
    /// GAP-015: pairs the rendered text with the per-item layout of the template
    /// that produced it. The text itself is never altered here — the marker is
    /// applied by <see cref="ReportPageComposer"/> at render time.
    /// </summary>
    private static ReportContentLine Layout(ReportItemTemplate template, string text)
        => new(text, template.LineSpacing, template.LeftOffsetCm, template.MarkerSymbol);

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
