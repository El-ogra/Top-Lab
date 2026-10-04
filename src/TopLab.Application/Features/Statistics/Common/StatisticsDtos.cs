namespace TopLab.Application.Features.Statistics.Common;

public sealed record ClassificationCountDto(
    string Key,
    string DisplayName,
    int Count);

public sealed record MonthlyCountDto(
    int Year,
    int Month,
    int Count);

public sealed record MonthlyClassificationCountDto(
    int Year,
    int Month,
    string Key,
    string DisplayName,
    int Count);

public sealed record PatientCountStatisticsDto(
    DateOnly From,
    DateOnly To,
    int TotalCount,
    IReadOnlyList<ClassificationCountDto> SexCounts,
    IReadOnlyList<ClassificationCountDto> ReferralEntityCounts,
    IReadOnlyList<ClassificationCountDto> AccountTypeCounts,
    IReadOnlyList<MonthlyCountDto> MonthlyCounts,
    IReadOnlyList<MonthlyClassificationCountDto> MonthlySexCounts,
    IReadOnlyList<DayOfMonthCountDto> DayOfMonthCounts,
    PeriodMoneyDto? Money);

/// <summary>
/// BR-F01-2/3: the day-of-month ordinal (1…31) and how many patients registered on it
/// inside the period. Days with zero patients are not emitted, and the list is empty
/// unless <c>GroupByDayOfMonth</c> is true.
/// </summary>
public sealed record DayOfMonthCountDto(
    int Day,
    int Count);

/// <summary>
/// BR-F01-7/10: cash RECEIVED in the period. <c>AmountsPaid</c> is
/// <see cref="TopLab.Domain.Billing.PatientAccountCalculator.TotalPaid"/> over the
/// operations whose <c>OperationAtUtc</c> falls in the period; <c>PaymentCount</c> is the
/// number of rows that were summed, so zero money and zero payments stay distinguishable.
/// </summary>
public sealed record PeriodMoneyDto(
    decimal AmountsPaid,
    int PaymentCount);

public sealed record TestCountDto(
    int TestId,
    string TestName,
    int Count);

public sealed record TestGroupCountDto(
    int TestGroupId,
    string GroupName,
    int Count);

public sealed record TestCountStatisticsDto(
    DateOnly From,
    DateOnly To,
    int TotalOrders,
    IReadOnlyList<TestCountDto> Tests,
    IReadOnlyList<TestGroupCountDto> Groups);

public sealed record SentOutLabStatisticsDto(
    int LabId,
    string LabName,
    int SentCount,
    decimal TotalCost,
    decimal TotalPaid,
    decimal Remaining);

public sealed record SentOutStatisticsDto(
    DateOnly From,
    DateOnly To,
    int TotalSent,
    IReadOnlyList<SentOutLabStatisticsDto> Labs);

public sealed record UserProductivityDto(
    int UserId,
    string UserName,
    int EnteredCount,
    int ReviewedCount,
    int PrintedCount,
    int DeliveredCount);

public sealed record UserProductivityStatisticsDto(
    DateOnly From,
    DateOnly To,
    IReadOnlyList<UserProductivityDto> Users);
