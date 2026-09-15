using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.SentOutSamples.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.ExternalEntities;
using TopLab.Domain.SentOutSamples;

namespace TopLab.Application.Features.SentOutSamples.Queries.GetSentOutLabAccount;

public sealed class GetSentOutLabAccountQueryHandler
    : IRequestHandler<GetSentOutLabAccountQuery, Result<SentOutLabAccountDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _dateTime;

    public GetSentOutLabAccountQueryHandler(IApplicationDbContext db, IDateTimeProvider dateTime)
    {
        _db = db;
        _dateTime = dateTime;
    }

    public Task<Result<SentOutLabAccountDto>> Handle(
        GetSentOutLabAccountQuery request, CancellationToken cancellationToken)
    {
        var entity = _db.Set<ExternalEntity>().FirstOrDefault(e => e.Id.Value == request.ExternalLabEntityId);
        if (entity is null)
        {
            return Task.FromResult(Result<SentOutLabAccountDto>.Failure(
                Error.NotFound("الجهة الخارجية غير موجودة.")));
        }

        if (entity.EntityType != EntityType.PartnerLab)
        {
            return Task.FromResult(Result<SentOutLabAccountDto>.Failure(
                Error.Conflict("الجهة المختارة ليست معملًا خارجيًا.")));
        }

        var today = DateOnly.FromDateTime(_dateTime.UtcNow);
        var from = request.From ?? today;
        var to = request.To ?? today;
        var fromStart = from.ToDateTime(TimeOnly.MinValue);
        var toEndExclusive = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var samples = _db.Set<SentOutSample>()
            .Where(s => s.ExternalLabEntityId.Value == request.ExternalLabEntityId
                && s.SentAtUtc >= fromStart && s.SentAtUtc < toEndExclusive)
            .ToList();

        var sampleIds = samples.Select(s => s.Id.Value).ToList();
        var payments = _db.Set<SentOutSamplePayment>()
            .Where(p => sampleIds.Contains(p.SentOutSampleId.Value))
            .ToList();

        var totalCost = SentOutAccountCalculator.TotalCost(samples);
        var totalPaid = SentOutAccountCalculator.TotalPaid(payments);

        return Task.FromResult(Result<SentOutLabAccountDto>.Success(new SentOutLabAccountDto(
            entity.Id.Value,
            entity.Name,
            samples.Count,
            totalCost,
            totalPaid,
            SentOutAccountCalculator.Remaining(totalCost, totalPaid))));
    }
}
