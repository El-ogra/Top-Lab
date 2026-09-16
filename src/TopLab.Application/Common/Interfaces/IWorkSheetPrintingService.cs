using TopLab.Application.Common.Results;

namespace TopLab.Application.Common.Interfaces;

/// <summary>
/// Port for visit-worksheet printing (S-01 slice S4, Settled Decision SD-7:
/// dedicated port rather than widening the report envelope).
/// Implemented in Infrastructure (Printing/).
/// </summary>
public interface IWorkSheetPrintingService
{
    Task<Result> PrintWorkSheetAsync(string workSheetToken, CancellationToken cancellationToken = default);
}
