using TopLab.Application.Common.Interfaces;

namespace TopLab.Infrastructure.Logging;

/// <summary>
/// W-02 S13 (WP-29): file sink for swallowed print exceptions. Never throws —
/// a diagnostic path must not become a fault path (same rule as
/// <see cref="FileAppLogger"/>). <c>component</c> and <c>operation</c> are fixed
/// type/method names — never a patient id, a temp path, or a result value.
/// </summary>
public sealed class PrintingDiagnostics : IPrintingDiagnostics
{
    private static readonly object Sync = new();

    private readonly string _directory;

    public PrintingDiagnostics()
        : this(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "TopLab",
            "logs"))
    {
    }

    /// <summary>Test seam: an explicit directory instead of the machine log folder.</summary>
    public PrintingDiagnostics(string directory)
    {
        _directory = directory;
    }

    public void ReportSwallowed(string component, string operation, Exception exception)
    {
        try
        {
            Directory.CreateDirectory(_directory);
            var path = Path.Combine(_directory, $"TopLab-print-{DateTime.UtcNow:yyyy-MM-dd}.log");
            var line = $"{DateTime.UtcNow:O}|PRINT|{component}|{operation}|{exception.GetType().Name}|{exception.Message}{Environment.NewLine}";
            lock (Sync)
            {
                File.AppendAllText(path, line);
            }
        }
        catch
        {
            // Diagnostics must never throw.
        }
    }
}
