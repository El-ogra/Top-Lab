using System.Text.Json;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ResultDelivery.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Settings;

namespace TopLab.Infrastructure.Printing;

/// <summary>
/// PDF-first implementation of <c>IGroupPatientSlipPrintingService</c>
/// (Phase 6, GAP-013). Deserializes the batch token, live-reads
/// <c>SystemSettings</c> (missing row ⇒ clean failure, BarcodeService
/// precedent) at print time, reads the workstation-local lab header text via the
/// existing <c>ILabPrintTextStore</c> port (scope Receipt — the follow-up/delivery
/// desk sits with the billing desk, matching the lab-order slip's decision 68-B),
/// renders the Arabic RTL slip batch via <see cref="IGroupPatientSlipPdfWriter"/>,
/// and dispatches it to the printer routed through <c>PrinterAssignment</c>
/// (OutputType = Reports — SD-8 forbids a fifth output type).
/// Every failure surfaces as <c>Error.Unexpected</c> — this service never throws.
/// </summary>
public sealed class GroupPatientSlipPrintingService : IGroupPatientSlipPrintingService
{
    private readonly IApplicationDbContext _db;
    private readonly IGroupPatientSlipPdfWriter _writer;
    private readonly ILabPrintTextStore _labTextStore;
    private readonly IPdfPrinterDispatcher _dispatcher;

    public GroupPatientSlipPrintingService(
        IApplicationDbContext db,
        IGroupPatientSlipPdfWriter writer,
        ILabPrintTextStore labTextStore,
        IPdfPrinterDispatcher dispatcher)
    {
        _db = db;
        _writer = writer;
        _labTextStore = labTextStore;
        _dispatcher = dispatcher;
    }

    public async Task<Result> PrintGroupPatientSlipsAsync(
        string batchToken, CancellationToken cancellationToken = default)
    {
        try
        {
            GroupPatientSlipBatchDto? batch = null;
            try
            {
                var envelope = JsonSerializer.Deserialize<GroupPatientSlipPrintEnvelope>(batchToken);
                batch = envelope is null ? null : JsonSerializer.Deserialize<GroupPatientSlipBatchDto>(envelope.BatchJson);
            }
            catch (JsonException)
            {
                batch = null;
            }

            if (batch is null)
            {
                return Result.Failure(Error.Unexpected("بيانات قصاصات المرضى غير صالحة."));
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

            var pdfPath = Path.Combine(Path.GetTempPath(), $"TopLabGroupSlips-{Guid.NewGuid():N}.pdf");
            await _writer.WritePdfAsync(pdfPath, batch, labText.Value!, cancellationToken);
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
            return Result.Failure(Error.Unexpected("تعذر طباعة قصاصات المرضى."));
        }
    }
}
