using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.SentOutSamples.Common;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.SentOutSamples;

namespace TopLab.Application.Features.SentOutSamples.Commands.SettleSentOutInFull;

public sealed class SettleSentOutInFullCommandHandler : IRequestHandler<SettleSentOutInFullCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _dateTime;

    public SettleSentOutInFullCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider dateTime)
    {
        _db = db;
        _currentUser = currentUser;
        _dateTime = dateTime;
    }

    public async Task<Result> Handle(SettleSentOutInFullCommand request, CancellationToken cancellationToken)
    {
        var sample = _db.Set<SentOutSample>().FirstOrDefault(s => s.Id.Value == request.SentOutSampleId);
        if (sample is null)
        {
            return Result.Failure(Error.NotFound("العينة المُرسَلة غير موجودة."));
        }

        var payments = _db.Set<SentOutSamplePayment>()
            .Where(p => p.SentOutSampleId.Value == sample.Id.Value)
            .ToList();
        var remaining = SentOutAccountCalculator.Remaining(
            sample.CostPrice,
            SentOutAccountCalculator.TotalPaid(payments));

        if (remaining <= 0)
        {
            return Result.Failure(Error.Conflict("لا يوجد رصيد مستحق للتسوية."));
        }

        var settlement = SentOutSamplePayment.Create(
            SentOutSamplePaymentId.Create(0),
            sample.Id,
            remaining,
            _dateTime.UtcNow,
            _currentUser.UserId);

        _db.Add(settlement);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
