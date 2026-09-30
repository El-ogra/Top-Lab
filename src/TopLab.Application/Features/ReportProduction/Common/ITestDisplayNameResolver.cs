namespace TopLab.Application.Features.ReportProduction.Common;

/// <summary>
/// Separates test-name lookup from the PDF writer so catalog display-name policy
/// can evolve without touching the printer (WP-01 initial: <c>TestName</c> only;
/// WP-17 expands to ReportName → ArabicName → TestName).
/// </summary>
public interface ITestDisplayNameResolver
{
    string ResolveReportName(CombinedReportLineDto line);

    string ResolveHistoryName(CombinedReportLineDto line);
}
