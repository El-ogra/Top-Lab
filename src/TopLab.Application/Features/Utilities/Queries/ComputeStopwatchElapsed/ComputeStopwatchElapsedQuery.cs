using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.Utilities.Common;

namespace TopLab.Application.Features.Utilities.Queries.ComputeStopwatchElapsed;

public sealed record ComputeStopwatchElapsedQuery(
    DateTime StartUtc,
    DateTime? EndUtc)
    : IRequest<Result<StopwatchElapsedDto>>;
