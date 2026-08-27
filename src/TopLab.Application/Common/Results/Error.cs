namespace TopLab.Application.Common.Results;

/// <summary>
/// A failure carried by a <see cref="Result"/>. Never thrown for expected outcomes
/// (see ADR-0008). Infrastructure translates external-library exceptions into an
/// <see cref="Error"/> of type <see cref="ErrorType.Unexpected"/> before they cross
/// back into the Application layer.
/// </summary>
/// <param name="Code">Stable, machine-readable code (e.g. "Validation", "NotFound").</param>
/// <param name="Message">User-facing, Arabic-first message where the failure is shown to a user.</param>
/// <param name="Type">One of the five mandated error categories.</param>
public sealed record Error(string Code, string Message, ErrorType Type)
{
    public static Error Validation(string message, string code = "Validation")
        => new(code, message, ErrorType.Validation);

    public static Error NotFound(string message, string code = "NotFound")
        => new(code, message, ErrorType.NotFound);

    public static Error Conflict(string message, string code = "Conflict")
        => new(code, message, ErrorType.Conflict);

    public static Error Forbidden(string message, string code = "Forbidden")
        => new(code, message, ErrorType.Forbidden);

    public static Error Unexpected(string message, string code = "Unexpected")
        => new(code, message, ErrorType.Unexpected);
}
