using System;
using TopLab.Application.Common.Interfaces;

namespace TopLab.Application.Tests.Common.Fakes;

/// <summary>
/// Deterministic fake of <see cref="IDateTimeProvider"/>.
/// </summary>
public sealed class FakeDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow { get; init; } = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
}
