using TopLab.Domain.Common.Enums;

namespace TopLab.Application.Features.InventoryAndAccounting.Common;

public sealed record CommissionShareDto(
    int EntityId,
    string EntityName,
    decimal Percent,
    decimal ReferredChargeBase,
    decimal CommissionAmount);

public sealed record CashDrawerInventoryDto(
    DateOnly From,
    DateOnly To,
    int TotalSamplesCount,
    decimal TotalSamplesAmount,
    decimal DiscountsValue,
    decimal TotalAfterDiscount,
    decimal CollectedAmount,
    decimal UncollectedAmount,
    decimal CashSupplies,
    decimal Disbursements,
    decimal SafeCash,
    int SentOutCount,
    decimal SentOutTotalCost,
    decimal SentOutTotalPaid,
    decimal SentOutRemaining,
    IReadOnlyList<CommissionShareDto> CommissionsAndShares,
    decimal RemainingToLab,
    decimal NetProfit);

public enum InventoryElementKind
{
    User = 0,
    ReferralEntity = 1,
    TreatingDoctor = 2,
    AccountType = 3,
    SentOutSamples = 4
}

public enum InventoryReportType
{
    Summary = 0,
    Detailed = 1,
    DetailedByPrices = 2,
    DetailedByResults = 3
}

public sealed record ElementInventoryDto(
    DateOnly From,
    DateOnly To,
    InventoryElementKind Element,
    InventoryReportType ReportType,
    int ElementId,
    string ElementName,
    int TotalSamplesCount,
    decimal TotalSamplesAmount,
    decimal DiscountsValue,
    decimal TotalAfterDiscount,
    decimal CollectedAmount,
    decimal UncollectedAmount,
    decimal CashSupplies,
    decimal Disbursements,
    decimal SafeCash,
    int SentOutCount,
    decimal SentOutTotalCost,
    decimal SentOutTotalPaid,
    decimal SentOutRemaining,
    decimal RemainingToLab,
    decimal NetProfit,
    IReadOnlyList<ElementLineDto> Lines);

public sealed record ElementLineDto(
    int Key,
    string DisplayName,
    int Count,
    decimal Amount);

public sealed record PatientSampleDetailDto(
    int PatientId,
    string FullName,
    int TestsCount,
    decimal Charged,
    decimal Paid,
    decimal Balance);

public sealed record CashMovementDto(
    int Id,
    MovementType MovementType,
    decimal Amount,
    int? RelatedExternalEntityId,
    string? RelatedExternalEntityName,
    int PerformedByUserId,
    string PerformedByName,
    DateTime OccurredAtUtc,
    string? Notes);

public sealed record CompanyDelegateAccountDto(
    int EntityId,
    string EntityName,
    decimal Deposits,
    decimal Disbursements,
    decimal Net,
    int SentOutCount,
    decimal SentOutTotalCost,
    decimal SentOutTotalPaid,
    decimal SentOutRemaining);
