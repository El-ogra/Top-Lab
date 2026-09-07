using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientRegistration.Common;

namespace TopLab.Application.Features.PatientRegistration.Queries.GetPatientVisitHistory;

public sealed record GetPatientVisitHistoryQuery(int PatientId)
    : IRequest<Result<IReadOnlyList<VisitHistoryDto>>>;