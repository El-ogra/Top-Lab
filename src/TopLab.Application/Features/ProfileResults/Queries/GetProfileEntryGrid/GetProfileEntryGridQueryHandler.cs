using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ProfileResults.Common;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.ProfileResults.Queries.GetProfileEntryGrid;

public sealed class GetProfileEntryGridQueryHandler
    : IRequestHandler<GetProfileEntryGridQuery, Result<ProfileEntryGridDto>>
{
    private readonly IApplicationDbContext _db;

    public GetProfileEntryGridQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<ProfileEntryGridDto>> Handle(GetProfileEntryGridQuery request, CancellationToken cancellationToken)
    {
        var pt = _db.Set<PatientTest>().FirstOrDefault(t => t.Id.Value == request.PatientTestId);
        if (pt is null)
        {
            return Task.FromResult(Result<ProfileEntryGridDto>.Failure(Error.NotFound("التحليل غير موجود")));
        }

        var patient = _db.Set<Patient>().FirstOrDefault(p => p.Id.Value == pt.PatientId.Value);
        if (patient is null || patient.IsDeleted)
        {
            return Task.FromResult(Result<ProfileEntryGridDto>.Failure(Error.NotFound("المريض غير موجود.")));
        }

        var profile = ProfileResultReferenceRangeCapture.FindProfile(_db, pt);
        if (profile is null)
        {
            return Task.FromResult(Result<ProfileEntryGridDto>.Failure(Error.NotFound("البروفايل غير موجود.")));
        }

        var items = _db.Set<ProfileResultItem>()
            .Where(i => i.PatientTestId.Value == pt.Id.Value)
            .ToDictionary(i => i.AnalyteId.Value);
        var itemIds = items.Values.Select(i => i.Id.Value).ToList();
        var snapshots = _db.Set<ProfileResultItemReferenceRangeSnapshot>()
            .Where(s => itemIds.Contains(s.ProfileResultItemId.Value))
            .ToDictionary(s => s.ProfileResultItemId.Value);
        var configured = ProfileResultReferenceRangeCapture.ConfiguredAnalytes(_db, profile);

        var dtos = configured.Select(analyte =>
        {
            items.TryGetValue(analyte.Id.Value, out var item);
            return new ProfileEntryItemDto(
                item?.Id.Value ?? 0,
                analyte.Id.Value,
                analyte.ReportName,
                item?.ResultValue,
                item?.Unit,
                item?.Flag == null ? null : (int)item.Flag.Value,
                item?.IsVerified ?? false,
                item?.IsPrinted ?? false,
                item != null && snapshots.TryGetValue(item.Id.Value, out var snap)
                    ? ToFrozen(snap, analyte)
                    : null);
        }).ToList();

        var dto = new ProfileEntryGridDto(
            pt.Id.Value,
            patient.Id.Value,
            patient.FullName,
            profile.Name,
            dtos);

        return Task.FromResult(Result<ProfileEntryGridDto>.Success(dto));
    }

    private static FrozenProfileRangeDto ToFrozen(ProfileResultItemReferenceRangeSnapshot snap, Analyte analyte)
    {
        return new FrozenProfileRangeDto(
            snap.AnalyteId.Value,
            analyte.ReportName,
            snap.Sex?.ToString(),
            snap.AgeUnit.ToString(),
            snap.AgeMin,
            snap.AgeMax,
            snap.MinValue,
            snap.MaxValue,
            snap.LowComment,
            snap.HighComment,
            snap.CapturedAtUtc);
    }
}