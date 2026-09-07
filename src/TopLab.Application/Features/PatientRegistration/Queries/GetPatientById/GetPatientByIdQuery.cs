using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientRegistration.Common;

namespace TopLab.Application.Features.PatientRegistration.Queries.GetPatientById;

public sealed record GetPatientByIdQuery(int PatientId) : IRequest<Result<PatientDetailDto>>;