using TopLab.Application.Common.Interfaces;
using Xunit;

namespace TopLab.Infrastructure.Tests.Logging;

/// <summary>W-02 S13 (WP-29): the diagnostics sink observes without ever throwing.</summary>
public class PrintingDiagnosticsTests
{
    private static string NewTempDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), $"TopLab-Diag-{Guid.NewGuid():N}");
        Directory.CreateDirectory(dir);
        return dir;
    }

    [Fact]
    public void PrintingDiagnostics_WritesOneLineWithTypeAndMessage()
    {
        var dir = NewTempDir();
        var sink = new TopLab.Infrastructure.Logging.PrintingDiagnostics(dir);

        sink.ReportSwallowed("ReportPrintingService", "PrintReportAsync", new InvalidOperationException("boom"));

        var file = Assert.Single(Directory.GetFiles(dir));
        var line = Assert.Single(File.ReadAllLines(file));
        Assert.Contains("ReportPrintingService", line, StringComparison.Ordinal);
        Assert.Contains("PrintReportAsync", line, StringComparison.Ordinal);
        Assert.Contains("InvalidOperationException", line, StringComparison.Ordinal);
        Assert.Contains("boom", line, StringComparison.Ordinal);
    }

    [Fact]
    public void PrintingDiagnostics_NeverThrows()
    {
        // A file where a directory is expected: every write inside fails.
        var fileAsDir = Path.GetTempFileName();
        var sink = new TopLab.Infrastructure.Logging.PrintingDiagnostics(fileAsDir);

        var ex = Record.Exception(() => sink.ReportSwallowed("C", "O", new Exception("e")));

        Assert.Null(ex);
    }

    [Fact]
    public void PrintingDiagnostics_LineContainsNoPatientIdentifier()
    {
        var dir = NewTempDir();
        var sink = new TopLab.Infrastructure.Logging.PrintingDiagnostics(dir);

        sink.ReportSwallowed("ReportPrintingService", "PrintReportAsync", new InvalidOperationException("x"));

        var line = File.ReadAllText(Assert.Single(Directory.GetFiles(dir)));
        Assert.DoesNotContain("patientId", line, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TopLabPrint-", line, StringComparison.Ordinal);
    }

    [Fact]
    public void IAppLogger_SignatureUnchanged()
    {
        var methods = typeof(IAppLogger).GetMethods();

        var single = Assert.Single(methods);
        Assert.Equal("Log", single.Name);
        Assert.Equal(
            new[] { typeof(string), typeof(string), typeof(TimeSpan) },
            single.GetParameters().Select(p => p.ParameterType));
    }

    [Fact]
    public void IPrintingDiagnostics_IsASeparateType()
    {
        Assert.NotEqual(typeof(IAppLogger), typeof(IPrintingDiagnostics));
        Assert.False(typeof(IPrintingDiagnostics).IsAssignableFrom(typeof(IAppLogger)));
    }
}
