using TopLab.Application.Common.Results;

namespace TopLab.Application.Common.Interfaces;

/// <summary>
/// Port for cashier receipt printing (S-01 slice S2, blueprint port #2).
/// Implemented in Infrastructure (Printing/). Declared here so
/// Application/Presentation depend only on the abstraction.
/// </summary>
public interface IReceiptPrintingService
{
    Task<Result> PrintReceiptAsync(string receiptToken, CancellationToken cancellationToken = default);
}
