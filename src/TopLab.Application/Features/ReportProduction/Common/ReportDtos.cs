namespace TopLab.Application.Features.ReportProduction.Common;

public sealed record CombinableTestDto(
    int PatientTestId,
    int TestId,
    string TestName,
    string TestCode,
    int ResultKind);

public sealed record FrozenProfileRangeDto(
    int AnalyteId,
    string AnalyteName,
    string? Sex,
    string AgeUnit,
    int AgeMin,
    int AgeMax,
    decimal MinValue,
    decimal MaxValue,
    string? LowComment,
    string? HighComment,
    DateTimeOffset CapturedAtUtc);

public sealed record ProfileReportLineDto(
    int ProfileResultItemId,
    int AnalyteId,
    string AnalyteName,
    string? ResultValue,
    string? Unit,
    int? Flag,
    FrozenProfileRangeDto? FrozenRange);

/// <summary>
/// GAP-017: a profile's own result styling, carried next to the result lines so the report
/// writer can apply it with precedence over the global <c>ReportSettings</c> colours.
/// A null member means "no per-profile override for that slot"; an all-null style means the
/// rendered output is byte-for-byte what Phase 9 produced.
/// </summary>
public sealed record ProfileResultStyleDto(
    string? NormalFont = null,
    string? NormalColor = null,
    string? AbnormalFont = null,
    string? AbnormalBackgroundColor = null);

public sealed record CultureReportRowDto(
    int AntibioticId,
    string AntibioticName,
    string? ScientificName,
    int? SensitivityCategory,
    decimal? InhibitionZoneMm,
    decimal? ThresholdMm);

public sealed record CultureReportSummaryDto(
    string? Sample,
    string? OrganismA,
    string? OrganismB,
    string? OrganismC,
    string? CultureCondition,
    string? ColonyCount,
    string? MicroscopyPusCells = null,
    string? MicroscopyRedBloodCells = null,
    string? MicroscopyEpithelialCells = null,
    string? MicroscopyCrystals = null,
    string? MicroscopyFungi = null,
    string? MicroscopyOthersOne = null,
    string? MicroscopyOthersTwo = null,
    string? MicroscopyOthersThree = null,
    bool MicroscopyIsDirect = false,
    IReadOnlyList<CultureReportRowDto>? SensitivityRows = null);

public sealed record CombinedReportLineDto(
    int PatientTestId,
    int TestId,
    string TestName,
    string TestCode,
    int ResultKind,
    string? ResultValue,
    int? ResultFlag,
    string? FrozenRangeText,
    IReadOnlyList<ProfileReportLineDto> ProfileLines,
    CultureReportSummaryDto? Culture,
    string? LowComment = null,
    string? HighComment = null,
    bool IsTakenOutsideLab = false,
    string? TestGroupName = null,
    IReadOnlyList<string>? TestComments = null,
    // GAP-017: set only on specialised-profile lines, so the writer can style that
    // profile's own result lines. Null on every other line keeps Phase 9 behaviour.
    ProfileResultStyleDto? ProfileStyle = null);

public sealed record CombinedReportDto(
    int PatientId,
    string PatientFullName,
    string? LabId,
    IReadOnlyList<CombinedReportLineDto> Lines);

public sealed record BlankReportDto(
    int PatientId,
    string PatientFullName,
    string? LabId,
    string Sex,
    int AgeValue,
    string AgeUnit,
    string? TreatingDoctorName,
    string? ReferralEntityName);

public sealed record HistoryEntryDto(
    int PatientTestId,
    int PatientId,
    int TestId,
    string TestName,
    string TestCode,
    int ResultKind,
    string? ResultValue,
    int? ResultFlag,
    bool IsReviewed,
    DateTime? EnteredAtUtc,
    DateTime? ReviewedAtUtc,
    string? LowComment = null,
    string? HighComment = null,
    bool IsTakenOutsideLab = false,
    IReadOnlyList<string>? TestComments = null,
    DateOnly? EnteredAtLocalDate = null);

public sealed record PatientHistoryDto(
    int PatientId,
    string PatientFullName,
    string? LabId,
    string HistorySortMode,
    bool HistoryAutoDisplayEnabled,
    IReadOnlyList<HistoryEntryDto> Entries);

public sealed record MultiPatientHistoryDto(
    string HistorySortMode,
    bool HistoryAutoDisplayEnabled,
    IReadOnlyList<HistoryEntryDto> Entries);