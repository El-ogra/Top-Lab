using TopLab.Application.Common.Interfaces;

namespace TopLab.Infrastructure.Identity;

/// <summary>
/// Default Infrastructure implementation of <see cref="IDateTimeProvider"/>.
/// Reads <c>DateTime.UtcNow</c> from the system clock (Coding Standards §6.6).
/// </summary>
public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
