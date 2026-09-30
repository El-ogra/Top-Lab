using TopLab.Application.Features.ReportProduction.Common;

namespace TopLab.Infrastructure.Printing;

/// <summary>
/// WP-01 initial resolver: display <c>TestName</c> only. WP-17 expands the
/// preference chain (ReportName → ArabicName → TestName) without touching the
/// PDF writer.
/// </summary>
public sealed class TestDisplayNameResolver : ITestDisplayNameResolver
{
    public string ResolveReportName(CombinedReportLineDto line)
    {
        ArgumentNullException.ThrowIfNull(line);
        return string.IsNullOrWhiteSpace(line.TestName) ? line.TestCode : line.TestName;
    }

    public string ResolveHistoryName(CombinedReportLineDto line)
    {
        return ResolveReportName(line);
    }
}
