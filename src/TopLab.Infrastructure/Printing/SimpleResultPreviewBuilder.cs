using System.Text.Json;
using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ReportProduction.Commands.BuildCombinedReport;
using TopLab.Application.Features.ReportProduction.Common;
using TopLab.Application.Features.ResultsEntry.Common;
using TopLab.Domain.Results;
using TopLab.Domain.Settings;

namespace TopLab.Infrastructure.Printing;

/// <summary>
/// Builds the preview PDF for ONE simple test's report (canonical reference 1.22.12.3).
/// <para>
/// It reuses the exact same report content the print path produces: the combined-report
/// envelope for the single test id, rendered by <see cref="ReportDocument"/>. Nothing is
/// sent to a printer and nothing is marked as printed.
/// </para>
/// </summary>
public sealed class SimpleResultPreviewBuilder : ISimpleResultPreviewBuilder
{
    private readonly ISender _sender;
    private readonly IApplicationDbContext _db;

    public SimpleResultPreviewBuilder(ISender sender, IApplicationDbContext db)
    {
        _sender = sender;
        _db = db;
    }

    public async Task<Result<string>> BuildAsync(int patientTestId, CancellationToken cancellationToken = default)
    {
        if (patientTestId <= 0)
        {
            return Result<string>.Failure(Error.Validation("معرّف التحليل غير صالح."));
        }

        var patientId = ResolvePatientId(patientTestId);
        if (!patientId.IsSuccess)
        {
            return Result<string>.Failure(patientId.Error!);
        }

        var built = await _sender.Send(
            new BuildCombinedReportCommand(patientId.Value, new[] { patientTestId }), cancellationToken);
        if (!built.IsSuccess)
        {
            return Result<string>.Failure(built.Error!);
        }

        var reportSettings = _db.Set<ReportSettings>().FirstOrDefault(s => s.Id == 1);
        var systemSettings = _db.Set<SystemSettings>().FirstOrDefault(s => s.Id == 1);
        if (reportSettings is null || systemSettings is null)
        {
            return Result<string>.Failure(Error.Unexpected("تعذّر قراءة إعدادات الطباعة."));
        }

        var envelope = new ReportPrintEnvelope(
            ReportPrintEnvelope.Combined,
            JsonSerializer.Serialize(built.Value));

        var content = ReportContentBuilder.FromEnvelope(
            envelope, reportSettings, systemSettings,
            itemTemplates: _db.Set<ReportItemTemplate>().ToList());

        var document = new ReportDocument(content, reportSettings);
        var path = Path.Combine(Path.GetTempPath(), $"TopLab-SimpleResultPreview-{Guid.NewGuid():N}.pdf");
        document.WriteToFile(path);

        return Result<string>.Success(path);
    }

    /// <summary>
    /// Resolves the owning patient through the results table, so this builder never duplicates
    /// an ownership rule and never needs a report query of its own.
    /// </summary>
    private Result<int> ResolvePatientId(int patientTestId)
    {
        var patientId = _db.Set<PatientTest>()
            .Where(pt => pt.Id.Value == patientTestId)
            .Select(pt => pt.PatientId.Value)
            .FirstOrDefault();

        return patientId <= 0
            ? Result<int>.Failure(Error.NotFound("التحليل غير موجود."))
            : Result<int>.Success(patientId);
    }
}
