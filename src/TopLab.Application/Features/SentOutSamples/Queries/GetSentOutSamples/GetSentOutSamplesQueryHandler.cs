using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.SentOutSamples.Common;
using TopLab.Domain.ExternalEntities;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.SentOutSamples;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.SentOutSamples.Queries.GetSentOutSamples;

public sealed class GetSentOutSamplesQueryHandler
    : IRequestHandler<GetSentOutSamplesQuery, Result<IReadOnlyList<SentOutSampleDto>>>
{
    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _dateTime;

    public GetSentOutSamplesQueryHandler(IApplicationDbContext db, IDateTimeProvider dateTime)
    {
        _db = db;
        _dateTime = dateTime;
    }

    public Task<Result<IReadOnlyList<SentOutSampleDto>>> Handle(
        GetSentOutSamplesQuery request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(_dateTime.UtcNow);
        var from = request.From ?? today;
        var to = request.To ?? today;
        var fromStart = from.ToDateTime(TimeOnly.MinValue);
        var toEndExclusive = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var query = _db.Set<SentOutSample>().AsQueryable();
        if (request.ExternalLabEntityId.HasValue)
        {
            query = query.Where(s => s.ExternalLabEntityId.Value == request.ExternalLabEntityId.Value);
        }

        var rows = query
            .Where(s => s.SentAtUtc >= fromStart && s.SentAtUtc < toEndExclusive)
            .OrderBy(s => s.SentAtUtc)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var patientTestIds = rows.Select(r => r.PatientTestId.Value).Distinct().ToList();
        var patientTests = _db.Set<PatientTest>()
            .Where(pt => patientTestIds.Contains(pt.Id.Value))
            .ToDictionary(pt => pt.Id.Value);

        var patientIds = patientTests.Values.Select(pt => pt.PatientId.Value).Distinct().ToList();
        var testIds = patientTests.Values.Select(pt => pt.TestId.Value).Distinct().ToList();
        var labIds = rows.Select(r => r.ExternalLabEntityId.Value).Distinct().ToList();

        var patients = _db.Set<Patient>()
            .Where(p => patientIds.Contains(p.Id.Value))
            .ToDictionary(p => p.Id.Value);
        var tests = _db.Set<Test>()
            .Where(t => testIds.Contains(t.Id.Value))
            .ToDictionary(t => t.Id.Value);
        var labs = _db.Set<ExternalEntity>()
            .Where(e => labIds.Contains(e.Id.Value))
            .ToDictionary(e => e.Id.Value);

        var sampleIds = rows.Select(r => r.Id.Value).ToList();
        var paidBySample = _db.Set<SentOutSamplePayment>()
            .Where(p => sampleIds.Contains(p.SentOutSampleId.Value))
            .ToList()
            .GroupBy(p => p.SentOutSampleId.Value)
            .ToDictionary(g => g.Key, g => g.Sum(p => p.AmountPaid));

        IReadOnlyList<SentOutSampleDto> items = rows
            .Select(s =>
            {
                patientTests.TryGetValue(s.PatientTestId.Value, out var patientTest);
                var patientName = patientTest != null && patients.TryGetValue(patientTest.PatientId.Value, out var patient)
                    ? patient.FullName
                    : string.Empty;
                var testName = patientTest != null && tests.TryGetValue(patientTest.TestId.Value, out var test)
                    ? test.Name
                    : string.Empty;
                var labName = labs.TryGetValue(s.ExternalLabEntityId.Value, out var lab)
                    ? lab.Name
                    : string.Empty;
                var paid = paidBySample.TryGetValue(s.Id.Value, out var totalPaid) ? totalPaid : 0m;
                var remaining = SentOutAccountCalculator.Remaining(s.CostPrice, paid);

                return new SentOutSampleDto(
                    s.Id.Value,
                    s.PatientTestId.Value,
                    patientName,
                    testName,
                    s.ExternalLabEntityId.Value,
                    labName,
                    s.CostPrice,
                    s.PatientPrice,
                    s.SentAtUtc,
                    paid,
                    remaining,
                    SentOutAccountCalculator.IsFullySettled(s.CostPrice, paid));
            })
            .ToList();

        return Task.FromResult(Result<IReadOnlyList<SentOutSampleDto>>.Success(items));
    }
}
