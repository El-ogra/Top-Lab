using TopLab.Presentation.Common;

namespace TopLab.Presentation.Tests;

/// <summary>
/// Owner decision: idle auto-lock fires LockWorkstation after a HARDCODED 10 minutes.
/// Fake time only — no real timers, no settings.
/// </summary>
public sealed class IdleAutoLockTimerTests
{
    [Fact]
    public void IdleTimeout_IsExactlyTenMinutes_NotConfigurable()
    {
        Assert.Equal(TimeSpan.FromMinutes(10), IdleAutoLockTimer.IdleTimeout);
    }

    [Fact]
    public async Task ElapseUnderTenMinutes_DoesNotLock()
    {
        var locks = 0;
        var timer = new IdleAutoLockTimer(() =>
        {
            locks++;
            return Task.CompletedTask;
        });

        await timer.TickAsync(TimeSpan.FromMinutes(9));
        await timer.TickAsync(TimeSpan.FromSeconds(59));

        Assert.Equal(0, locks);
        Assert.Equal(TimeSpan.FromMinutes(9) + TimeSpan.FromSeconds(59), timer.IdleElapsed);
    }

    [Fact]
    public async Task ElapseAtOrOverTenMinutes_LocksOnce()
    {
        var locks = 0;
        var timer = new IdleAutoLockTimer(() =>
        {
            locks++;
            return Task.CompletedTask;
        });

        await timer.TickAsync(TimeSpan.FromMinutes(10));

        Assert.Equal(1, locks);
        Assert.Equal(TimeSpan.Zero, timer.IdleElapsed);
    }

    [Fact]
    public async Task ElapseOverTenMinutes_InOneTick_LocksOnce()
    {
        var locks = 0;
        var timer = new IdleAutoLockTimer(() =>
        {
            locks++;
            return Task.CompletedTask;
        });

        await timer.TickAsync(TimeSpan.FromMinutes(30));

        Assert.Equal(1, locks);
    }

    [Fact]
    public async Task Activity_ResetsIdleClock()
    {
        var locks = 0;
        var timer = new IdleAutoLockTimer(() =>
        {
            locks++;
            return Task.CompletedTask;
        });

        await timer.TickAsync(TimeSpan.FromMinutes(9));
        timer.NotifyActivity();
        Assert.Equal(TimeSpan.Zero, timer.IdleElapsed);

        await timer.TickAsync(TimeSpan.FromMinutes(9));
        Assert.Equal(0, locks);

        await timer.TickAsync(TimeSpan.FromMinutes(1));
        Assert.Equal(1, locks);
    }

    [Fact]
    public async Task AfterLock_ClockResets_SoSecondLockNeedsFullTimeout()
    {
        var locks = 0;
        var timer = new IdleAutoLockTimer(() =>
        {
            locks++;
            return Task.CompletedTask;
        });

        await timer.TickAsync(TimeSpan.FromMinutes(10));
        await timer.TickAsync(TimeSpan.FromMinutes(10));

        Assert.Equal(2, locks);
    }
}
