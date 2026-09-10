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