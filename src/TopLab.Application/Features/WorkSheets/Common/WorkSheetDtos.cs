namespace TopLab.Application.Features.WorkSheets.Common;

public sealed record WorkSheetLineDto(
    int PatientTestId,
    int PatientId,
    string PatientFullName,
    string? LabId,
    string TestName,
    string TestCode,
    string? Barcode,
    bool IsSampleDrawn,
    DateTime? SampleDrawnAtUtc,
    bool HasResult,
    bool IsReviewed,
    int CompletionDurationMinutes);

public sealed record WorkSheetSectionDto(
    int SectionId,
    string SectionName,
    IReadOnlyList<WorkSheetLineDto> Lines);

public sealed record WorkSheetDto(
    DateOnly From,
    DateOnly To,
    string Mode,
    IReadOnlyList<WorkSheetSectionDto> Sections,
    int TotalTests,
    bool PrintFileExternalBarcode,
    bool PrintDateTimeOnTubeBarcode,
    bool PrintLabIdInsteadOfPatientId);

public sealed record WorkSheetSummaryRowDto(
    int WorkGroupLogId,
    string Name,
    int PendingCount,
    int DrawnCount,
    int ResultedCount);

public sealed record WorkSheetTestCountRowDto(
    int TestId,
    string TestName,
    string TestCode,
    int Count);

public sealed record WorkSheetTestCountDto(
    DateOnly From,
    DateOnly To,
    IReadOnlyList<WorkSheetTestCountRowDto> Rows,
    int TotalCount);

/// <summary>
/// Per-visit worksheet (S-01 slice S4): the current patient's ordered tests.
/// Reuses <see cref="WorkSheetLineDto"/> verbatim for the bench lines;
/// per-line sample-kind flags travel in the parallel <see cref="VisitWorkSheetSampleDto"/>
/// list (joined by <c>PatientTestId</c>) because the shared line DTO carries
/// only <c>IsSampleDrawn</c>.
/// </summary>
public sealed record VisitWorkSheetDto(
    int PatientId,
    string PatientFullName,
    string? LabId,
    IReadOnlyList<WorkSheetSectionDto> Sections,
    IReadOnlyList<VisitWorkSheetSampleDto> Samples,
    int TotalTests,
    bool PrintFileExternalBarcode,
    bool PrintDateTimeOnTubeBarcode,
    bool PrintLabIdInsteadOfPatientId);

public sealed record VisitWorkSheetSampleDto(
    int PatientTestId,
    bool IsUrine,
    bool IsStool,
    bool IsBlood,
    bool IsSemen,
    bool IsCsf,
    bool IsTakenOutsideLab);
