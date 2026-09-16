using TopLab.Application.Common.Results;

namespace TopLab.Application.Common.Interfaces;

/// <summary>
/// Port for invoice printing (S-01 slice S3, blueprint invoice document).
/// Implemented in Infrastructure (Printing/). Declared here so
/// Application/Presentation depend only on the abstraction.
/// </summary>
public interface IInvoicePrintingService
{
    Task<Result> PrintInvoiceAsync(string invoiceToken, CancellationToken cancellationToken = default);
}
