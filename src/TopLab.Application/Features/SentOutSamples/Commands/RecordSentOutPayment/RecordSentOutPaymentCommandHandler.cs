using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.SentOutSamples.Common;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.SentOutSamples;

namespace TopLab.Application.Features.SentOutSamples.Commands.RecordSentOutPayment;

public sealed class RecordSentOutPaymentCommandHandler : IRequestHandler<RecordSentOutPaymentCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _dateTime;

    public RecordSentOutPaymentCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider dateTime)
    {
        _db = db;
        _currentUser = currentUser;
        _dateTime = dateTime;
    }

    public async Task<Result> Handle(RecordSentOutPaymentCommand request, CancellationToken cancellationToken)
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

        if (request.AmountPaid > remaining)
        {
            return Result.Failure(Error.Conflict("مبلغ الدفع يتجاوز المتبقي."));
        }

        SentOutSamplePayment payment;
        try
        {
            payment = SentOutSamplePayment.Create(
                SentOutSamplePaymentId.Create(0),
                sample.Id,
                request.AmountPaid,
                _dateTime.UtcNow,
                _currentUser.UserId);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(Error.Validation(DomainFailureTranslator.Translate(ex)));
        }

        _db.Add(payment);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
