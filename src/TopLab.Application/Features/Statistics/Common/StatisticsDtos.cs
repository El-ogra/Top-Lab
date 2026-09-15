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
    IReadOnlyList<MonthlyClassificationCountDto> MonthlySexCounts);

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
