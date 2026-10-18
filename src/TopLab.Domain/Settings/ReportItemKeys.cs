namespace TopLab.Domain.Settings;

/// <summary>
/// The fixed, natural string keys identifying every template-controllable line
/// kind emitted by <c>ReportContentBuilder</c> (GAP-014, Phase 7). Each constant
/// name is its own value so domain, application, infrastructure, presentation and
/// tests all compare the same token. The set was confirmed branch-by-branch
/// against the builder before seeding the migration.
/// </summary>
public static class ReportItemKeys
{
    public const string GroupSubtitle = nameof(GroupSubtitle);
    public const string TestName = nameof(TestName);
    public const string ResultValue = nameof(ResultValue);
    public const string OffLabNote = nameof(OffLabNote);
    public const string ReferenceRange = nameof(ReferenceRange);
    public const string LowComment = nameof(LowComment);
    public const string HighComment = nameof(HighComment);
    public const string TestComment = nameof(TestComment);
    public const string ProfileAnalyte = nameof(ProfileAnalyte);
    public const string ProfileReferenceRange = nameof(ProfileReferenceRange);
    public const string ProfileLowComment = nameof(ProfileLowComment);
    public const string ProfileHighComment = nameof(ProfileHighComment);
    public const string CultureLine = nameof(CultureLine);
    public const string MicroscopyLine = nameof(MicroscopyLine);
    public const string CultureSensitivityGrid = nameof(CultureSensitivityGrid);
    public const string HistoryDate = nameof(HistoryDate);
    public const string HistoryTest = nameof(HistoryTest);
    public const string HistoryResult = nameof(HistoryResult);
    public const string HistoryReviewed = nameof(HistoryReviewed);
    public const string HistoryComment = nameof(HistoryComment);
    public const string PatientExportTestLine = nameof(PatientExportTestLine);
    public const string PatientExportProfileItems = nameof(PatientExportProfileItems);
    public const string PatientExportCultureSummary = nameof(PatientExportCultureSummary);
    public const string ProfileReportComment = nameof(ProfileReportComment);

    /// <summary>All keys in canonical, stable order (used by tests and tooling).</summary>
    public static IReadOnlyList<string> All { get; } = new[]
    {
        GroupSubtitle,
        TestName,
        ResultValue,
        OffLabNote,
        ReferenceRange,
        LowComment,
        HighComment,
        TestComment,
        ProfileAnalyte,
        ProfileReferenceRange,
        ProfileLowComment,
        ProfileHighComment,
        CultureLine,
        MicroscopyLine,
        CultureSensitivityGrid,
        HistoryDate,
        HistoryTest,
        HistoryResult,
        HistoryReviewed,
        HistoryComment,
        PatientExportTestLine,
        PatientExportProfileItems,
        PatientExportCultureSummary,
        ProfileReportComment
    };
}
