using TopLab.Application.Common.Interfaces;

namespace TopLab.Application.Tests.Common.Fakes;

/// <summary>W-02 S13 (WP-29): recording diagnostics double for handler tests.</summary>
public sealed class FakePrintingDiagnostics : IPrintingDiagnostics
{
    public List<(string Component, string Operation, Exception Exception)> Calls { get; } = new();

    public void ReportSwallowed(string component, string operation, Exception exception) =>
        Calls.Add((component, operation, exception));
}
