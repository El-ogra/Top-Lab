using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientSearch.Common;

namespace TopLab.Application.Features.PatientSearch.Queries.GetVisitDetail;

public sealed record GetVisitDetailQuery(int PatientId) : IRequest<Result<VisitDetailDto>>;