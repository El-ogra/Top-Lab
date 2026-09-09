using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ProfileResults.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;

namespace TopLab.Application.Features.ProfileResults.Commands.SaveProfileResults;

/// <summary>
/// Entry for one profile result (Decision 1): the same save persists the active
/// items and each item's frozen analyte band in
/// <see cref="ProfileResultItemReferenceRangeSnapshot"/>. Only pre-review drafts are
/// replaced; printed/amended rows are never deleted. Comment maps to
/// PatientTest.Notes and entry is recorded via PatientTest.MarkEntered
/// (EnterResult(null) since profiles carry no scalar result value).
/// </summary>
public sealed class SaveProfileResultsCommandHandler : IRequestHandler<SaveProfileResultsCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public SaveProfileResultsCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result> Handle(SaveProfileResultsCommand request, CancellationToken cancellationToken)
    {
        var pt = _db.Set<PatientTest>().FirstOrDefault(t => t.Id.Value == request.PatientTestId);
        if (pt is null)
        {
            return Result.Failure(Error.NotFound("التحليل غير موجود"));
        }

        var patient = _db.Set<Patient>().FirstOrDefault(p => p.Id.Value == pt.PatientId.Value);
        if (patient is null || patient.IsDeleted)
        {
            return Result.Failure(Error.NotFound("المريض غير موجود."));
        }

        if (pt.IsReviewed)
        {
            return Result.Failure(Error.Conflict("النتيجة معتمدة؛ ألغِ الاعتماد أولاً."));
        }

        var profile = ProfileResultReferenceRangeCapture.FindProfile(_db, pt);
        if (profile is null)
        {
            return Result.Failure(Error.NotFound("البروفايل غير موجود."));
        }

        var configured = ProfileResultReferenceRangeCapture.ConfiguredAnalytes(_db, profile);
        var configuredIds = configured.Select(a => a.Id.Value).ToHashSet();
        var incoming = (request.Items ?? Array.Empty<ProfileItemInput>())
            .GroupBy(i => i.AnalyteId)
            .ToDictionary(g => g.Key, g => g.First());

        if (incoming.Count == 0)
        {
            return Result.Failure(Error.Validation("يجب إدخال نتيجة مادة واحدة على الأقل."));
        }

        foreach (var analyteId in incoming.Keys)
        {
            if (!configuredIds.Contains(analyteId))
            {
                return Result.Failure(Error.Validation("المادة التحليلية غير ضمن مكونات البروفايل."));
            }

            if (string.IsNullOrWhiteSpace(incoming[analyteId].ResultValue))
            {
                return Result.Failure(Error.Validation("قيمة النتيجة مطلوبة."));
            }
        }

        var existingItems = _db.Set<ProfileResultItem>()
            .Where(i => i.PatientTestId.Value == pt.Id.Value)
            .ToList();
        var existingSnapshots = _db.Set<ProfileResultItemReferenceRangeSnapshot>()
            .Where(s => existingItems.Select(i => i.Id.Value).Contains(s.ProfileResultItemId.Value))
            .ToList();

        try
        {
            pt.EnterResult(null, null, _currentUser.UserId, _clock.UtcNow, request.Comment);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Conflict(DomainFailureTranslator.Translate(ex)));
        }

        // Wholesale draft replacement: printed/amended rows survive untouched; drafts do not.
        foreach (var existing in existingItems)
        {
            if (existing.IsPrinted)
            {
                continue;
            }

            var snapshotFor = existingSnapshots.FirstOrDefault(s => s.ProfileResultItemId.Equals(existing.Id));
            if (snapshotFor is not null)
            {
                _db.Remove(snapshotFor);
            }

            _db.Remove(existing);
        }

        foreach (var configuredAnalyte in configured)
        {
            if (existingItems.Any(i => i.AnalyteId.Equals(configuredAnalyte.Id) && i.IsPrinted))
            {
                continue;
            }

            if (!incoming.TryGetValue(configuredAnalyte.Id.Value, out var input))
            {
                continue;
            }

            var item = ProfileResultItem.Create(
                ProfileResultItemId.Create(0),
                pt.Id,
                configuredAnalyte.Id,
                input.ResultValue,
                input.Unit,
                input.Flag == null ? null : (ProfileResultFlag)input.Flag.Value);

            _db.Add(item);

            var snapshot = ProfileResultReferenceRangeCapture.Capture(_db, item, patient, _clock);
            if (snapshot is not null)
            {
                _db.Add(snapshot);
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}