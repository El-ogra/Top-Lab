namespace TopLab.Application.Features.ResultDelivery.Common;

/// <summary>
/// Delivery-handover read DTOs (M-09 S1).
/// </summary>

/// <summary>
/// One row per visit (registration) that still has undelivered result lines.
/// <c>VisitStatus</c> is the <see cref="TopLab.Domain.PatientStatus.PatientAggregateStatus"/>
/// int value computed verbatim by <c>PatientStatusCalculator</c> over ALL of the
/// patient's tests plus the account balance.
/// </summary>
public sealed record UndeliveredPatientRowDto(
    int PatientId,
    string PatientFullName,
    string? LabId,
    DateTime RegistrationDateUtc,
    int VisitStatus,
    int UndeliveredCount);

/// <summary>
/// One row per ordered test of the patient. <c>Status</c> is the catalog
/// <c>ResultKind</c> int (same convention as <c>ResultWorklistItemDto.ResultKind</c>).
/// <c>Price</c> is the frozen <c>PatientTest.PriceAtOrderTime</c> — never a live catalog price.
/// </summary>
public sealed record DeliveryGridRowDto(
    int PatientTestId,
    string TestName,
    string TestCode,
    string? ResultValue,
    int? Flag,
    int Status,
    bool IsEntered,
    bool IsReviewed,
    bool IsPrinted,
    bool IsDelivered,
    decimal Price);

/// <summary>
/// Financial position at handover. Totals delegate to M-03's reader; the two
/// remaining amounts derive from the sign of <c>Balance</c> exclusively (OD-09-C):
/// positive = remaining to the lab, negative (credit) = remaining to the patient.
/// </summary>
public sealed record DeliveryAccountDto(
    int PatientId,
    decimal TotalCharged,
    decimal TotalPaid,
    decimal Balance,
    decimal RemainingToLab,
    decimal RemainingToPatient);
