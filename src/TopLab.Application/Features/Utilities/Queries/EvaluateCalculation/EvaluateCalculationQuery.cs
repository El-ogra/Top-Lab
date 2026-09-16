using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.Utilities.Common;

namespace TopLab.Application.Features.Utilities.Queries.EvaluateCalculation;

public sealed record EvaluateCalculationQuery(string Expression)
    : IRequest<Result<CalculationResultDto>>;
