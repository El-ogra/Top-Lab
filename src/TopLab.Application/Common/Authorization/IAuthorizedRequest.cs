namespace TopLab.Application.Common.Authorization;

/// <summary>
/// Marker implemented by Commands/Queries that require a specific permission.
/// <see cref="Behaviors.AuthorizationBehavior"/> enforces it uniformly before the handler
/// runs (Architecture §6.3, ADR-0009). Permission code is UPPER_SNAKE_CASE (Coding §4.4).
/// </summary>
public interface IAuthorizedRequest
{
    string RequiredPermissionCode { get; }
}
