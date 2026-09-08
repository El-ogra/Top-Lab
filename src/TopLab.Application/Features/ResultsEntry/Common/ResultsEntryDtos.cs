using TopLab.Application.Features.TestCatalogAndReferenceRanges.Common;

namespace TopLab.Application.Features.ResultsEntry.Common;

public sealed record ResultWorklistItemDto(
    int PatientTestId,
    int PatientId,
    string PatientFullName,
    string? LabId,
    string TestName,
    string TestCode,
    int ResultKind,
    bool IsCultureType,
    bool IsSampleDrawn,
    bool IsTakenOutsideLab,
    string? ResultValue,
    int? ResultFlag,
    bool IsReviewed,
    bool IsPrinted,
    bool IsDelivered,
    int AggregateStatus,
    DateTime RegistrationDateUtc);

public sealed record ResultEntryDto(
    int PatientTestId,
    int PatientId,
    string TestName,
    string TestCode,
    int ResultKind,
    string? ResultValue,
    int? ResultFlag,
    string? Notes,
    bool IsReviewed,
    bool IsPrinted,
    bool IsDelivered,
    int PatientAgeValue,
    string PatientAgeUnit,
    string PatientSex,
    IReadOnlyList<ReferenceRangeDto> ReferenceRanges,
    FrozenRangeDto? FrozenRange);

public sealed record FrozenRangeDto(
    int TestId,
    string? Sex,
    string AgeUnit,
    int AgeMin,
    int AgeMax,
    decimal MinValue,
    decimal MaxValue,
    string? LowComment,
    string? HighComment,
    DateTimeOffset CapturedAtUtc);

public sealed record PatientResultSheetDto(
    int PatientId,
    string PatientFullName,
    string? LabId,
    IReadOnlyList<ResultSheetLineDto> Lines);

public sealed record ResultSheetLineDto(
    int PatientTestId,
    string TestName,
    string TestCode,
    string? ResultValue,
    int? ResultFlag,
    string? Notes,
    bool IsReviewed,
    bool IsPrinted,
    FrozenRangeDto? FrozenRange);
