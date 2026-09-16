using System.Text.Json;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientBilling.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Settings;

namespace TopLab.Infrastructure.Printing;

/// <summary>
/// PDF-first implementation of <c>IReceiptPrintingService</c> (S-01 slice S2).
/// Deserializes the receipt token, reads <c>ReceiptSettings</c> at print time
/// (single row, PK=1), reads the workstation-local lab header text via the
/// existing <c>ILabPrintTextStore</c> port, renders an Arabic RTL receipt via
/// <see cref="IReceiptPdfWriter"/>, and dispatches it to the printer routed
/// through <c>PrinterAssignment</c> (OutputType = Receipt).
/// Every failure surfaces as <c>Error.Unexpected</c> — this service never throws.
/// </summary>
/// <remarks>
/// <c>ReceiptSettings.PrintOnce</c> is a display-only setting: no reprint-guard
/// behavior is wired anywhere in the codebase, so this service treats every
/// call as an independent print (documented per plan §4; a reprint guard would
/// be a future behavioral decision, not part of this slice).
/// </remarks>
public sealed class ReceiptPrintingService : IReceiptPrintingService
{
    private readonly IApplicationDbContext _db;
    private readonly IReceiptPdfWriter _writer;
    private readonly ILabPrintTextStore _labTextStore;
    private readonly IPdfPrinterDispatcher _dispatcher;

    public ReceiptPrintingService(
        IApplicationDbContext db,
        IReceiptPdfWriter writer,
        ILabPrintTextStore labTextStore,
        IPdfPrinterDispatcher dispatcher)
    {
        _db = db;
        _writer = writer;
        _labTextStore = labTextStore;
        _dispatcher = dispatcher;
    }

    public async Task<Result> PrintReceiptAsync(string receiptToken, CancellationToken cancellationToken = default)
    {
        try
        {
            ReceiptDto? receipt = null;
            try
            {
                var envelope = JsonSerializer.Deserialize<ReceiptPrintEnvelope>(receiptToken);
                receipt = envelope is null ? null : JsonSerializer.Deserialize<ReceiptDto>(envelope.ReceiptJson);
            }
            catch (JsonException)
            {
                receipt = null;
            }

            if (receipt is null)
            {
                return Result.Failure(Error.Unexpected("بيانات الإيصال غير صالحة."));
            }

            var receiptSettings = _db.Set<ReceiptSettings>().SingleOrDefault(s => s.Id == 1);
            if (receiptSettings is null)
            {
                return Result.Failure(Error.Unexpected("سجل إعدادات الإيصال مفقود."));
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

            var pdfPath = Path.Combine(Path.GetTempPath(), $"TopLabReceipt-{Guid.NewGuid():N}.pdf");
            await _writer.WritePdfAsync(pdfPath, receipt, receiptSettings, labText.Value!, cancellationToken);
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
            return Result.Failure(Error.Unexpected("تعذر طباعة الإيصال."));
        }
    }
}
