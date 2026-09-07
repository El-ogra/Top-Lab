using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.CultureAndAntibiotics.Common;

namespace TopLab.Application.Features.CultureAndAntibiotics.Queries.GetAntibiotics;

public sealed record GetAntibioticsQuery(string? SearchTerm)
    : IRequest<Result<IReadOnlyList<AntibioticDto>>>;