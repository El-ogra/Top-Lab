using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientSearch.Common;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Settings;

namespace TopLab.Application.Features.PatientSearch.Queries.SearchPatientsGlobal;

public sealed class SearchPatientsGlobalQueryHandler
    : IRequestHandler<SearchPatientsGlobalQuery, Result<IReadOnlyList<PatientSearchHitDto>>>
{
    private readonly IApplicationDbContext _db;

    public SearchPatientsGlobalQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<IReadOnlyList<PatientSearchHitDto>>> Handle(
        SearchPatientsGlobalQuery request, CancellationToken cancellationToken)
    {
        var term = request.Text?.Trim();
        var nameAssist = _db.Set<SystemSettings>().SingleOrDefault()?.EnablePatientNameSearchAssist ?? false;

        IQueryable<Patient> query = _db.Set<Patient>()
            .Where(p => !p.IsDeleted);

        if (!string.IsNullOrWhiteSpace(term))
        {
            var lowerTerm = term.ToLowerInvariant();

            var phonePatientIds = _db.Set<PatientPhoneNumber>()
                .Where(ph => ph.PhoneNumber.Contains(term))
                .Select(ph => ph.PatientId)
                .Distinct()
                .ToList();

            query = query.Where(p =>
                (nameAssist && p.FullName.ToLowerInvariant().Contains(lowerTerm))
                || (p.LabId != null && p.LabId.Value.Trim() == term)
                || (p.NationalId != null && p.NationalId.Trim() == term)
                || phonePatientIds.Contains(p.Id));
        }

        // ------------------------------------------------------------------
        // P-01 filters. Six independent, AND-combined guard clauses (SD-5).
        // Each is shaped like the term guard and is inert when its parameter is null.
        // Each narrows with a conjunction; none widens with a disjunction on a
        // patient column. The Text clause above is neither replaced nor widened.
        // ------------------------------------------------------------------

        // F1a — treating doctor (an individual physician). Separate column, separate guard.
        if (request.TreatingDoctorId is not null)
        {
            var treatingDoctorId = request.TreatingDoctorId.Value;
            query = query.Where(p => p.TreatingDoctorId != null && p.TreatingDoctorId.Value == treatingDoctorId);
        }

        // F1b — referral entity (an institution). SD-6: never merged with the clause above.
        if (request.ReferralEntityId is not null)
        {
            var referralEntityId = request.ReferralEntityId.Value;
            query = query.Where(p => p.ReferralEntityId != null && p.ReferralEntityId.Value == referralEntityId);
        }

        // F2 — patients who have a PatientTest for the chosen test (AS-3).
        if (request.TestId is not null)
        {
            var testId = request.TestId.Value;
            query = query.Where(p =>
                _db.Set<PatientTest>().Any(pt => pt.PatientId.Value == p.Id.Value && pt.TestId.Value == testId));
        }

        // F3
        if (request.Sex is not null)
        {
            var sex = request.Sex.Value;
            query = query.Where(p => p.Sex == sex);
        }

        // F4 — band compares like with like: the stored unit must equal the band's unit,
        // and no conversion between Day / Month / Year is performed (AS-5, BR-04).
        if (request.Age is not null)
        {
            var age = request.Age;
            query = query.Where(p =>
                p.AgeUnit == age.Unit
                && (age.From == null || p.AgeValue >= age.From.Value)
                && (age.To == null || p.AgeValue <= age.To.Value));
        }

        // F5 — bounds RegistrationDateUtc, inclusive on both ends (AS-4).
        if (request.From is not null)
        {
            var fromUtc = request.From.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            query = query.Where(p => p.RegistrationDateUtc >= fromUtc);
        }

        if (request.To is not null)
        {
            var toUtc = request.To.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
            query = query.Where(p => p.RegistrationDateUtc <= toUtc);
        }

        var rows = query
            .OrderByDescending(p => p.RegistrationDateUtc)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var rowIds = rows.Select(p => p.Id).ToList();

        var phoneGroups = _db.Set<PatientPhoneNumber>()
            .Where(ph => rowIds.Contains(ph.PatientId))
            .OrderBy(ph => ph.PatientId.Value)
            .ThenBy(ph => ph.SortOrder)
            .GroupBy(ph => ph.PatientId.Value)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<string>)g.Select(ph => ph.PhoneNumber).ToList());

        var testsByPatient = _db.Set<PatientTest>()
            .Where(pt => rowIds.Contains(pt.PatientId))
            .ToList()
            .GroupBy(pt => pt.PatientId.Value)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<PatientTest>)g.ToList());

        IReadOnlyList<PatientSearchHitDto> items = rows
            .Select(p =>
            {
                var tests = testsByPatient.TryGetValue(p.Id.Value, out var visitTests)
                    ? visitTests
                    : System.Array.Empty<PatientTest>();
                return new PatientSearchHitDto(
                    p.Id.Value,
                    p.LabId?.Value,
                    p.FullName,
                    p.Title,
                    p.Sex.ToString(),
                    p.AgeValue,
                    p.AgeUnit.ToString(),
                    p.NationalId,
                    phoneGroups.TryGetValue(p.Id.Value, out var phones) ? phones : System.Array.Empty<string>(),
                    p.AccountType.ToString(),
                    p.IsVip,
                    VisitRollup.AggregateStatus(_db, p, tests),
                    p.RegistrationDateUtc,
                    tests.Count);
            })
            .ToList();

        return Task.FromResult(Result<IReadOnlyList<PatientSearchHitDto>>.Success(items));
    }
}