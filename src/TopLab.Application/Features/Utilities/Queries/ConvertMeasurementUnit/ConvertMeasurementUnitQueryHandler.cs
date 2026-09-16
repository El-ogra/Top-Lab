using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.Utilities.Common;
using TopLab.Domain.Utilities;

namespace TopLab.Application.Features.Utilities.Queries.ConvertMeasurementUnit;

public sealed class ConvertMeasurementUnitQueryHandler
    : IRequestHandler<ConvertMeasurementUnitQuery, Result<ConversionResultDto>>
{
    public Task<Result<ConversionResultDto>> Handle(
        ConvertMeasurementUnitQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var converted = MeasurementUnitConverter.Convert(
                request.Value,
                request.FromUnit,
                request.ToUnit);

            return Task.FromResult(Result<ConversionResultDto>.Success(new ConversionResultDto(
                request.Value,
                request.FromUnit.Trim(),
                request.ToUnit.Trim(),
                converted)));
        }
        catch (ArgumentException)
        {
            return Task.FromResult(Result<ConversionResultDto>.Failure(
                Error.Validation("زوج الوحدات غير مدعوم.")));
        }
    }
}
