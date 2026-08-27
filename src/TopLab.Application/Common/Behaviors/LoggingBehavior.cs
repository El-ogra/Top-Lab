using System.Diagnostics;
using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Common.Behaviors;

/// <summary>
/// Wraps every request with structured logging of the request name, the outcome
/// (success or failure with error type), and the execution duration (ADR-0009, §6.5).
/// Handlers add no ad-hoc logging that duplicates this output.
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IAppLogger _logger;

    public LoggingBehavior(IAppLogger logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();
            stopwatch.Stop();

            var outcome = response is IResult result && !result.IsSuccess
                ? $"Failure:{result.Error?.Type}"
                : "Success";

            _logger.Log(requestName, outcome, stopwatch.Elapsed);
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.Log(requestName, $"Exception:{ex.GetType().Name}", stopwatch.Elapsed);
            throw;
        }
    }
}
