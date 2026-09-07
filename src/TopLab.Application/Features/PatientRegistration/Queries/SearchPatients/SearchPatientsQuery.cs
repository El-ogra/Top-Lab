using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientRegistration.Common;

namespace TopLab.Application.Features.PatientRegistration.Queries.SearchPatients;

public sealed record SearchPatientsQuery(
    string? SearchTerm,
    int Page,
    int PageSize) : IRequest<Result<IReadOnlyList<PatientSummaryDto>>>;