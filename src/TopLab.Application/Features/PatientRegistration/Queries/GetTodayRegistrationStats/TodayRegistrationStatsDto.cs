namespace TopLab.Application.Features.PatientRegistration.Queries.GetTodayRegistrationStats;

public sealed record TodayRegistrationStatsDto(int PatientCount, int TestCount, decimal Revenue);