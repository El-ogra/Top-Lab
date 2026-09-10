using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientSearch.Common;

namespace TopLab.Application.Features.PatientSearch.Queries.GetVisitHistory;

public sealed record GetVisitHistoryQuery(int PatientId) : IRequest<Result<VisitHistoryDto>>;