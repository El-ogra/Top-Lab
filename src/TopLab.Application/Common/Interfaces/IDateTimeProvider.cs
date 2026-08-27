namespace TopLab.Application.Common.Interfaces;

/// <summary>
/// Supplies the current UTC time. Handlers must read time through this port rather
/// than calling <c>DateTime.UtcNow</c> directly (Coding Standards §6.6).
/// </summary>
public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
