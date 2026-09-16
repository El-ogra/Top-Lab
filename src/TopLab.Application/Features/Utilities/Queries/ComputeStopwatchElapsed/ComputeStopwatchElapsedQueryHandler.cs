using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.Utilities.Common;
using TopLab.Domain.Utilities;

namespace TopLab.Application.Features.Utilities.Queries.ComputeStopwatchElapsed;

public sealed class ComputeStopwatchElapsedQueryHandler
    : IRequestHandler<ComputeStopwatchElapsedQuery, Result<StopwatchElapsedDto>>
{
    private readonly IDateTimeProvider _dateTime;

    public ComputeStopwatchElapsedQueryHandler(IDateTimeProvider dateTime)
    {
        _dateTime = dateTime;
    }

    public Task<Result<StopwatchElapsedDto>> Handle(
        ComputeStopwatchElapsedQuery request,
        CancellationToken cancellationToken)
    {
        var end = request.EndUtc ?? _dateTime.UtcNow;
        try
        {
            var elapsed = StopwatchCalculator.Elapsed(request.StartUtc, end);
            return Task.FromResult(Result<StopwatchElapsedDto>.Success(
                new StopwatchElapsedDto(request.StartUtc, end, elapsed)));
        }
        catch (ArgumentException)
        {
            return Task.FromResult(Result<StopwatchElapsedDto>.Failure(
                Error.Validation("وقت النهاية يسبق وقت البداية.")));
        }
    }
}
