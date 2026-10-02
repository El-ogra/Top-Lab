namespace TopLab.Application.Common.Interfaces;

/// <summary>
/// W-02 S13 (WP-29): records exceptions swallowed inside the no-throw printing
/// services. Deliberately separate from <see cref="IAppLogger"/>, whose
/// three-parameter signature is the structural guarantee that logs cannot carry
/// request payloads, passwords, patient identifiers or results. Do not merge the two.
/// </summary>
public interface IPrintingDiagnostics
{
    void ReportSwallowed(string component, string operation, Exception exception);
}
