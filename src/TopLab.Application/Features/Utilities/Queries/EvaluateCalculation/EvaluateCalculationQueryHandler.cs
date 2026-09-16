using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.Utilities.Common;
using TopLab.Domain.Utilities;

namespace TopLab.Application.Features.Utilities.Queries.EvaluateCalculation;

public sealed class EvaluateCalculationQueryHandler
    : IRequestHandler<EvaluateCalculationQuery, Result<CalculationResultDto>>
{
    public Task<Result<CalculationResultDto>> Handle(
        EvaluateCalculationQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var value = ArithmeticCalculator.Evaluate(request.Expression);
            return Task.FromResult(Result<CalculationResultDto>.Success(
                new CalculationResultDto(request.Expression, value)));
        }
        catch (CalculatorException)
        {
            return Task.FromResult(Result<CalculationResultDto>.Failure(
                Error.Validation("لا يمكن القسمة على صفر.")));
        }
        catch (ArgumentException)
        {
            return Task.FromResult(Result<CalculationResultDto>.Failure(
                Error.Validation("التعبير الحسابي غير صالح.")));
        }
    }
}
