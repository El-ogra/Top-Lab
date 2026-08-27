namespace TopLab.Application.Common.Interfaces;

/// <summary>
/// Exposes the currently authenticated user to the Application layer without
/// depending on any Infrastructure type. Implemented in Infrastructure (ADR-0013 / §6.6).
/// </summary>
public interface ICurrentUserService
{
    bool IsAuthenticated { get; }

    int UserId { get; }

    /// <summary>True for users holding absolute permission; such users bypass per-item permission checks.</summary>
    bool IsAbsolutePermission { get; }

    /// <summary>True when the current user holds the named permission code (UPPER_SNAKE_CASE).</summary>
    bool HasPermission(string code);
}
