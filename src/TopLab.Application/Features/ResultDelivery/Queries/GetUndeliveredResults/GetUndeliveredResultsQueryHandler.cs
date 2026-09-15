using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ResultDelivery.Common;
using TopLab.Domain.Billing;
using TopLab.Domain.Patients;
using TopLab.Domain.PatientStatus;
using TopLab.Domain.Results;

namespace TopLab.Application.Features.ResultDelivery.Queries.GetUndeliveredResults;

public sealed class GetUndeliveredResultsQueryHandler
    : IRequestHandler<GetUndeliveredResultsQuery, Result<IReadOnlyList<UndeliveredPatientRowDto>>>
{
    private readonly IApplicationDbContext _db;
    private readonly PatientStatusCalculator _calculator = new();

    public GetUndeliveredResultsQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<IReadOnlyList<UndeliveredPatientRowDto>>> Handle(
        GetUndeliveredResultsQuery request, CancellationToken cancellationToken)
    {
        var (from, to) = ResultDeliveryPeriod.Resolve(request.From, request.To);

        var patients = _db.Set<Patient>()
            .Where(p => !p.IsDeleted
                && DateOnly.FromDateTime(p.RegistrationDateUtc) >= from
                && DateOnly.FromDateTime(p.RegistrationDateUtc) <= to)
            .OrderBy(p => p.RegistrationDateUtc)
            .ThenBy(p => p.Id.Value)
            .ToList();

        if (patients.Count == 0)
        {
            return Task.FromResult(Result<IReadOnlyList<UndeliveredPatientRowDto>>.Success(
                Array.Empty<UndeliveredPatientRowDto>()));
        }

        var patientIds = patients.Select(p => p.Id.Value).ToHashSet();

        var undeliveredCounts = _db.Set<PatientTest>()
            .Where(pt => patientIds.Contains(pt.PatientId.Value) && !pt.IsDelivered)
            .GroupBy(pt => pt.PatientId.Value)
            .Select(g => new { PatientId = g.Key, Count = g.Count() })
            .ToList()
            .ToDictionary(x => x.PatientId, x => x.Count);

        var listed = patients
            .Where(p => undeliveredCounts.ContainsKey(p.Id.Value))
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var listedIds = listed.Select(p => p.Id.Value).ToHashSet();

        var allTestsByPatient = _db.Set<PatientTest>()
            .Where(pt => listedIds.Contains(pt.PatientId.Value))
            .ToList()
            .GroupBy(pt => pt.PatientId.Value)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<PatientTest>)g.ToList());

        IReadOnlyList<UndeliveredPatientRowDto> rows = listed.Select(patient =>
        {
            var allTests = allTestsByPatient.TryGetValue(patient.Id.Value, out var list)
                ? list
                : Array.Empty<PatientTest>();
            var balance = Balance(patient.Id.Value);
            var status = _calculator.Calculate(patient, allTests, balance);
            return new UndeliveredPatientRowDto(
                patient.Id.Value,
                patient.FullName,
                patient.LabId == null ? null : patient.LabId.Value,
                patient.RegistrationDateUtc,
                (int)status,
                undeliveredCounts[patient.Id.Value]);
        }).ToList();

        return Task.FromResult(Result<IReadOnlyList<UndeliveredPatientRowDto>>.Success(rows));
    }

    private decimal Balance(int patientId)
    {
        var prices = _db.Set<PatientTest>()
            .Where(pt => pt.PatientId.Value == patientId)
            .Select(pt => pt.PriceAtOrderTime)
            .ToList();

        var operations = _db.Set<PaymentOperation>()
            .Where(o => o.PatientId.Value == patientId)
            .ToList();

        return PatientAccountCalculator.Balance(prices, operations);
    }
}
