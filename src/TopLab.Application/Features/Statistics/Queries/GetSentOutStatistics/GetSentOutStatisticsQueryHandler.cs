using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.Statistics.Common;
using TopLab.Domain.ExternalEntities;
using TopLab.Domain.SentOutSamples;

namespace TopLab.Application.Features.Statistics.Queries.GetSentOutStatistics;

public sealed class GetSentOutStatisticsQueryHandler
    : IRequestHandler<GetSentOutStatisticsQuery, Result<SentOutStatisticsDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _dateTime;

    public GetSentOutStatisticsQueryHandler(IApplicationDbContext db, IDateTimeProvider dateTime)
    {
        _db = db;
        _dateTime = dateTime;
    }

    public Task<Result<SentOutStatisticsDto>> Handle(
        GetSentOutStatisticsQuery request,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(_dateTime.UtcNow);
        var from = request.From ?? today;
        var to = request.To ?? today;
        var fromStart = from.ToDateTime(TimeOnly.MinValue);
        var toEndExclusive = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

        if (request.ExternalLabEntityId.HasValue
            && !_db.Set<ExternalEntity>().Any(e => e.Id.Value == request.ExternalLabEntityId.Value))
        {
            return Task.FromResult(Result<SentOutStatisticsDto>.Failure(
                Error.NotFound("الجهة الخارجية غير موجودة.", "NotFound")));
        }

        var query = _db.Set<SentOutSample>()
            .Where(s => s.SentAtUtc >= fromStart && s.SentAtUtc < toEndExclusive);

        if (request.ExternalLabEntityId.HasValue)
        {
            var labId = request.ExternalLabEntityId.Value;
            query = query.Where(s => s.ExternalLabEntityId.Value == labId);
        }

        var samples = query.ToList();
        var sampleIds = samples.Select(s => s.Id.Value).ToList();
        var payments = _db.Set<SentOutSamplePayment>()
            .Where(p => sampleIds.Contains(p.SentOutSampleId.Value))
            .ToList()
            .GroupBy(p => p.SentOutSampleId.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var labIds = samples.Select(s => s.ExternalLabEntityId.Value).Distinct().ToList();
        var labNames = _db.Set<ExternalEntity>()
            .Where(e => labIds.Contains(e.Id.Value))
            .ToDictionary(e => e.Id.Value, e => e.Name);

        var labs = samples
            .GroupBy(s => s.ExternalLabEntityId.Value)
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var labSamples = g.ToList();
                var labPayments = labSamples
                    .SelectMany(s => payments.TryGetValue(s.Id.Value, out var list) ? list : [])
                    .ToList();

                var totalCost = SentOutAccountCalculator.TotalCost(labSamples);
                var totalPaid = SentOutAccountCalculator.TotalPaid(labPayments);

                var name = labNames.TryGetValue(g.Key, out var labName)
                    ? labName
                    : g.Key.ToString();

                return new SentOutLabStatisticsDto(
                    g.Key,
                    name,
                    labSamples.Count,
                    totalCost,
                    totalPaid,
                    SentOutAccountCalculator.Remaining(totalCost, totalPaid));
            })
            .ToList();

        var dto = new SentOutStatisticsDto(from, to, samples.Count, labs);

        return Task.FromResult(Result<SentOutStatisticsDto>.Success(dto));
    }
}
