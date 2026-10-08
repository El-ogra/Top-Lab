using System.Text.Json;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientEnvelope.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Settings;

namespace TopLab.Infrastructure.Printing;

/// <summary>
/// PDF-first implementation of <c>ILabOrderPrintingService</c> (Phase 1,
/// REF-068). Deserializes the lab-order token, live-reads
/// <c>SystemSettings</c> (missing row ⇒ clean failure, BarcodeService
/// precedent) at print time, reads the workstation-local lab header text via
/// the existing <c>ILabPrintTextStore</c> port (scope Receipt — decision 68-B,
/// the slip originates at the S-03 billing desk), renders an Arabic RTL slip
/// via <see cref="ILabOrderPdfWriter"/>, and dispatches it to the printer
/// routed through <c>PrinterAssignment</c> (OutputType = Reports — decision
/// 68-A, SD-8 forbids a fifth output type).
/// Every failure surfaces as <c>Error.Unexpected</c> — this service never throws.
/// </summary>
public sealed class LabOrderPrintingService : ILabOrderPrintingService
{
    private readonly IApplicationDbContext _db;
    private readonly ILabOrderPdfWriter _writer;
    private readonly ILabPrintTextStore _labTextStore;
    private readonly IPdfPrinterDispatcher _dispatcher;

    public LabOrderPrintingService(
        IApplicationDbContext db,
        ILabOrderPdfWriter writer,
        ILabPrintTextStore labTextStore,
        IPdfPrinterDispatcher dispatcher)
    {
        _db = db;
        _writer = writer;
        _labTextStore = labTextStore;
        _dispatcher = dispatcher;
    }

    public async Task<Result> PrintLabOrderAsync(string labOrderToken, CancellationToken cancellationToken = default)
    {
        try
        {
            LabOrderDto? order = null;
            try
            {
                var envelope = JsonSerializer.Deserialize<LabOrderPrintEnvelope>(labOrderToken);
                order = envelope is null ? null : JsonSerializer.Deserialize<LabOrderDto>(envelope.LabOrderJson);
            }
            catch (JsonException)
            {
                order = null;
            }

            if (order is null)
            {
                return Result.Failure(Error.Unexpected("بيانات طلب التحاليل غير صالحة."));
            }

            var systemSettings = _db.Set<SystemSettings>().SingleOrDefault(s => s.Id == 1);
            if (systemSettings is null)
            {
                return Result.Failure(Error.Unexpected("سجل إعدادات النظام مفقود."));
            }

            var assignment = _db.Set<PrinterAssignment>().FirstOrDefault(a => a.OutputType == PrinterOutputType.Reports);
            if (assignment is null)
            {
                return Result.Failure(Error.Unexpected("لم يتم تعيين طابعة لتقارير المختبر."));
            }

            var labText = await _labTextStore.GetAsync(LabPrintTextScope.Receipt, cancellationToken);
            if (!labText.IsSuccess)
            {
                return Result.Failure(labText.Error!);
            }

            var pdfPath = Path.Combine(Path.GetTempPath(), $"TopLabLabOrder-{Guid.NewGuid():N}.pdf");
            await _writer.WritePdfAsync(pdfPath, order, labText.Value!, cancellationToken);
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
            return Result.Failure(Error.Unexpected("تعذر طباعة طلب التحاليل."));
        }
    }
}
