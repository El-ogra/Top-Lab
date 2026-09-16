using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.Utilities.Common;

namespace TopLab.Application.Features.Utilities.Queries.ConvertMeasurementUnit;

public sealed record ConvertMeasurementUnitQuery(
    decimal Value,
    string FromUnit,
    string ToUnit)
    : IRequest<Result<ConversionResultDto>>;
