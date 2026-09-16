namespace TopLab.Domain.Utilities;

/// <summary>
/// Pure static Domain service for stopwatch elapsed-time math (SD-23-10).
/// Holds no business state; inverted bounds are rejected explicitly.
/// </summary>
public static class StopwatchCalculator
{
    /// <exception cref="ArgumentException">When <paramref name="endUtc"/> is earlier than <paramref name="startUtc"/>.</exception>
    public static TimeSpan Elapsed(DateTime startUtc, DateTime endUtc)
    {
        if (endUtc < startUtc)
        {
            throw new ArgumentException("End time cannot be earlier than start time.", nameof(endUtc));
        }

        return endUtc - startUtc;
    }
}
