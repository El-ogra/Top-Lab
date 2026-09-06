using TopLab.Domain.Common.Enums;

namespace TopLab.Application.Features.TestCatalogAndReferenceRanges.Common;

public sealed record TestSummaryDto(
    int Id,
    string TestCode,
    string Name,
    string ReportName,
    int? TestGroupId,
    string? TestGroupName,
    string? Barcode,
    int CompletionDurationMinutes,
    bool IsSentOut,
    decimal PatientPrice,
    decimal? LabToLabPrice,
    bool IsActive);

public sealed record TestDetailDto(
    int Id,
    string TestCode,
    string Name,
    string ReportName,
    string ReceiptName,
    int? TestGroupId,
    string? TestGroupName,
    string? Barcode,
    int CompletionDurationMinutes,
    bool IsSentOut,
    decimal? SentOutCostPrice,
    decimal PatientPrice,
    decimal? LabToLabPrice,
    int ResultKind,
    bool IsCultureType,
    bool IsActive);

public sealed record TestGroupDto(int Id, string Name, bool IsActive);

public sealed record WorkGroupLogDto(int Id, string Name, IReadOnlyList<WorkGroupLogItemDto> Items);

public sealed record WorkGroupLogItemDto(int TestId, string TestName);

public sealed record ReferenceRangeDto(
    int Id,
    int TestId,
    Sex? Sex,
    AgeUnit AgeUnit,
    int AgeMin,
    int AgeMax,
    decimal MinValue,
    decimal MaxValue,
    string? LowComment,
    string? HighComment);