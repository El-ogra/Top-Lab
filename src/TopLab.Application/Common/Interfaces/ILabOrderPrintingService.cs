using TopLab.Application.Common.Results;

namespace TopLab.Application.Common.Interfaces;

/// <summary>
/// Port for laboratory-order (requisition) slip printing (Phase 1, REF-068).
/// Implemented in Infrastructure (Printing/). Declared here so
/// Application/Presentation depend only on the abstraction.
/// </summary>
public interface ILabOrderPrintingService
{
    Task<Result> PrintLabOrderAsync(string labOrderToken, CancellationToken cancellationToken = default);
}
