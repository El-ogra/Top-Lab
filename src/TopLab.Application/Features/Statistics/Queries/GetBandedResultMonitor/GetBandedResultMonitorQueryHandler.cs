using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ResultsEntry.Common;
using TopLab.Application.Features.Statistics.Common;
using TopLab.Domain.ExternalEntities;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.Statistics.Queries.GetBandedResultMonitor;

/// <summary>
/// R-F05 banded result monitor. Reads only columns that already exist; no migration.
/// </summary>
public sealed class GetBandedResultMonitorQueryHandler
    : IRequestHandler<GetBandedResultMonitorQuery, Result<BandedResultMonitorDto>>
{
    private const string NoReferralBucketLabel = "بدون جهة إحالة";

    private const string DeliveredStatusLabel = "تم التسليم";
    private const string PrintedStatusLabel = "تمت الطباعة";
    private const string ReviewedStatusLabel = "معتمد";
    private const string EnteredOnlyStatusLabel = "غير معتمد";

    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _dateTime;

    public GetBandedResultMonitorQueryHandler(IApplicationDbContext db, IDateTimeProvider dateTime)
    {
        _db = db;
        _dateTime = dateTime;
    }

    public Task<Result<BandedResultMonitorDto>> Handle(
        GetBandedResultMonitorQuery request,
        CancellationToken cancellationToken)
    {
        var test = _db.Set<Test>()
            .FirstOrDefault(t => t.Id.Value == request.TestId);

        if (test is null)
        {
            return Task.FromResult(Result<BandedResultMonitorDto>.Failure(
                Error.NotFound("التحليل غير موجود.", "NotFound")));
        }

        var today = DateOnly.FromDateTime(_dateTime.UtcNow);
        var from = request.From ?? today;
        var to = request.To ?? today;
        var fromStart = from.ToDateTime(TimeOnly.MinValue);
        var toEndExclusive = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

        // BR-F05-3: the date column is the result-entry instant, so unentered rows drop out
        // by construction (EnteredAtUtc is null).
        var patientTests = _db.Set<PatientTest>()
            .Where(pt => pt.TestId.Value == request.TestId
                && pt.EnteredAtUtc != null
                && pt.EnteredAtUtc >= fromStart
                && pt.EnteredAtUtc < toEndExclusive)
            .ToList();

        // BR-F05-5: the identical parse rule the flag computer uses, reused verbatim.
        var inBand = patientTests
            .Where(pt => ResultFlagComputer.TryParse(pt.ResultValue, out var value)
                && value >= request.MinValue
                && value <= request.MaxValue)
            .ToList();

        var patientIds = inBand.Select(pt => pt.PatientId.Value).Distinct().ToList();

        // BR-F05-9: soft-deleted patients are excluded, exactly like the other statistics queries.
        var patients = _db.Set<Patient>()
            .Where(p => patientIds.Contains(p.Id.Value) && !p.IsDeleted)
            .ToDictionary(p => p.Id.Value);

        var referralIds = patients.Values
            .Where(p => p.ReferralEntityId is not null)
            .Select(p => p.ReferralEntityId!.Value)
            .Distinct()
            .ToList();

        var referralNames = _db.Set<ExternalEntity>()
            .Where(e => referralIds.Contains(e.Id.Value))
            .ToDictionary(e => e.Id.Value, e => e.Name);

        // BR-F05-11: EnteredAtUtc ascending, then PatientTestId ascending (deterministic).
        var rows = inBand
            .Where(pt => patients.ContainsKey(pt.PatientId.Value))
            .OrderBy(pt => pt.EnteredAtUtc!.Value)
            .ThenBy(pt => pt.Id.Value)
            .Select(pt =>
            {
                var patient = patients[pt.PatientId.Value];
                return new BandedResultRowDto(
                    pt.Id.Value,
                    pt.EnteredAtUtc!.Value,
                    patient.Id.Value,
                    patient.FullName,
                    patient.Sex.ToString(),
                    patient.AgeValue,
                    patient.AgeUnit.ToString(),
                    ReferralName(patient, referralNames),
                    test.Name,
                    pt.ResultValue ?? string.Empty,
                    StatusLabel(pt));
            })
            .ToList();

        var dto = new BandedResultMonitorDto(
            from,
            to,
            test.Id.Value,
            test.Name,
            request.MinValue,
            request.MaxValue,
            rows,
            rows.Count);

        return Task.FromResult(Result<BandedResultMonitorDto>.Success(dto));
    }

    private static string ReferralName(Patient patient, IReadOnlyDictionary<int, string> referralNames)
    {
        if (patient.ReferralEntityId is null)
        {
            return NoReferralBucketLabel;
        }

        return referralNames.TryGetValue(patient.ReferralEntityId.Value, out var name)
            ? name
            : patient.ReferralEntityId.Value.ToString();
    }

    /// <summary>BR-F05-10: lifecycle from existing columns only; no new persisted state.</summary>
    private static string StatusLabel(PatientTest pt)
    {
        if (pt.IsDelivered)
        {
            return DeliveredStatusLabel;
        }

        if (pt.IsPrinted)
        {
            return PrintedStatusLabel;
        }

        if (pt.IsReviewed)
        {
            return ReviewedStatusLabel;
        }

        return EnteredOnlyStatusLabel;
    }
}