using System.Collections.Generic;
using System.Reflection;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Common.Behaviors;

/// <summary>
/// Runs the request through its validator before the handler executes and, on any
/// violation, short-circuits with a <see cref="Result.Failure"/> of type Validation,
/// carrying every violated rule (ADR-0009, Architecture §6.2). Invalid input never
/// reaches a handler.
/// </summary>
/// <typeparam name="TRequest">The command or query.</typeparam>
/// <typeparam name="TResponse">Must be a <see cref="Result"/> or <see cref="Result{T}"/>.</typeparam>
public sealed class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IValidator<TRequest>? _validator;

    public ValidationBehavior(IValidator<TRequest>? validator = null)
    {
        _validator = validator;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (_validator is null)
        {
            return await next(cancellationToken);
        }

        var context = new ValidationContext<TRequest>(request);
        var failures = await _validator.ValidateAsync(context, cancellationToken);

        if (failures.IsValid)
        {
            return await next(cancellationToken);
        }

        return BuildFailure(failures.Errors.Select(e => Error.Validation(e.ErrorMessage, e.ErrorCode)).ToList());
    }

    private static TResponse BuildFailure(IReadOnlyList<Error> errors)
    {
        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(errors);
        }

        if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            var resultT = typeof(Result<>).MakeGenericType(typeof(TResponse).GetGenericArguments());
            var failure = resultT
                .GetMethod(nameof(Result.Failure), new[] { typeof(IReadOnlyList<Error>) })!
                .Invoke(null, new object[] { errors })!;
            return (TResponse)failure;
        }

        throw new InvalidOperationException(
            $"ValidationBehavior can only wrap Result/Result<T> responses, not {typeof(TResponse).Name}.");
    }
}
