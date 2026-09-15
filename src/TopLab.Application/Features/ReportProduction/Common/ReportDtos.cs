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

public sealed record CultureReportSummaryDto(
    string? Sample,
    string? OrganismA,
    string? OrganismB,
    string? OrganismC,
    string? CultureCondition,
    string? ColonyCount);

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
    CultureReportSummaryDto? Culture);

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
    DateTime? ReviewedAtUtc);

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