using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientSearch.Common;

namespace TopLab.Application.Features.PatientSearch.Queries.GetPatientByLabId;

public sealed record GetPatientByLabIdQuery(string LabId) : IRequest<Result<VisitHistoryDto>>;