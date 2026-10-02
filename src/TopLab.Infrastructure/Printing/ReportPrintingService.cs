using System.Text.Json;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ReportProduction.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Settings;

namespace TopLab.Infrastructure.Printing;

/// <summary>
/// PDF-first implementation of <c>IReportPrintingService</c> (D21 / OD-07-A).
/// Deserializes the report token, reads <c>ReportSettings</c>/<c>SystemSettings</c>
/// at print time (single rows, PK=1), renders a minimal PDF via
/// <see cref="IReportPdfWriter"/>, and dispatches it to the printer routed through
/// <c>PrinterAssignment</c> (OutputType = Reports). Every failure surfaces as
/// <c>Error.Unexpected</c> — this service never throws (Test Strategy §3).
/// </summary>
public sealed class ReportPrintingService : IReportPrintingService
{
    private readonly IApplicationDbContext _db;
    private readonly IReportPdfWriter _writer;
    private readonly IPdfPrinterDispatcher _dispatcher;
    private readonly IPrintingDiagnostics _diagnostics;

    public ReportPrintingService(
        IApplicationDbContext db,
        IReportPdfWriter writer,
        IPdfPrinterDispatcher dispatcher,
        IPrintingDiagnostics diagnostics)
    {
        _db = db;
        _writer = writer;
        _dispatcher = dispatcher;
        _diagnostics = diagnostics;
    }

    public async Task<Result> PrintReportAsync(string reportToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var envelope = JsonSerializer.Deserialize<ReportPrintEnvelope>(reportToken);
            if (envelope is null)
            {
                return Result.Failure(Error.Unexpected("بيانات التقرير غير صالحة."));
            }

            var reportSettings = _db.Set<ReportSettings>().SingleOrDefault(s => s.Id == 1);
            if (reportSettings is null)
            {
                return Result.Failure(Error.Unexpected("سجل إعدادات التقرير مفقود."));
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

            // W-02 S15 (WP-29): the owned temp folder, cleaned by TempPdfCleanupService.
            var printDir = Path.Combine(Path.GetTempPath(), "TopLab", "Print");
            Directory.CreateDirectory(printDir);
            var pdfPath = Path.Combine(printDir, $"TopLabPrint-{Guid.NewGuid():N}.pdf");
            await _writer.WritePdfAsync(pdfPath, envelope, reportSettings, systemSettings, cancellationToken);
            await _dispatcher.DispatchAsync(pdfPath, assignment.PrinterName, cancellationToken);

            // The temp PDF is intentionally left in the OS temp directory.
            return Result.Success();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            // W-02 S13 (WP-29): the swallow is now observed — behaviour unchanged.
            _diagnostics.ReportSwallowed("ReportPrintingService", nameof(PrintReportAsync), ex);
            return Result.Failure(Error.Unexpected("تعذر طباعة التقرير."));
        }
    }
}