using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.Statistics.Common;
using TopLab.Domain.Results;
using TopLab.Domain.Users;

namespace TopLab.Application.Features.Statistics.Queries.GetUserProductivityStatistics;

public sealed class GetUserProductivityStatisticsQueryHandler
    : IRequestHandler<GetUserProductivityStatisticsQuery, Result<UserProductivityStatisticsDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _dateTime;

    public GetUserProductivityStatisticsQueryHandler(IApplicationDbContext db, IDateTimeProvider dateTime)
    {
        _db = db;
        _dateTime = dateTime;
    }

    public Task<Result<UserProductivityStatisticsDto>> Handle(
        GetUserProductivityStatisticsQuery request,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(_dateTime.UtcNow);
        var from = request.From ?? today;
        var to = request.To ?? today;
        var fromStart = from.ToDateTime(TimeOnly.MinValue);
        var toEndExclusive = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var rows = _db.Set<PatientTest>()
            .Where(pt => (pt.EnteredAtUtc != null && pt.EnteredAtUtc >= fromStart && pt.EnteredAtUtc < toEndExclusive)
                || (pt.ReviewedAtUtc != null && pt.ReviewedAtUtc >= fromStart && pt.ReviewedAtUtc < toEndExclusive)
                || (pt.LastPrintedAtUtc != null && pt.LastPrintedAtUtc >= fromStart && pt.LastPrintedAtUtc < toEndExclusive)
                || (pt.DeliveredAtUtc != null && pt.DeliveredAtUtc >= fromStart && pt.DeliveredAtUtc < toEndExclusive))
            .ToList();

        if (request.UserId.HasValue)
        {
            var userId = request.UserId.Value;
            rows = rows
                .Where(r => r.EnteredByUserId == userId
                    || r.ReviewedByUserId == userId
                    || r.LastPrintedByUserId == userId
                    || r.DeliveredByUserId == userId)
                .ToList();
        }

        var counters = new Dictionary<int, (int Entered, int Reviewed, int Printed, int Delivered)>();

        foreach (var row in rows)
        {
            if (row.EnteredByUserId.HasValue
                && row.EnteredAtUtc >= fromStart && row.EnteredAtUtc < toEndExclusive)
            {
                var current = counters.TryGetValue(row.EnteredByUserId.Value, out var e) ? e : default;
                counters[row.EnteredByUserId.Value] = current with { Entered = current.Entered + 1 };
            }

            if (row.ReviewedByUserId.HasValue
                && row.ReviewedAtUtc >= fromStart && row.ReviewedAtUtc < toEndExclusive)
            {
                var current = counters.TryGetValue(row.ReviewedByUserId.Value, out var e) ? e : default;
                counters[row.ReviewedByUserId.Value] = current with { Reviewed = current.Reviewed + 1 };
            }

            if (row.LastPrintedByUserId.HasValue
                && row.LastPrintedAtUtc >= fromStart && row.LastPrintedAtUtc < toEndExclusive
                && row.PrintCount > 0)
            {
                var current = counters.TryGetValue(row.LastPrintedByUserId.Value, out var e) ? e : default;
                counters[row.LastPrintedByUserId.Value] = current with { Printed = current.Printed + row.PrintCount };
            }

            if (row.DeliveredByUserId.HasValue
                && row.DeliveredAtUtc >= fromStart && row.DeliveredAtUtc < toEndExclusive)
            {
                var current = counters.TryGetValue(row.DeliveredByUserId.Value, out var e) ? e : default;
                counters[row.DeliveredByUserId.Value] = current with { Delivered = current.Delivered + 1 };
            }
        }

        var userIds = counters.Keys.ToList();
        var names = _db.Set<User>()
            .Where(u => userIds.Contains(u.Id.Value))
            .ToDictionary(u => u.Id.Value, u => u.UserName);

        var users = counters
            .OrderBy(kv => kv.Key)
            .Select(kv =>
            {
                var name = names.TryGetValue(kv.Key, out var userName)
                    ? userName
                    : kv.Key.ToString();
                return new UserProductivityDto(
                    kv.Key,
                    name,
                    kv.Value.Entered,
                    kv.Value.Reviewed,
                    kv.Value.Printed,
                    kv.Value.Delivered);
            })
            .ToList();

        var dto = new UserProductivityStatisticsDto(from, to, users);

        return Task.FromResult(Result<UserProductivityStatisticsDto>.Success(dto));
    }
}
