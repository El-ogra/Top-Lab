using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientSearch.Common;

namespace TopLab.Application.Features.PatientSearch.Queries.SearchPatientsGlobal;

public sealed record SearchPatientsGlobalQuery(
    string? Text,
    int Page = 1,
    int PageSize = 50) : IRequest<Result<IReadOnlyList<PatientSearchHitDto>>>;