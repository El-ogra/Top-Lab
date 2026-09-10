namespace TopLab.Application.Features.PatientSearch.Common;

public sealed record PatientSearchHitDto(int PatientId, string? LabId, string FullName,
    string? Title, string Sex, int AgeValue, string AgeUnit, string? NationalId,
    IReadOnlyList<string> PhoneNumbers, string AccountType, bool IsVip,
    int AggregateStatus,                        // settled seven-state status (FR-M08-007, PRD §8)
    DateTime RegistrationDateUtc, int TestCount);

public sealed record VisitSummaryDto(int PatientId, string? LabId, DateTime RegistrationDateUtc,
    string AccountType, bool IsVip, int TestCount, int ResultsEntered, int ReviewedCount,
    int PrintedCount, int DeliveredCount, bool AllResultsEntered, bool AllReviewed,
    bool AllPrinted, bool AllDelivered,
    int AggregateStatus,                        // per-visit status via the settled calculator
    decimal Balance);

public sealed record VisitHistoryDto(string LabId, string PatientFullName,
    string HistorySortMode, bool HistoryAutoDisplayEnabled,          // echoed settings (FR-M22-015)
    IReadOnlyList<VisitSummaryDto> Visits);

public sealed record VisitDetailDto(int PatientId, string FullName,
    string? Title, string Sex, int AgeValue, string AgeUnit, string? NationalId, string? Address,
    string AccountType, bool IsVip, DateTime RegistrationDateUtc, DateTime? PickupDateUtc,
    IReadOnlyList<string> PhoneNumbers, IReadOnlyList<string> MedicalConditionNames,
    IReadOnlyList<VisitTestLineDto> Tests, decimal Balance);

public sealed record VisitTestLineDto(int PatientTestId, string TestName, string TestCode,
    decimal PriceAtOrderTime, bool IsSampleDrawn, bool IsTakenOutsideLab,
    string? ResultValue, int? ResultFlag, bool IsReviewed, bool IsPrinted, int PrintCount,
    bool IsDelivered);