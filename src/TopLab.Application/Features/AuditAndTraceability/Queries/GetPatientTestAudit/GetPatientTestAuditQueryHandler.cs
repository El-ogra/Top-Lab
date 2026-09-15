using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.AuditAndTraceability.Common;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;
using TopLab.Domain.Users;

namespace TopLab.Application.Features.AuditAndTraceability.Queries.GetPatientTestAudit;

/// <summary>
/// T view (SD-10-6): read-only per-test lifecycle audit. Lifecycle columns
/// map null-conditionally (partial lifecycles return nulls, not errors).
/// User names resolve via one <c>Users</c> dictionary read with raw-id
/// fallback for deleted users (SD-10-4).
/// </summary>
public sealed class GetPatientTestAuditQueryHandler
    : IRequestHandler<GetPatientTestAuditQuery, Result<PatientTestAuditDto>>
{
    private readonly IApplicationDbContext _db;

    public GetPatientTestAuditQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<PatientTestAuditDto>> Handle(
        GetPatientTestAuditQuery request, CancellationToken cancellationToken)
    {
        var patientTest = _db.Set<PatientTest>()
            .FirstOrDefault(pt => pt.Id.Value == request.PatientTestId);

        if (patientTest is null)
        {
            return Task.FromResult(Result<PatientTestAuditDto>.Failure(
                Error.NotFound("التحليل غير موجود")));
        }

        var userIds = new[]
            {
                patientTest.EnteredByUserId,
                patientTest.ReviewedByUserId,
                patientTest.LastPrintedByUserId,
                patientTest.DeliveredByUserId,
            }
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToHashSet();

        var userNames = _db.Set<User>()
            .Where(u => userIds.Contains(u.Id.Value))
            .ToDictionary(u => u.Id.Value, u => u.UserName);

        string? Resolve(int? userId) =>
            userId is null
                ? null
                : userNames.TryGetValue(userId.Value, out var name)
                    ? name
                    : userId.Value.ToString();

        var catalog = _db.Set<Test>().ToDictionary(t => t.Id.Value);
        var testName = catalog.TryGetValue(patientTest.TestId.Value, out var test)
            ? test.Name
            : string.Empty;

        var dto = new PatientTestAuditDto(
            patientTest.Id.Value,
            patientTest.PatientId.Value,
            testName,
            patientTest.EnteredByUserId,
            Resolve(patientTest.EnteredByUserId),
            patientTest.EnteredAtUtc,
            patientTest.ReviewedByUserId,
            Resolve(patientTest.ReviewedByUserId),
            patientTest.ReviewedAtUtc,
            patientTest.LastPrintedByUserId,
            Resolve(patientTest.LastPrintedByUserId),
            patientTest.LastPrintedAtUtc,
            patientTest.PrintCount,
            patientTest.DeliveredByUserId,
            Resolve(patientTest.DeliveredByUserId),
            patientTest.DeliveredAtUtc);

        return Task.FromResult(Result<PatientTestAuditDto>.Success(dto));
    }
}
