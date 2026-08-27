namespace TopLab.Domain.Common;

/// <summary>
/// Base class for all domain-originated exceptions. Thrown to signal an
/// invariant violation or illegal state transition that the domain itself enforces.
/// Infrastructure translates external-library failures into a separate
/// <c>Error</c> type and returns them as <c>Result</c>; <see cref="DomainException"/>
/// is reserved for in-domain rule violations.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message)
        : base(message)
    {
    }

    protected DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
