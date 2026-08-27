using System.Collections.Generic;
using TopLab.Application.Common.Interfaces;

namespace TopLab.Application.Tests.Common.Fakes;

/// <summary>
/// Fake logger that records every call, so <c>LoggingBehavior</c> can be asserted.
/// </summary>
public sealed class FakeAppLogger : IAppLogger
{
    public sealed record Entry(string RequestName, string Outcome, long ElapsedTicks);

    public List<Entry> Entries { get; } = new();

    public void Log(string requestName, string outcome, TimeSpan duration)
        => Entries.Add(new Entry(requestName, outcome, duration.Ticks));
}
