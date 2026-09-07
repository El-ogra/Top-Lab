using TopLab.Application.Features.AccessAndNavigation.Common.Interfaces;

namespace TopLab.Application.Tests.Common.Fakes;

/// <summary>
/// Deterministic fake of <see cref="IDbConnectionDescriptor"/> for
/// Application-layer tests (Test Strategy §5 / decision #2 — hand-rolled
/// fakes, no mocking library).
/// </summary>
public sealed class FakeDbConnectionDescriptor : IDbConnectionDescriptor
{
    public string? ServerName { get; init; }

    public string? DatabaseName { get; init; }
}