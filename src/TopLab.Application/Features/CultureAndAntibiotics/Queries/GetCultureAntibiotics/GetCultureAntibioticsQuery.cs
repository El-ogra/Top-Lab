using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.CultureAndAntibiotics.Common;

namespace TopLab.Application.Features.CultureAndAntibiotics.Queries.GetCultureAntibiotics;

public sealed record GetCultureAntibioticsQuery(int TestId)
    : IRequest<Result<CultureAntibioticListDto>>;