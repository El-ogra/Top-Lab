using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientRegistration.Common;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.PatientRegistration.Queries.GetPatientVisitHistory;

public sealed class GetPatientVisitHistoryQueryHandler
    : IRequestHandler<GetPatientVisitHistoryQuery, Result<IReadOnlyList<VisitHistoryDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetPatientVisitHistoryQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<IReadOnlyList<VisitHistoryDto>>> Handle(
        GetPatientVisitHistoryQuery request, CancellationToken cancellationToken)
    {
        var patient = _db.Set<Patient>()
            .FirstOrDefault(p => p.Id.Value == request.PatientId);

        if (patient is null || patient.IsDeleted)
        {
            return Task.FromResult(Result<IReadOnlyList<VisitHistoryDto>>.Failure(
                Error.NotFound("المريض غير موجود.")));
        }

        var siblingPatients = string.IsNullOrWhiteSpace(patient.LabId?.Value)
            ? new List<Patient> { patient }
            : _db.Set<Patient>()
                .Where(p => p.LabId != null && p.LabId.Value == patient.LabId!.Value)
                .ToList();

        var patientIds = siblingPatients.Select(p => p.Id).ToList();

        var patientTests = _db.Set<PatientTest>()
            .Where(pt => patientIds.Contains(pt.PatientId))
            .ToList();

        var testIds = patientTests.Select(pt => pt.TestId).Distinct().ToList();
        var tests = _db.Set<Test>()
            .Where(t => testIds.Contains(t.Id))
            .ToDictionary(t => t.Id, t => t);

        var visits = siblingPatients
            .OrderBy(p => p.RegistrationDateUtc)
            .Select(p => new VisitHistoryDto(
                p.Id.Value,
                p.LabId?.Value,
                p.RegistrationDateUtc,
                patientTests
                    .Where(pt => pt.PatientId.Equals(p.Id))
                    .OrderBy(pt => pt.CreatedAtUtc)
                    .Select(pt =>
                    {
                        tests.TryGetValue(pt.TestId, out var test);
                        return new PatientTestSummaryDto(
                            pt.Id.Value,
                            pt.TestId.Value,
                            test?.Name ?? string.Empty,
                            test?.TestCode ?? string.Empty,
                            pt.PriceAtOrderTime,
                            pt.IsUrine,
                            pt.IsStool,
                            pt.IsBlood,
                            pt.IsSemen,
                            pt.IsCsf,
                            pt.IsTakenOutsideLab,
                            pt.IsSampleDrawn,
                            pt.SampleDrawnAtUtc,
                            pt.CreatedAtUtc);
                    })
                    .ToList()))
            .ToList();

        return Task.FromResult(Result<IReadOnlyList<VisitHistoryDto>>.Success(visits));
    }
}