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
