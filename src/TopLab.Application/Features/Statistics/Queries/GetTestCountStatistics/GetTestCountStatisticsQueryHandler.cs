using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.Statistics.Common;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.Statistics.Queries.GetTestCountStatistics;

public sealed class GetTestCountStatisticsQueryHandler
    : IRequestHandler<GetTestCountStatisticsQuery, Result<TestCountStatisticsDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _dateTime;

    public GetTestCountStatisticsQueryHandler(IApplicationDbContext db, IDateTimeProvider dateTime)
    {
        _db = db;
        _dateTime = dateTime;
    }

    public Task<Result<TestCountStatisticsDto>> Handle(
        GetTestCountStatisticsQuery request,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(_dateTime.UtcNow);
        var from = request.From ?? today;
        var to = request.To ?? today;
        var fromStart = from.ToDateTime(TimeOnly.MinValue);
        var toEndExclusive = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

        if (request.TestGroupId.HasValue
            && !_db.Set<TestGroup>().Any(g => g.Id.Value == request.TestGroupId.Value))
        {
            return Task.FromResult(Result<TestCountStatisticsDto>.Failure(
                Error.NotFound("مجموعة التحاليل غير موجودة.", "NotFound")));
        }

        var patientTests = _db.Set<PatientTest>()
            .Where(pt => pt.CreatedAtUtc >= fromStart && pt.CreatedAtUtc < toEndExclusive)
            .ToList();

        var livePatientIds = _db.Set<Patient>()
            .Where(p => !p.IsDeleted)
            .Select(p => p.Id.Value)
            .ToHashSet();

        patientTests = patientTests
            .Where(pt => livePatientIds.Contains(pt.PatientId.Value))
            .ToList();

        var testIds = patientTests.Select(pt => pt.TestId.Value).Distinct().ToList();
        var tests = _db.Set<Test>()
            .Where(t => testIds.Contains(t.Id.Value))
            .ToDictionary(t => t.Id.Value);

        if (request.TestGroupId.HasValue)
        {
            var groupId = request.TestGroupId.Value;
            patientTests = patientTests
                .Where(pt => tests.TryGetValue(pt.TestId.Value, out var test)
                    && test.TestGroupId is not null
                    && test.TestGroupId.Value == groupId)
                .ToList();
            testIds = patientTests.Select(pt => pt.TestId.Value).Distinct().ToList();
        }

        var testCounts = patientTests
            .GroupBy(pt => pt.TestId.Value)
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var name = tests.TryGetValue(g.Key, out var test)
                    ? test.Name
                    : g.Key.ToString();
                return new TestCountDto(g.Key, name, g.Count());
            })
            .ToList();

        var groupIds = tests.Values
            .Where(t => t.TestGroupId is not null)
            .Select(t => t.TestGroupId!.Value)
            .Distinct()
            .ToList();

        var groups = _db.Set<TestGroup>()
            .Where(g => groupIds.Contains(g.Id.Value))
            .ToDictionary(g => g.Id.Value, g => g.Name);

        var groupCounts = patientTests
            .Where(pt => tests.TryGetValue(pt.TestId.Value, out var test) && test.TestGroupId is not null)
            .GroupBy(pt => tests[pt.TestId.Value].TestGroupId!.Value)
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var name = groups.TryGetValue(g.Key, out var groupName)
                    ? groupName
                    : g.Key.ToString();
                return new TestGroupCountDto(g.Key, name, g.Count());
            })
            .ToList();

        var dto = new TestCountStatisticsDto(
            from,
            to,
            patientTests.Count,
            testCounts,
            groupCounts);

        return Task.FromResult(Result<TestCountStatisticsDto>.Success(dto));
    }
}
