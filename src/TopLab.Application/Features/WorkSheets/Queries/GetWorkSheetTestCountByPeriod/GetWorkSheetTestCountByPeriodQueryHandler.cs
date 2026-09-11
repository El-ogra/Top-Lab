using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.WorkSheets.Common;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.WorkSheets.Queries.GetWorkSheetTestCountByPeriod;

/// <summary>
/// FR-M11-004 period test-count classification. This is a period activity count,
/// so the <c>!IsTakenOutsideLab</c> bench-work exclusion does NOT apply here.
/// </summary>
public sealed class GetWorkSheetTestCountByPeriodQueryHandler
    : IRequestHandler<GetWorkSheetTestCountByPeriodQuery, Result<WorkSheetTestCountDto>>
{
    private readonly IApplicationDbContext _db;

    public GetWorkSheetTestCountByPeriodQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<WorkSheetTestCountDto>> Handle(
        GetWorkSheetTestCountByPeriodQuery request, CancellationToken cancellationToken)
    {
        var (from, to) = WorkSheetPeriod.Resolve(request.From, request.To);

        var patientIds = _db.Set<Patient>()
            .Where(p => !p.IsDeleted
                && DateOnly.FromDateTime(p.RegistrationDateUtc) >= from
                && DateOnly.FromDateTime(p.RegistrationDateUtc) <= to)
            .Select(p => p.Id.Value)
            .ToHashSet();

        var testsById = _db.Set<Test>().ToDictionary(t => t.Id.Value);

        var counts = _db.Set<PatientTest>()
            .Where(pt => patientIds.Contains(pt.PatientId.Value))
            .GroupBy(pt => pt.TestId.Value)
            .Select(g => new { TestId = g.Key, Count = g.Count() })
            .ToList();

        var rows = counts
            .Select(c =>
            {
                testsById.TryGetValue(c.TestId, out var test);
                return new WorkSheetTestCountRowDto(
                    c.TestId,
                    test?.Name ?? string.Empty,
                    test?.TestCode ?? string.Empty,
                    c.Count);
            })
            .OrderByDescending(r => r.Count)
            .ThenBy(r => r.TestId)
            .ToList();

        return Task.FromResult(Result<WorkSheetTestCountDto>.Success(
            new WorkSheetTestCountDto(from, to, rows, rows.Sum(r => r.Count))));
    }
}
