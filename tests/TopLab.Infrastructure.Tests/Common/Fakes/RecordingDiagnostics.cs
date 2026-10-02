using TopLab.Application.Common.Interfaces;

namespace TopLab.Infrastructure.Tests.Common.Fakes;

/// <summary>W-02 S13 (WP-29): recording diagnostics double for service tests.</summary>
public sealed class RecordingDiagnostics : IPrintingDiagnostics
{
    public List<(string Component, string Operation, Exception Exception)> Calls { get; } = new();

    public void ReportSwallowed(string component, string operation, Exception exception) =>
        Calls.Add((component, operation, exception));
}
