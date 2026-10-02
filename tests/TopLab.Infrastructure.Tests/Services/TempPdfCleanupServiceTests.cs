using Xunit;

namespace TopLab.Infrastructure.Tests.Services;

/// <summary>W-02 S15 (WP-29): temp janitor owns one folder and nothing else.</summary>
public class TempPdfCleanupServiceTests
{
    [Fact]
    public void TempCleanup_RemovesOnlyFilesOlderThan24Hours()
    {
        var dir = Path.Combine(Path.GetTempPath(), $"TopLab-Test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(dir);
        var oldFile = Path.Combine(dir, "old.pdf");
        var recentFile = Path.Combine(dir, "recent.pdf");
        File.WriteAllText(oldFile, "x");
        File.WriteAllText(recentFile, "x");
        File.SetLastWriteTimeUtc(oldFile, DateTime.UtcNow - TimeSpan.FromHours(25));
        File.SetLastWriteTimeUtc(recentFile, DateTime.UtcNow);

        var removed = TopLab.Infrastructure.Hosting.TempPdfCleanupService.CleanupDirectory(dir, DateTime.UtcNow);

        Assert.Equal(1, removed);
        Assert.False(File.Exists(oldFile));
        Assert.True(File.Exists(recentFile));
        Directory.Delete(dir, true);
    }

    [Fact]
    public void TempCleanup_LeavesRecentFiles()
    {
        var dir = Path.Combine(Path.GetTempPath(), $"TopLab-Test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(dir);
        var recentFile = Path.Combine(dir, "recent.pdf");
        File.WriteAllText(recentFile, "x");

        var removed = TopLab.Infrastructure.Hosting.TempPdfCleanupService.CleanupDirectory(dir, DateTime.UtcNow);

        Assert.Equal(0, removed);
        Assert.True(File.Exists(recentFile));
        Directory.Delete(dir, true);
    }

    [Fact]
    public void TempCleanup_IgnoresOtherDirectories()
    {
        Assert.Equal(0, TopLab.Infrastructure.Hosting.TempPdfCleanupService.CleanupDirectory(
            Path.Combine(Path.GetTempPath(), $"TopLab-Test-{Guid.NewGuid():N}-missing"),
            DateTime.UtcNow));
    }

    [Fact]
    public void TempCleanup_NeverTouchesTheSystemTempRoot()
    {
        var systemRoot = Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var owned = TopLab.Infrastructure.Hosting.TempPdfCleanupService.OwnedDirectory
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        Assert.NotEqual(systemRoot, owned);
        Assert.StartsWith(
            Path.Combine(Path.GetTempPath(), "TopLab"),
            owned + Path.DirectorySeparatorChar,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void PrintingServices_WriteIntoTheOwnedTempDirectory()
    {
        var printing = File.ReadAllText(RepoFile("src", "TopLab.Infrastructure", "Printing", "ReportPrintingService.cs"));
        var barcode = File.ReadAllText(RepoFile("src", "TopLab.Infrastructure", "Barcode", "BarcodeService.cs"));

        // Both writers build the owned folder from the same two segments.
        Assert.Contains("\"TopLab\", \"Print\"", printing, StringComparison.Ordinal);
        Assert.Contains("\"TopLab\", \"Print\"", barcode, StringComparison.Ordinal);
    }

    private static string RepoFile(params string[] parts)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = parts.Aggregate(dir.FullName, Path.Combine);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException("Could not locate repository sources.");
    }
}
