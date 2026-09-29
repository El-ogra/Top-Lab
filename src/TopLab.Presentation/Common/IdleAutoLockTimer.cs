using System;

namespace TopLab.Presentation.Common;

/// <summary>
/// Owner decision: fixed 10-minute inactivity auto-lock. Not configurable —
/// no settings storage (would require a migration). Presentation-layer only.
/// </summary>
public sealed class IdleAutoLockTimer
{
    /// <summary>Hardcoded idle timeout. Do not make this configurable (owner decision).</summary>
    public static readonly TimeSpan IdleTimeout = TimeSpan.FromMinutes(10);

    private readonly Func<Task> _onIdleAsync;
    private TimeSpan _idle;

    public IdleAutoLockTimer(Func<Task> onIdleAsync)
    {
        _onIdleAsync = onIdleAsync ?? throw new ArgumentNullException(nameof(onIdleAsync));
    }

    /// <summary>Elapsed idle time since the last activity. Exposed for tests.</summary>
    public TimeSpan IdleElapsed => _idle;

    /// <summary>Call when the user interacts with the shell; resets the idle clock.</summary>
    public void NotifyActivity() => _idle = TimeSpan.Zero;

    /// <summary>
    /// Advance the idle clock by <paramref name="delta"/>.
    /// When idle reaches <see cref="IdleTimeout"/>, invokes the lock callback exactly once
    /// and resets the clock so it cannot fire in a tight loop.
    /// </summary>
    public async Task TickAsync(TimeSpan delta)
    {
        if (delta < TimeSpan.Zero)
        {
            return;
        }

        _idle += delta;
        if (_idle < IdleTimeout)
        {
            return;
        }

        _idle = TimeSpan.Zero;
        await _onIdleAsync().ConfigureAwait(false);
    }
}
