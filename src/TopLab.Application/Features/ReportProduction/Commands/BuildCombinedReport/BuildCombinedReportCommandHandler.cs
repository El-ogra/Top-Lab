using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ReportProduction.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Patients;
using TopLab.Domain.Reports;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.ReportProduction.Commands.BuildCombinedReport;

public sealed class BuildCombinedReportCommandHandler
    : IRequestHandler<BuildCombinedReportCommand, Result<CombinedReportDto>>
{
    private readonly IApplicationDbContext _db;

    public BuildCombinedReportCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<CombinedReportDto>> Handle(
        BuildCombinedReportCommand request, CancellationToken cancellationToken)
    {
        var patient = _db.Set<Patient>().FirstOrDefault(p => p.Id.Value == request.PatientId);
        if (patient is null || patient.IsDeleted)
        {
            return Task.FromResult(Result<CombinedReportDto>.Failure(
                Error.NotFound("المريض غير موجود.")));
        }

        var distinctIds = request.OrderedPatientTestIds.Distinct().ToList();
        var pts = _db.Set<PatientTest>()
            .Where(pt => distinctIds.Contains(pt.Id.Value))
            .ToList();

        if (pts.Count != distinctIds.Count)
        {
            return Task.FromResult(Result<CombinedReportDto>.Failure(
                Error.NotFound("التحليل غير موجود")));
        }

        var byId = pts.ToDictionary(pt => pt.Id.Value);

        var selection = new CombinedReportSelection();
        try
        {
            foreach (var id in request.OrderedPatientTestIds)
            {
                selection.Add(id, byId[id].IsReviewed);
            }
        }
        catch (ArgumentException ex)
        {
            return Task.FromResult(Result<CombinedReportDto>.Failure(
                Error.Conflict(DomainFailureTranslator.Translate(ex))));
        }

        var catalog = _db.Set<Test>().ToDictionary(t => t.Id.Value);
        var snapshots = _db.Set<PatientTestReferenceRangeSnapshot>()
            .Where(s => distinctIds.Contains(s.PatientTestId.Value))
            .ToDictionary(s => s.PatientTestId.Value);

        var profileTestIds = pts
            .Where(pt => catalog.TryGetValue(pt.TestId.Value, out var test)
                && test.ResultKind == ResultKind.SpecializedProfile)
            .Select(pt => pt.Id.Value)
            .ToList();

        var profileItems = _db.Set<ProfileResultItem>()
            .Where(i => profileTestIds.Contains(i.PatientTestId.Value))
            .ToList();

        var profileByTest = profileItems
            .GroupBy(i => i.PatientTestId.Value)
            .ToDictionary(g => g.Key, g => g.OrderBy(i => i.Id.Value).ToList());

        var profileItemIds = profileItems.Select(i => i.Id.Value).ToList();
        var profileSnapshots = profileItemIds.Count == 0
            ? new Dictionary<int, ProfileResultItemReferenceRangeSnapshot>()
            : _db.Set<ProfileResultItemReferenceRangeSnapshot>()
                .Where(s => profileItemIds.Contains(s.ProfileResultItemId.Value))
                .ToDictionary(s => s.ProfileResultItemId.Value);

        var analyteCatalog = _db.Set<Analyte>().ToDictionary(a => a.Id);

        var cultureRows = _db.Set<CultureResult>()
            .Where(c => distinctIds.Contains(c.PatientTestId.Value))
            .ToDictionary(c => c.PatientTestId.Value);

        IReadOnlyList<CombinedReportLineDto> lines = selection.OrderedIds.Select(id =>
        {
            var pt = byId[id];
            catalog.TryGetValue(pt.TestId.Value, out var test);
            snapshots.TryGetValue(id, out var snap);

            IReadOnlyList<ProfileReportLineDto> profileLines = new List<ProfileReportLineDto>();
            CultureReportSummaryDto? culture = null;

            if (test is not null && test.ResultKind == ResultKind.SpecializedProfile
                && profileByTest.TryGetValue(id, out var items))
            {
                profileLines = items.Select(item =>
                {
                    profileSnapshots.TryGetValue(item.Id.Value, out var pSnap);
                    analyteCatalog.TryGetValue(item.AnalyteId, out var analyte);
                    return new ProfileReportLineDto(
                        item.Id.Value,
                        item.AnalyteId.Value,
                        analyte?.ReportName ?? $"[{item.AnalyteId.Value}]",
                        item.ResultValue,
                        item.Unit,
                        item.Flag == null ? null : (int)item.Flag.Value,
                        pSnap is null
                            ? null
                            : new FrozenProfileRangeDto(
                                pSnap.AnalyteId.Value,
                                analyte?.ReportName ?? $"[{pSnap.AnalyteId.Value}]",
                                pSnap.Sex?.ToString(),
                                pSnap.AgeUnit.ToString(),
                                pSnap.AgeMin,
                                pSnap.AgeMax,
                                pSnap.MinValue,
                                pSnap.MaxValue,
                                pSnap.LowComment,
                                pSnap.HighComment,
                                pSnap.CapturedAtUtc));
                }).ToList();
            }
            else if (cultureRows.TryGetValue(id, out var cr))
            {
                culture = new CultureReportSummaryDto(
                    cr.Sample, cr.OrganismA, cr.OrganismB, cr.OrganismC,
                    cr.CultureCondition, cr.ColonyCount);
            }

            return new CombinedReportLineDto(
                pt.Id.Value,
                pt.TestId.Value,
                test?.Name ?? string.Empty,
                test?.TestCode ?? string.Empty,
                test == null ? 0 : (int)test.ResultKind,
                pt.ResultValue,
                pt.ResultFlag == null ? null : (int)pt.ResultFlag.Value,
                snap is null ? null : FormatRange(snap),
                profileLines,
                culture);
        }).ToList();

        var dto = new CombinedReportDto(
            patient.Id.Value,
            patient.FullName,
            patient.LabId?.Value,
            lines);

        return Task.FromResult(Result<CombinedReportDto>.Success(dto));
    }

    private static string FormatRange(PatientTestReferenceRangeSnapshot snap)
    {
        return FormattableString.Invariant($"{snap.MinValue} - {snap.MaxValue}");
    }
}