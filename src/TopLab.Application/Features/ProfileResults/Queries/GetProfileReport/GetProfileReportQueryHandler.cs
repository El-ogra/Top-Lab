using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ProfileResults.Common;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.ProfileResults.Queries.GetProfileReport;

/// <summary>
/// Report/reprint surface (Decision 1): renders the frozen ranges exclusively from
/// <see cref="ProfileResultItemReferenceRangeSnapshot"/> and never resolves a live
/// analyte range. Verified header, comment and print-state fields are supplied from
/// the persisted PatientTest/item lifecycle.
/// </summary>
public sealed class GetProfileReportQueryHandler
    : IRequestHandler<GetProfileReportQuery, Result<ProfileReportDto>>
{
    private readonly IApplicationDbContext _db;

    public GetProfileReportQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<ProfileReportDto>> Handle(GetProfileReportQuery request, CancellationToken cancellationToken)
    {
        var pt = _db.Set<PatientTest>().FirstOrDefault(t => t.Id.Value == request.PatientTestId);
        if (pt is null)
        {
            return Task.FromResult(Result<ProfileReportDto>.Failure(Error.NotFound("التحليل غير موجود")));
        }

        var patient = _db.Set<Patient>().FirstOrDefault(p => p.Id.Value == pt.PatientId.Value);
        if (patient is null || patient.IsDeleted)
        {
            return Task.FromResult(Result<ProfileReportDto>.Failure(Error.NotFound("المريض غير موجود.")));
        }

        var profile = ProfileResultReferenceRangeCapture.FindProfile(_db, pt);
        if (profile is null)
        {
            return Task.FromResult(Result<ProfileReportDto>.Failure(Error.NotFound("البروفايل غير موجود.")));
        }

        var items = _db.Set<ProfileResultItem>()
            .Where(i => i.PatientTestId.Value == pt.Id.Value)
            .OrderBy(i => i.Id.Value)
            .ToList();
        var itemIds = items.Select(i => i.Id.Value).ToList();
        var snapshots = _db.Set<ProfileResultItemReferenceRangeSnapshot>()
            .Where(s => itemIds.Contains(s.ProfileResultItemId.Value))
            .ToDictionary(s => s.ProfileResultItemId.Value);
        var analyteCatalog = _db.Set<Analyte>().ToDictionary(a => a.Id);

        var lines = items.Select(item =>
        {
            snapshots.TryGetValue(item.Id.Value, out var snap);
            analyteCatalog.TryGetValue(item.AnalyteId, out var analyte);
            return new ProfileReportLineDto(
                item.Id.Value,
                item.AnalyteId.Value,
                analyte?.ReportName ?? $"[{item.AnalyteId.Value}]",
                item.ResultValue,
                item.Unit,
                item.Flag == null ? null : (int)item.Flag.Value,
                snap is null
                    ? null
                    : new FrozenProfileRangeDto(
                        snap.AnalyteId.Value,
                        analyte?.ReportName ?? $"[{snap.AnalyteId.Value}]",
                        snap.Sex?.ToString(),
                        snap.AgeUnit.ToString(),
                        snap.AgeMin,
                        snap.AgeMax,
                        snap.MinValue,
                        snap.MaxValue,
                        snap.LowComment,
                        snap.HighComment,
                        snap.CapturedAtUtc));
        }).ToList();

        var dto = new ProfileReportDto(
            pt.Id.Value,
            patient.Id.Value,
            patient.FullName,
            patient.LabId == null ? null : patient.LabId.Value,
            profile.Name,
            pt.Notes,
            pt.IsReviewed,
            pt.IsPrinted,
            pt.IsDelivered,
            lines);

        return Task.FromResult(Result<ProfileReportDto>.Success(dto));
    }
}