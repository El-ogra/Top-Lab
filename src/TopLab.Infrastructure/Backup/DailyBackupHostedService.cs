using Microsoft.Extensions.Hosting;
using TopLab.Application.Common.Interfaces;
using TopLab.Domain.Settings;
using Microsoft.EntityFrameworkCore;

namespace TopLab.Infrastructure.Backup;

/// <summary>
/// Runs the daily scheduled backup (settled boundary §2.3-7) when system settings
/// have daily backup enabled with a valid destination. On start it runs a first
/// check, then re-checks every 24 hours. Failures are logged and never block or
/// fail application startup; no OS-level task scheduling is used.
/// </summary>
public sealed class DailyBackupHostedService : BackgroundService
{
    private readonly IDatabaseMaintenanceService _maintenance;
    private readonly IApplicationDbContext _db;
    private readonly IAppLogger _logger;
    private readonly IDateTimeProvider _time;

    public DailyBackupHostedService(
        IDatabaseMaintenanceService maintenance,
        IApplicationDbContext db,
        IAppLogger logger,
        IDateTimeProvider time)
    {
        _maintenance = maintenance;
        _db = db;
        _logger = logger;
        _time = time;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Fire-and-forget: never block startup with a database round-trip.
        _ = RunBackupCheckAsync(stoppingToken);

        using var timer = new PeriodicTimer(TimeSpan.FromHours(24));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            _ = RunBackupCheckAsync(stoppingToken);
        }
    }

    private async Task RunBackupCheckAsync(CancellationToken ct)
    {
        try
        {
            var systemSettings = await _db.Set<SystemSettings>()
                .SingleOrDefaultAsync(s => s.Id == 1, cancellationToken: ct);

            if (systemSettings is null)
            {
                return;
            }

            if (!systemSettings.DailyBackupEnabled)
            {
                return;
            }

            var destination = systemSettings.DailyBackupPath;
            if (string.IsNullOrWhiteSpace(destination))
            {
                _logger.Log(nameof(DailyBackupHostedService), "backup skipped, empty path", TimeSpan.Zero);
                return;
            }

            if (BackupExistsForToday(destination))
            {
                return;
            }

            var started = _time.UtcNow;
            var result = await _maintenance.BackupNowAsync(destination, ct);
            _logger.Log(
                nameof(DailyBackupHostedService),
                result.IsSuccess ? "backup ok" : $"backup failed: {result.Error?.Message}",
                _time.UtcNow - started);
        }
        catch (Exception ex)
        {
            _logger.Log(nameof(DailyBackupHostedService), $"backup error: {ex.Message}", TimeSpan.Zero);
        }
    }

    private static bool BackupExistsForToday(string destination)
    {
        var today = $"{DateTime.Now:yyyyMMdd}";
        var expectedPrefix = $"TopLab_{today}";
        return Directory.Exists(destination)
            && Directory.EnumerateFiles(destination, "TopLab_*.bak")
                .Any(f => Path.GetFileName(f).StartsWith(expectedPrefix, StringComparison.Ordinal));
    }
}