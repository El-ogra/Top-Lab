namespace TopLab.Application.Features.ResultsEntry.Common;

/// <summary>
/// Builds the printable PDF of ONE simple test's report and returns its absolute path,
/// without sending anything to a printer (canonical reference 1.22.12.3 «معاينة الطباعة»).
/// <para>
/// This port exists so the single-test result screen can offer a real preview without the
/// Presentation layer referencing the PDF writer directly. The two accepted-debt preview
/// paths on the profile and combined screens build the document inline; a fourth such
/// reference would break the layering pin, so this one goes through a port instead.
/// </para>
/// <para>
/// The implementation renders exactly what the print path renders for
/// <see cref="ResultPrintKind.SimpleResult"/> — the same combined-report envelope — so what
/// the operator previews is what the printer would produce.
/// </para>
/// </summary>
public interface ISimpleResultPreviewBuilder
{
    /// <summary>
    /// Writes the preview PDF for <paramref name="patientTestId"/> to a temporary file and
    /// returns its absolute path. Fails when the test has no report to build.
    /// </summary>
    Task<TopLab.Application.Common.Results.Result<string>> BuildAsync(
        int patientTestId,
        CancellationToken cancellationToken = default);
}
