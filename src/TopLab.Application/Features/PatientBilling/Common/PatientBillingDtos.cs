namespace TopLab.Application.Features.PatientBilling.Common;

public sealed record PatientAccountDto(
    int PatientId,
    string PatientFullName,
    string? LabId,
    string AccountType,
    decimal TotalCharged,
    decimal TotalPaid,
    decimal TotalDiscount,
    decimal Balance,
    IReadOnlyList<ChargedTestDto> ChargedTests,
    IReadOnlyList<PaymentOperationDto> Operations);

public sealed record ChargedTestDto(
    int PatientTestId,
    string TestName,
    string TestCode,
    string ReceiptName,
    decimal PriceAtOrderTime);

public sealed record PaymentOperationDto(
    int PaymentOperationId,
    decimal Amount,
    decimal? DiscountAmount,
    bool IsExtraCharge,
    string OperationType,
    int ReceivedByUserId,
    string ReceivedByUserName,
    DateTime OperationAtUtc,
    bool IsVoided);

public sealed record ReceiptDto(
    int PatientId,
    string PatientFullName,
    string? LabId,
    IReadOnlyList<ChargedTestDto> ChargedTests,
    decimal TotalCharged,
    decimal TotalDiscount,
    decimal TotalPaid,
    decimal Balance,
    string Currency);
