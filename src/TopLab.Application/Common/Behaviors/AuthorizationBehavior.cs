using System.Reflection;
using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Common.Behaviors;

/// <summary>
/// Enforces the permission declared by requests implementing <see cref="IAuthorizedRequest"/>
/// before the handler runs, returning <see cref="Result.Failure"/> of type Forbidden when
/// the current user lacks it (ADR-0009, Architecture §6.3, Coding Standards §6.3).
/// Users with <c>IsAbsolutePermission</c> bypass the per-item check. Handlers never
/// re-implement this concern.
/// </summary>
public sealed class AuthorizationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICurrentUserService _currentUser;

    public AuthorizationBehavior(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is IAuthorizedRequest authorized)
        {
            var allowed = _currentUser.IsAbsolutePermission
                || _currentUser.HasPermission(authorized.RequiredPermissionCode);

            if (!allowed)
            {
                return BuildForbidden(authorized.RequiredPermissionCode);
            }
        }

        return await next(cancellationToken);
    }

    private static TResponse BuildForbidden(string code)
    {
        var error = Error.Forbidden(
            "أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام",
            code);

        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(error);
        }

        if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            var resultT = typeof(Result<>).MakeGenericType(typeof(TResponse).GetGenericArguments());
            var failure = resultT
                .GetMethod(nameof(Result.Failure), new[] { typeof(Error) })!
                .Invoke(null, new object[] { error })!;
            return (TResponse)failure;
        }

        throw new InvalidOperationException(
            $"AuthorizationBehavior can only wrap Result/Result<T> responses, not {typeof(TResponse).Name}.");
    }
}
