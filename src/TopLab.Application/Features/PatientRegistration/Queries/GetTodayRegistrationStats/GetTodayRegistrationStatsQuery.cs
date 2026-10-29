using MediatR;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.PatientRegistration.Queries.GetTodayRegistrationStats;

public sealed record GetTodayRegistrationStatsQuery() : IRequest<Result<TodayRegistrationStatsDto>>;