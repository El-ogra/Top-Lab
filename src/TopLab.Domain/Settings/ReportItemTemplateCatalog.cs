namespace TopLab.Domain.Settings;

/// <summary>
/// Canonical default report-item templates (GAP-014, Phase 7). The defaults
/// reproduce the pre-Phase-7 hard-coded output exactly, so seeding them changes
/// nothing and a missing row at render time falls back to the same values.
/// Used by <c>ReportItemTemplateConfiguration</c> for seed data and by
/// <c>ReportContentBuilder</c> as the fallback for any unconfigured key.
/// </summary>
public static class ReportItemTemplateCatalog
{
    /// <summary>One default row per template-controllable builder line kind.</summary>
    public static IReadOnlyList<ReportItemTemplate> Defaults { get; } = new[]
    {
        ReportItemTemplate.Create(ReportItemKeys.GroupSubtitle, "المجموعة"),
        ReportItemTemplate.Create(ReportItemKeys.TestName, "التحليل"),
        ReportItemTemplate.Create(ReportItemKeys.ResultValue, "النتيجة"),
        ReportItemTemplate.Create(ReportItemKeys.OffLabNote, "العينة أُخذت خارج المعمل"),
        ReportItemTemplate.Create(ReportItemKeys.ReferenceRange, "المدى"),
        ReportItemTemplate.Create(ReportItemKeys.LowComment, "تعليق القيمة المنخفضة"),
        ReportItemTemplate.Create(ReportItemKeys.HighComment, "تعليق القيمة المرتفعة"),
        ReportItemTemplate.Create(ReportItemKeys.TestComment, "تعليق التحليل"),
        ReportItemTemplate.Create(ReportItemKeys.ProfileAnalyte, "التحليل"),
        ReportItemTemplate.Create(ReportItemKeys.ProfileReferenceRange, "المدى"),
        ReportItemTemplate.Create(ReportItemKeys.ProfileLowComment, "تعليق منخفض للمحلل"),
        ReportItemTemplate.Create(ReportItemKeys.ProfileHighComment, "تعليق مرتفع للمحلل"),
        ReportItemTemplate.Create(ReportItemKeys.CultureLine, "المزرعة"),
        ReportItemTemplate.Create(ReportItemKeys.MicroscopyLine, "الفحص المجهري"),
        ReportItemTemplate.Create(ReportItemKeys.CultureSensitivityGrid, "جدول الحساسية"),
        ReportItemTemplate.Create(ReportItemKeys.HistoryDate, "التاريخ"),
        ReportItemTemplate.Create(ReportItemKeys.HistoryTest, "التحليل"),
        ReportItemTemplate.Create(ReportItemKeys.HistoryResult, "النتيجة"),
        ReportItemTemplate.Create(ReportItemKeys.HistoryReviewed, "معتمد"),
        ReportItemTemplate.Create(ReportItemKeys.HistoryComment, "التعليق"),
        ReportItemTemplate.Create(ReportItemKeys.PatientExportTestLine, "التحليل"),
        ReportItemTemplate.Create(ReportItemKeys.PatientExportProfileItems, "عناصر التحليل"),
        ReportItemTemplate.Create(ReportItemKeys.PatientExportCultureSummary, "ملخص المزرعة"),
        ReportItemTemplate.Create(ReportItemKeys.ProfileReportComment, "تعليق التقرير")
    };

    private static readonly IReadOnlyDictionary<string, ReportItemTemplate> ByKey =
        Defaults.ToDictionary(t => t.ItemKey, StringComparer.Ordinal);

    /// <summary>
    /// Returns the configured default for <paramref name="itemKey"/>, or a
    /// permissive fallback (visible, structured) when the key is unknown, so a
    /// missing row can never silently remove content.
    /// </summary>
    public static ReportItemTemplate GetDefault(string itemKey)
    {
        if (!string.IsNullOrWhiteSpace(itemKey) && ByKey.TryGetValue(itemKey, out var template))
        {
            return template;
        }

        var label = string.IsNullOrWhiteSpace(itemKey) ? "-" : itemKey;
        return ReportItemTemplate.Create(label, label);
    }
}
