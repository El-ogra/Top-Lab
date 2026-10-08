using System.Text.Json;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientEnvelope.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Settings;

namespace TopLab.Infrastructure.Printing;

/// <summary>
/// PDF-first implementation of <c>IEnvelopePrintingService</c> (Phase 1,
/// REF-066). Deserializes the envelope token, reads <c>EnvelopeSettings</c> +
/// <c>EnvelopePrintItemPositions</c> at print time (live read, no caching),
/// reads the workstation-local lab header text via the existing
/// <c>ILabPrintTextStore</c> port (scope Envelope), renders an Arabic RTL
/// envelope via <see cref="IEnvelopePdfWriter"/>, and dispatches it to the
/// printer routed through <c>PrinterAssignment</c> (OutputType = Envelope).
/// Every failure surfaces as <c>Error.Unexpected</c> — this service never throws.
/// </summary>
public sealed class EnvelopePrintingService : IEnvelopePrintingService
{
    private readonly IApplicationDbContext _db;
    private readonly IEnvelopePdfWriter _writer;
    private readonly ILabPrintTextStore _labTextStore;
    private readonly IPdfPrinterDispatcher _dispatcher;

    public EnvelopePrintingService(
        IApplicationDbContext db,
        IEnvelopePdfWriter writer,
        ILabPrintTextStore labTextStore,
        IPdfPrinterDispatcher dispatcher)
    {
        _db = db;
        _writer = writer;
        _labTextStore = labTextStore;
        _dispatcher = dispatcher;
    }

    public async Task<Result> PrintEnvelopeAsync(string envelopeToken, CancellationToken cancellationToken = default)
    {
        try
        {
            EnvelopeDto? envelope = null;
            try
            {
                var token = JsonSerializer.Deserialize<EnvelopePrintEnvelope>(envelopeToken);
                envelope = token is null ? null : JsonSerializer.Deserialize<EnvelopeDto>(token.EnvelopeJson);
            }
            catch (JsonException)
            {
                envelope = null;
            }

            if (envelope is null)
            {
                return Result.Failure(Error.Unexpected("بيانات المظروف غير صالحة."));
            }

            var settings = _db.Set<EnvelopeSettings>().SingleOrDefault(s => s.Id == 1);
            if (settings is null)
            {
                return Result.Failure(Error.Unexpected("سجل إعدادات المظروف مفقود."));
            }

            var positions = _db.Set<EnvelopePrintItemPosition>().ToList();

            var assignment = _db.Set<PrinterAssignment>().FirstOrDefault(a => a.OutputType == PrinterOutputType.Envelope);
            if (assignment is null)
            {
                return Result.Failure(Error.Unexpected("لم يتم تعيين طابعة للمظاريف."));
            }

            var labText = await _labTextStore.GetAsync(LabPrintTextScope.Envelope, cancellationToken);
            if (!labText.IsSuccess)
            {
                return Result.Failure(labText.Error!);
            }

            var pdfPath = Path.Combine(Path.GetTempPath(), $"TopLabEnvelope-{Guid.NewGuid():N}.pdf");
            await _writer.WritePdfAsync(pdfPath, envelope, settings, positions, labText.Value!, cancellationToken);
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
            return Result.Failure(Error.Unexpected("تعذر طباعة المظروف."));
        }
    }
}
