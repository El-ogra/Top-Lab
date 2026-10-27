namespace TopLab.Application.Features.AuditAndTraceability.Common;

/// <summary>
/// One payment-receiving user within the P view: the receiver identity,
/// the resolved display name (raw id string when the user row is gone),
/// and the latest receipt timestamp for that receiver.
/// </summary>
public sealed record PaymentReceiverAuditDto(
    int UserId,
    string UserName,
    DateTime OperationAtUtc);

/// <summary>
/// P view (SD-10-5): patient-record audit — registering user, modification
/// count, last modifying user, and the distinct ordered payment receivers
/// (voided operations included).
/// </summary>
public sealed record PatientAuditDto(
    int PatientId,
    string FullName,
    int CreatedByUserId,
    string CreatedByUserName,
    DateTime CreatedAtUtc,
    int ModificationCount,
    int LastModifiedByUserId,
    string LastModifiedByUserName,
    DateTime LastModifiedAtUtc,
    IReadOnlyList<PaymentReceiverAuditDto> PaymentReceivers);

/// <summary>
/// T view (SD-10-6): per-test lifecycle audit — entered / reviewed /
/// printed+count / delivered, each with user and UTC time. Lifecycle fields
/// are null until the corresponding step completes.
/// </summary>
public sealed record PatientTestAuditDto(
    int PatientTestId,
    int PatientId,
    string TestName,
    int? EnteredByUserId,
    string? EnteredByUserName,
    DateTime? EnteredAtUtc,
    int? ReviewedByUserId,
    string? ReviewedByUserName,
    DateTime? ReviewedAtUtc,
    int? LastPrintedByUserId,
    string? LastPrintedByUserName,
    DateTime? LastPrintedAtUtc,
    int PrintCount,
    int? DeliveredByUserId,
    string? DeliveredByUserName,
    DateTime? DeliveredAtUtc);

/// <summary>
/// GAP-19 (4.5.1.2.2/4.5.1.2.3): one row of audit activity across all
/// audit-bearing sources, filtered by period and (optionally) by user.
/// </summary>
public sealed record AuditActivityRowDto(
    int UserId,
    string UserName,
    string ActivityCode,
    string ActivityLabel,
    DateTime OccurredAtUtc,
    int? PatientId,
    int? PatientTestId,
    string? Detail);
