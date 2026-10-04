using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.Statistics.Common;
using TopLab.Domain.Billing;
using TopLab.Domain.ExternalEntities;
using TopLab.Domain.Patients;

namespace TopLab.Application.Features.Statistics.Queries.GetPatientCountStatistics;

public sealed class GetPatientCountStatisticsQueryHandler
    : IRequestHandler<GetPatientCountStatisticsQuery, Result<PatientCountStatisticsDto>>
{
    private const string NoReferralBucketKey = "";
    private const string NoReferralBucketLabel = "بدون جهة إحالة";

    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _dateTime;

    public GetPatientCountStatisticsQueryHandler(IApplicationDbContext db, IDateTimeProvider dateTime)
    {
        _db = db;
        _dateTime = dateTime;
    }

    public Task<Result<PatientCountStatisticsDto>> Handle(
        GetPatientCountStatisticsQuery request,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(_dateTime.UtcNow);
        var from = request.From ?? today;
        var to = request.To ?? today;
        var fromStart = from.ToDateTime(TimeOnly.MinValue);
        var toEndExclusive = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var patients = _db.Set<Patient>()
            .Where(p => !p.IsDeleted
                && p.RegistrationDateUtc >= fromStart
                && p.RegistrationDateUtc < toEndExclusive)
            .ToList();

        var sexCounts = request.BySex
            ? patients
                .GroupBy(p => p.Sex)
                .OrderBy(g => g.Key)
                .Select(g => new ClassificationCountDto(
                    ((int)g.Key).ToString(),
                    g.Key.ToString(),
                    g.Count()))
                .ToList()
            : [];

        IReadOnlyList<ClassificationCountDto> referralCounts = [];
        if (request.ByReferralEntity)
        {
            var byReferral = patients
                .GroupBy(p => p.ReferralEntityId)
                .ToList();

            var referralIds = byReferral
                .Where(g => g.Key is not null)
                .Select(g => g.Key!.Value)
                .Distinct()
                .ToList();

            var names = _db.Set<ExternalEntity>()
                .Where(e => referralIds.Contains(e.Id.Value))
                .ToDictionary(e => e.Id.Value, e => e.Name);

            referralCounts = byReferral
                .OrderBy(g => g.Key is null ? 0 : 1)
                .ThenBy(g => g.Key?.Value ?? 0)
                .Select(g =>
                {
                    if (g.Key is null)
                    {
                        return new ClassificationCountDto(NoReferralBucketKey, NoReferralBucketLabel, g.Count());
                    }

                    var key = g.Key.Value.ToString();
                    var display = names.TryGetValue(g.Key.Value, out var name)
                        ? name
                        : key;
                    return new ClassificationCountDto(key, display, g.Count());
                })
                .ToList();
        }

        var accountTypeCounts = request.ByAccountType
            ? patients
                .GroupBy(p => p.AccountType)
                .OrderBy(g => g.Key)
                .Select(g => new ClassificationCountDto(
                    ((int)g.Key).ToString(),
                    g.Key.ToString(),
                    g.Count()))
                .ToList()
            : [];

        var monthlyCounts = request.GroupByMonth
            ? patients
                .GroupBy(p => (Year: p.RegistrationDateUtc.Year, Month: p.RegistrationDateUtc.Month))
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month)
                .Select(g => new MonthlyCountDto(g.Key.Year, g.Key.Month, g.Count()))
                .ToList()
            : [];

        // BR-F01-2/3/4: the day-of-month ORDINAL across the whole period, not a
        // (Year, Month, Day) tuple. Zero-count days are not emitted, and the list is
        // empty unless the flag is true — the same convention as MonthlyCounts above.
        var dayOfMonthCounts = request.GroupByDayOfMonth
            ? patients
                .GroupBy(p => p.RegistrationDateUtc.Day)
                .OrderBy(g => g.Key)
                .Select(g => new DayOfMonthCountDto(g.Key, g.Count()))
                .ToList()
            : [];

        var monthlySexCounts = request.GroupByMonth && request.BySex
            ? patients
                .GroupBy(p => (Year: p.RegistrationDateUtc.Year, Month: p.RegistrationDateUtc.Month, p.Sex))
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month)
                .ThenBy(g => g.Key.Sex)
                .Select(g => new MonthlyClassificationCountDto(
                    g.Key.Year,
                    g.Key.Month,
                    ((int)g.Key.Sex).ToString(),
                    g.Key.Sex.ToString(),
                    g.Count()))
                .ToList()
            : [];

        // BR-F01-7/8/9 (OD-2 = Option A): the money RECEIVED in the period. The candidate
        // set is filtered on OperationAtUtc ALONE — it is deliberately NOT routed through
        // the period's `patients` list, so a payment collected in March for a December
        // visit counts in March. Payments of soft-deleted patients are INCLUDED: the
        // repository's own mechanism for "this money is not real" is IsVoided, which
        // TotalPaid already filters, and excluding on the patient row's current soft-delete
        // flag would reintroduce exactly the patient-state coupling OD-2 removed.
        PeriodMoneyDto? money = null;
        if (request.IncludeMoneyRow)
        {
            var periodOperations = _db.Set<PaymentOperation>()
                .Where(o => o.OperationAtUtc >= fromStart && o.OperationAtUtc < toEndExclusive)
                .ToList();

            // Reuse the settled formula verbatim — never a third rule (BR-F01-7, H-7).
            var qualifying = periodOperations.Where(o => !o.IsVoided && !o.IsExtraCharge).ToList();
            money = new PeriodMoneyDto(
                PatientAccountCalculator.TotalPaid(periodOperations),
                qualifying.Count);
        }

        var dto = new PatientCountStatisticsDto(
            from,
            to,
            patients.Count,
            sexCounts,
            referralCounts,
            accountTypeCounts,
            monthlyCounts,
            monthlySexCounts,
            dayOfMonthCounts,
            money);

        return Task.FromResult(Result<PatientCountStatisticsDto>.Success(dto));
    }
}
