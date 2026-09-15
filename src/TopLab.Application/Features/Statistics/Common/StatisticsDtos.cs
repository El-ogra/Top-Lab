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
