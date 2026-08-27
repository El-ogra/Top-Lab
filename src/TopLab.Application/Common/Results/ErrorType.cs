namespace TopLab.Application.Common.Results;

/// <summary>
/// Classification of a failed <see cref="Result"/>. Mirrors the error taxonomy
/// mandated by Architecture Blueprint §6.1 and Coding Standards §6.1.
/// </summary>
public enum ErrorType
{
    Validation,
    NotFound,
    Conflict,
    Forbidden,
    Unexpected,
}
