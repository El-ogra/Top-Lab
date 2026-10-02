using Microsoft.Extensions.Hosting;

namespace TopLab.Infrastructure.Hosting;

/// <summary>
/// W-02 S15 (WP-29): removes print PDFs older than 24 hours from the
/// application-owned temp folder only (<c>%TEMP%\TopLab\Print</c>).
/// It never touches the system temp root or any other directory.
/// </summary>
public sealed class TempPdfCleanupService : BackgroundService
{
    public static readonly TimeSpan Retention = TimeSpan.FromHours(24);

    /// <summary>The one folder this service owns. Never the system temp root.</summary>
    public static string OwnedDirectory =>
        Path.Combine(Path.GetTempPath(), "TopLab", "Print");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                CleanupDirectory(OwnedDirectory, DateTime.UtcNow);
            }
            catch
            {
                // Cleanup must never fault the host.
            }

            try
            {
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    /// <summary>Deletes files older than <see cref="Retention"/> in exactly one folder.</summary>
    public static int CleanupDirectory(string directory, DateTime utcNow)
    {
        if (!Directory.Exists(directory))
        {
            return 0;
        }

        var removed = 0;
        foreach (var file in Directory.GetFiles(directory))
        {
            try
            {
                if (File.GetLastWriteTimeUtc(file) < utcNow - Retention)
                {
                    File.Delete(file);
                    removed++;
                }
            }
            catch
            {
                // One bad file must not stop the sweep.
            }
        }

        return removed;
    }
}
