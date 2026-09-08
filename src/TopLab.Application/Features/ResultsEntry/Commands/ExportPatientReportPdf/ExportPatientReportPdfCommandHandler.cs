using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ResultsEntry.Common;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.ResultsEntry.Commands.ExportPatientReportPdf;

public sealed class ExportPatientReportPdfCommandHandler : IRequestHandler<ExportPatientReportPdfCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly IPatientReportPdfExporter _exporter;
    private readonly IDateTimeProvider _clock;

    public ExportPatientReportPdfCommandHandler(
        IApplicationDbContext db,
        IPatientReportPdfExporter exporter,
        IDateTimeProvider clock)
    {
        _db = db;
        _exporter = exporter;
        _clock = clock;
    }

    public async Task<Result> Handle(ExportPatientReportPdfCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.AbsolutePath))
        {
            return Result.Failure(Error.Validation("مسار ملف PDF مطلوب."));
        }

        if (!Path.IsPathFullyQualified(request.AbsolutePath))
        {
            return Result.Failure(Error.Validation("مسار ملف PDF يجب أن يكون مطلقًا."));
        }

        if (!request.AbsolutePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure(Error.Validation("مسار الملف يجب أن يكون بامتداد PDF."));
        }

        var patient = _db.Set<Patient>().FirstOrDefault(p => p.Id.Value == request.PatientId);
        if (patient is null || patient.IsDeleted)
        {
            return Result.Failure(Error.NotFound("المريض غير موجود."));
        }

        var tests = _db.Set<PatientTest>()
            .Where(pt => pt.PatientId.Value == patient.Id.Value)
            .OrderBy(pt => pt.Id.Value)
            .ToList();

        if (tests.Count == 0)
        {
            return Result.Failure(Error.Conflict("لا توجد نتائج معتمدة للتصدير."));
        }

        if (tests.Any(pt => pt.EnteredAtUtc is null || !pt.IsReviewed))
        {
            return Result.Failure(Error.Conflict("لا يمكن تصدير تقرير غير معتمد."));
        }

        if (File.Exists(request.AbsolutePath))
        {
            return Result.Failure(Error.Conflict("ملف التصدير موجود مسبقًا."));
        }

        var catalog = _db.Set<Test>().ToDictionary(t => t.Id.Value);
        var snapshots = _db.Set<PatientTestReferenceRangeSnapshot>()
            .Where(s => tests.Select(t => t.Id.Value).Contains(s.PatientTestId.Value))
            .ToDictionary(s => s.PatientTestId.Value);

        var testIds = tests.Select(t => t.Id.Value).ToList();
        var profileByTest = _db.Set<ProfileResultItem>()
            .Where(p => testIds.Contains(p.PatientTestId.Value))
            .ToList()
            .GroupBy(p => p.PatientTestId.Value)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<ProfileResultItem>)g.ToList());

        var cultureByTest = _db.Set<CultureResult>()
            .Where(c => testIds.Contains(c.PatientTestId.Value))
            .ToDictionary(c => c.PatientTestId.Value);

        var lines = new List<PatientReportPdfLine>();
        foreach (var pt in tests)
        {
            catalog.TryGetValue(pt.TestId.Value, out var test);
            snapshots.TryGetValue(pt.Id.Value, out var snapshot);

            FrozenRangeDto? frozen = snapshot == null
                ? null
                : new FrozenRangeDto(
                    snapshot.TestId,
                    snapshot.Sex?.ToString(),
                    snapshot.AgeUnit.ToString(),
                    snapshot.AgeMin,
                    snapshot.AgeMax,
                    snapshot.MinValue,
                    snapshot.MaxValue,
                    snapshot.LowComment,
                    snapshot.HighComment,
                    snapshot.CapturedAtUtc);

            var profileTexts = new List<string>();
            if (profileByTest.TryGetValue(pt.Id.Value, out var profileItems))
            {
                foreach (var item in profileItems)
                {
                    profileTexts.Add($"{item.AnalyteName}: {item.ResultValue}{(string.IsNullOrWhiteSpace(item.Unit) ? string.Empty : " " + item.Unit)}");
                }
            }

            string? cultureSummary = null;
            if (cultureByTest.TryGetValue(pt.Id.Value, out var culture))
            {
                var parts = new List<string>();
                if (!string.IsNullOrWhiteSpace(culture.Sample))
                {
                    parts.Add($"Sample: {culture.Sample}");
                }

                foreach (var org in new[] { culture.OrganismA, culture.OrganismB, culture.OrganismC })
                {
                    if (!string.IsNullOrWhiteSpace(org))
                    {
                        parts.Add(org);
                    }
                }

                if (!string.IsNullOrWhiteSpace(culture.ColonyCount))
                {
                    parts.Add($"Count: {culture.ColonyCount}");
                }

                cultureSummary = parts.Count > 0 ? string.Join("; ", parts) : null;
            }

            lines.Add(new PatientReportPdfLine(
                pt.Id.Value,
                test?.Name ?? string.Empty,
                test?.TestCode ?? string.Empty,
                pt.ResultValue,
                pt.ResultFlag == null ? null : (int)pt.ResultFlag.Value,
                pt.Notes,
                frozen,
                profileTexts,
                cultureSummary));
        }

        var data = new PatientReportPdfData(
            patient.Id.Value,
            patient.FullName,
            patient.LabId == null ? null : patient.LabId.Value,
            lines);

        try
        {
            await _exporter.ExportAsync(request.AbsolutePath, data, cancellationToken);
        }
        catch (Exception)
        {
            return Result.Failure(Error.Unexpected("فشل تصدير تقرير PDF."));
        }

        var exportedAt = _clock.UtcNow;
        foreach (var pt in tests)
        {
            pt.MarkExported(exportedAt);
        }

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
