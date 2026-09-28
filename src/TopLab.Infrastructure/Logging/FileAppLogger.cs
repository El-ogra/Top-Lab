using System.Text;
using TopLab.Application.Common.Interfaces;

namespace TopLab.Infrastructure.Logging;

/// <summary>
/// S-07 Slice 8 (F-03): durable file logger under %ProgramData%\TopLab\logs\.
/// Replaces the Debug.WriteLine call that the Release compiler deletes.
/// Signature unchanged (SD-4): Log(string requestName, string outcome, TimeSpan duration).
/// This is the structural guarantee that logs cannot carry passwords, patient
/// identifiers or results.
/// </summary>
public sealed class FileAppLogger : IAppLogger
{
    private static readonly object Lock = new();
    private readonly string _logDirectory;

    public FileAppLogger()
    {
        _logDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "TopLab", "logs");
    }

    // Test seam: allow overriding the directory
    public FileAppLogger(string logDirectory)
    {
        _logDirectory = logDirectory;
    }

    public void Log(string requestName, string outcome, TimeSpan duration)
    {
        try
        {
            Directory.CreateDirectory(_logDirectory);

            var fileName = $"app-{DateTime.UtcNow:yyyy-MM-dd}.log";
            var filePath = Path.Combine(_logDirectory, fileName);
            var line = $"{DateTime.UtcNow:O}|{requestName}|{outcome}|{duration.TotalMilliseconds:F0}ms";

            lock (Lock)
            {
                File.AppendAllText(filePath, line + Environment.NewLine, Encoding.UTF8);
            }
        }
        catch
        {
            // Logging must never throw — an unwritable path is silently ignored.
        }
    }
}
