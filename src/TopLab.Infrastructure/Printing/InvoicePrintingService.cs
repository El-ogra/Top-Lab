using System.Text.Json;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientBilling.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Settings;

namespace TopLab.Infrastructure.Printing;

/// <summary>
/// PDF-first implementation of <c>IInvoicePrintingService</c> (S-01 slice S3).
/// Deserializes the invoice token, reads the workstation-local lab header text
/// via the existing <c>ILabPrintTextStore</c> port, renders an Arabic RTL
/// itemized invoice via <see cref="IInvoicePdfWriter"/>, and dispatches it to
/// the printer routed through <c>PrinterAssignment</c> (OutputType = Receipt —
/// Settled Decision SD-8: no fifth output type).
/// Every failure surfaces as <c>Error.Unexpected</c> — this service never throws.
/// </summary>
public sealed class InvoicePrintingService : IInvoicePrintingService
{
    private readonly IApplicationDbContext _db;
    private readonly IInvoicePdfWriter _writer;
    private readonly ILabPrintTextStore _labTextStore;
    private readonly IPdfPrinterDispatcher _dispatcher;

    public InvoicePrintingService(
        IApplicationDbContext db,
        IInvoicePdfWriter writer,
        ILabPrintTextStore labTextStore,
        IPdfPrinterDispatcher dispatcher)
    {
        _db = db;
        _writer = writer;
        _labTextStore = labTextStore;
        _dispatcher = dispatcher;
    }

    public async Task<Result> PrintInvoiceAsync(string invoiceToken, CancellationToken cancellationToken = default)
    {
        try
        {
            InvoiceDto? invoice = null;
            try
            {
                var envelope = JsonSerializer.Deserialize<InvoicePrintEnvelope>(invoiceToken);
                invoice = envelope is null ? null : JsonSerializer.Deserialize<InvoiceDto>(envelope.InvoiceJson);
            }
            catch (JsonException)
            {
                invoice = null;
            }

            if (invoice is null)
            {
                return Result.Failure(Error.Unexpected("بيانات الفاتورة غير صالحة."));
            }

            var assignment = _db.Set<PrinterAssignment>().FirstOrDefault(a => a.OutputType == PrinterOutputType.Receipt);
            if (assignment is null)
            {
                return Result.Failure(Error.Unexpected("لم يتم تعيين طابعة للإيصالات."));
            }

            var labText = await _labTextStore.GetAsync(LabPrintTextScope.Receipt, cancellationToken);
            if (!labText.IsSuccess)
            {
                return Result.Failure(labText.Error!);
            }

            var pdfPath = Path.Combine(Path.GetTempPath(), $"TopLabInvoice-{Guid.NewGuid():N}.pdf");
            await _writer.WritePdfAsync(pdfPath, invoice, labText.Value!, cancellationToken);
            await _dispatcher.DispatchAsync(pdfPath, assignment.PrinterName, cancellationToken);

            // The temp PDF is intentionally left in the OS temp directory.
            return Result.Success();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return Result.Failure(Error.Unexpected("تعذر طباعة الفاتورة."));
        }
    }
}
