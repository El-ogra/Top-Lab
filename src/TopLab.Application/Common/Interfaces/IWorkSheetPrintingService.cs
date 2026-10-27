using TopLab.Application.Common.Results;

namespace TopLab.Application.Common.Interfaces;

/// <summary>
/// Port for worksheet printing (S-01 slice S4, Settled Decision SD-7:
/// dedicated port rather than widening the report envelope).
/// Implemented in Infrastructure (Printing/).
/// </summary>
public interface IWorkSheetPrintingService
{
    Task<Result> PrintWorkSheetAsync(string workSheetToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// GAP-17: prints a test-group worksheet token produced by
    /// <c>WorkSheetGroupPrintEnvelope</c>. Same printer routing as the visit sheet.
    /// </summary>
    Task<Result> PrintGroupWorkSheetAsync(string workSheetToken, CancellationToken cancellationToken = default);
}
