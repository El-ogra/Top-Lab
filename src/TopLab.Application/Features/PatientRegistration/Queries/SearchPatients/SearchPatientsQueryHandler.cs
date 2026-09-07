using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientRegistration.Common;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;

namespace TopLab.Application.Features.PatientRegistration.Queries.SearchPatients;

public sealed class SearchPatientsQueryHandler
    : IRequestHandler<SearchPatientsQuery, Result<IReadOnlyList<PatientSummaryDto>>>
{
    private readonly IApplicationDbContext _db;

    public SearchPatientsQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<IReadOnlyList<PatientSummaryDto>>> Handle(
        SearchPatientsQuery request, CancellationToken cancellationToken)
    {
        var term = request.SearchTerm?.Trim();

        IQueryable<Patient> query = _db.Set<Patient>()
            .Where(p => !p.IsDeleted);

        if (!string.IsNullOrWhiteSpace(term))
        {
            var matchedIds = _db.Set<PatientPhoneNumber>()
                .Where(ph => ph.PhoneNumber.Contains(term))
                .Select(ph => ph.PatientId)
                .Distinct()
                .ToList();

            query = query.Where(p =>
                p.FullName.Contains(term)
                || (p.NationalId != null && p.NationalId.Contains(term))
                || (p.LabId != null && p.LabId.Value.Contains(term))
                || matchedIds.Contains(p.Id));
        }

        var rows = query
            .OrderByDescending(p => p.RegistrationDateUtc)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var primaryPhones = new Dictionary<PatientId, string>();
        foreach (var p in rows)
        {
            var ph = p.PhoneNumbers
                .OrderBy(ph => ph.SortOrder)
                .Select(ph => (string?)ph.PhoneNumber)
                .FirstOrDefault();
            if (ph is not null)
            {
                primaryPhones[p.Id] = ph;
            }
        }

        IReadOnlyList<PatientSummaryDto> items = rows
            .Select(p => new PatientSummaryDto(
                p.Id.Value,
                p.LabId?.Value,
                p.FullName,
                p.Sex,
                p.AgeValue,
                p.AgeUnit,
                p.NationalId,
                primaryPhones.TryGetValue(p.Id, out var ph) ? ph : null,
                p.RegistrationDateUtc,
                p.AccountType,
                p.IsVip,
                p.IsDeleted))
            .ToList();

        return Task.FromResult(Result<IReadOnlyList<PatientSummaryDto>>.Success(items));
    }
}