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
