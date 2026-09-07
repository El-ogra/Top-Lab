using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientRegistration.Common;

namespace TopLab.Application.Features.PatientRegistration.Queries.GetPatientTitles;

public sealed record GetPatientTitlesQuery : IRequest<Result<IReadOnlyList<PatientTitleDto>>>;