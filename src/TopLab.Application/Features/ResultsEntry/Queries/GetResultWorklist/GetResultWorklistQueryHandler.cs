using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ResultsEntry.Common;
using TopLab.Domain.Patients;
using TopLab.Domain.PatientStatus;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.ResultsEntry.Queries.GetResultWorklist;

public sealed class GetResultWorklistQueryHandler
    : IRequestHandler<GetResultWorklistQuery, Result<IReadOnlyList<ResultWorklistItemDto>>>
{
    private readonly IApplicationDbContext _db;
    private readonly PatientStatusCalculator _calculator = new();

    public GetResultWorklistQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<IReadOnlyList<ResultWorklistItemDto>>> Handle(
        GetResultWorklistQuery request, CancellationToken cancellationToken)
    {
        var day = request.Day ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var patientsById = _db.Set<Patient>()
            .Where(p => !p.IsDeleted && DateOnly.FromDateTime(p.RegistrationDateUtc) == day)
            .ToDictionary(p => p.Id.Value);

        if (patientsById.Count == 0)
        {
            return Task.FromResult(Result<IReadOnlyList<ResultWorklistItemDto>>.Success(Array.Empty<ResultWorklistItemDto>()));
        }

        var catalog = _db.Set<Test>().ToDictionary(t => t.Id.Value);

        var rows = _db.Set<PatientTest>()
            .Where(pt => patientsById.Keys.Contains(pt.PatientId.Value))
            .OrderBy(pt => patientsById[pt.PatientId.Value].RegistrationDateUtc)
            .ThenBy(pt => pt.Id.Value)
            .ToList();

        if (request.HasResult.HasValue)
        {
            rows = rows.Where(pt => request.HasResult.Value
                ? pt.EnteredAtUtc is not null
                : pt.EnteredAtUtc is null).ToList();
        }

        if (request.IsReviewed.HasValue)
        {
            rows = rows.Where(pt => pt.IsReviewed == request.IsReviewed.Value).ToList();
        }

        if (request.TestGroupId.HasValue)
        {
            rows = rows.Where(pt =>
                catalog.TryGetValue(pt.TestId.Value, out var test)
                && test.TestGroupId != null
                && test.TestGroupId.Value == request.TestGroupId.Value).ToList();
        }

        if (request.ResultKind.HasValue)
        {
            rows = rows.Where(pt =>
                catalog.TryGetValue(pt.TestId.Value, out var test)
                && (int)test.ResultKind == request.ResultKind.Value).ToList();
        }

        rows = rows
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        // Aggregate status per listed patient over ALL of that patient's tests + balance.
        var patientIds = rows.Select(pt => pt.PatientId.Value).Distinct().ToList();
        var allTestsByPatient = _db.Set<PatientTest>()
            .Where(pt => patientIds.Contains(pt.PatientId.Value))
            .ToList()
            .GroupBy(pt => pt.PatientId.Value)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<PatientTest>)g.ToList());

        var statusByPatient = new Dictionary<int, int>();
        foreach (var patientId in patientIds)
        {
            var patient = patientsById[patientId];
            var allTests = allTestsByPatient.TryGetValue(patientId, out var list)
                ? list
                : Array.Empty<PatientTest>();
            var balance = BalanceProbe.Balance(_db, patientId);
            var status = _calculator.Calculate(patient, allTests, balance);
            statusByPatient[patientId] = (int)status;
        }

        IReadOnlyList<ResultWorklistItemDto> items = rows.Select(pt =>
        {
            var patient = patientsById[pt.PatientId.Value];
            catalog.TryGetValue(pt.TestId.Value, out var test);
            return new ResultWorklistItemDto(
                pt.Id.Value,
                patient.Id.Value,
                patient.FullName,
                patient.LabId == null ? null : patient.LabId.Value,
                test?.Name ?? string.Empty,
                test?.TestCode ?? string.Empty,
                test == null ? 0 : (int)test.ResultKind,
                test != null && test.IsCultureType,
                pt.IsSampleDrawn,
                pt.IsTakenOutsideLab,
                pt.ResultValue,
                pt.ResultFlag == null ? null : (int)pt.ResultFlag.Value,
                pt.IsReviewed,
                pt.IsPrinted,
                pt.IsDelivered,
                statusByPatient[pt.PatientId.Value],
                patient.RegistrationDateUtc);
        }).ToList();

        return Task.FromResult(Result<IReadOnlyList<ResultWorklistItemDto>>.Success(items));
    }
}
