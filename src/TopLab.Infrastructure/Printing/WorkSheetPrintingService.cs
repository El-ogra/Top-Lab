using System.Text.Json;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.WorkSheets.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Settings;

namespace TopLab.Infrastructure.Printing;

/// <summary>
/// PDF-first implementation of <c>IWorkSheetPrintingService</c> (S-01 slice S4).
/// Deserializes the visit-worksheet token, reads the workstation-local lab
/// header text via the existing <c>ILabPrintTextStore</c> port, renders an
/// Arabic RTL bench sheet via <see cref="IWorkSheetPdfWriter"/>, and dispatches
/// it to the printer routed through <c>PrinterAssignment</c> (OutputType =
/// Reports — worksheets are lab-internal documents).
/// Every failure surfaces as <c>Error.Unexpected</c> — this service never throws.
/// </summary>
public sealed class WorkSheetPrintingService : IWorkSheetPrintingService
{
    private readonly IApplicationDbContext _db;
    private readonly IWorkSheetPdfWriter _writer;
    private readonly ILabPrintTextStore _labTextStore;
    private readonly IPdfPrinterDispatcher _dispatcher;

    public WorkSheetPrintingService(
        IApplicationDbContext db,
        IWorkSheetPdfWriter writer,
        ILabPrintTextStore labTextStore,
        IPdfPrinterDispatcher dispatcher)
    {
        _db = db;
        _writer = writer;
        _labTextStore = labTextStore;
        _dispatcher = dispatcher;
    }

    public async Task<Result> PrintWorkSheetAsync(string workSheetToken, CancellationToken cancellationToken = default)
    {
        try
        {
            VisitWorkSheetDto? sheet = null;
            try
            {
                var envelope = JsonSerializer.Deserialize<WorkSheetPrintEnvelope>(workSheetToken);
                sheet = envelope is null ? null : JsonSerializer.Deserialize<VisitWorkSheetDto>(envelope.WorkSheetJson);
            }
            catch (JsonException)
            {
                sheet = null;
            }

            if (sheet is null)
            {
                return Result.Failure(Error.Unexpected("بيانات ورقة العمل غير صالحة."));
            }

            var assignment = _db.Set<PrinterAssignment>().FirstOrDefault(a => a.OutputType == PrinterOutputType.Reports);
            if (assignment is null)
            {
                return Result.Failure(Error.Unexpected("لم يتم تعيين طابعة لتقارير المختبر."));
            }

            var labText = await _labTextStore.GetAsync(LabPrintTextScope.Report, cancellationToken);
            if (!labText.IsSuccess)
            {
                return Result.Failure(labText.Error!);
            }

            var pdfPath = Path.Combine(Path.GetTempPath(), $"TopLabWorkSheet-{Guid.NewGuid():N}.pdf");
            await _writer.WritePdfAsync(pdfPath, sheet, labText.Value!, cancellationToken);
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
            return Result.Failure(Error.Unexpected("تعذر طباعة ورقة العمل."));
        }
    }
}
