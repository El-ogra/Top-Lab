using TopLab.Domain.Common;
using TopLab.Domain.Common.Ids;

namespace TopLab.Domain.SentOutSamples;

public sealed class SentOutSamplePayment : Entity<SentOutSamplePaymentId>
{
    public SentOutSampleId SentOutSampleId { get; private set; } = default!;

    public decimal AmountPaid { get; private set; }

    public DateTime PaidAtUtc { get; private set; }

    public int PerformedByUserId { get; private set; }

    private SentOutSamplePayment()
    {
    }

    private SentOutSamplePayment(SentOutSamplePaymentId id, SentOutSampleId sentOutSampleId, decimal amountPaid, DateTime paidAtUtc, int performedByUserId)
        : base(id)
    {
        SentOutSampleId = sentOutSampleId;
        AmountPaid = amountPaid;
        PaidAtUtc = paidAtUtc;
        PerformedByUserId = performedByUserId;
    }

    public static SentOutSamplePayment Create(SentOutSamplePaymentId id, SentOutSampleId sentOutSampleId, decimal amountPaid, DateTime paidAtUtc, int performedByUserId)
    {
        if (amountPaid <= 0)
        {
            throw new ArgumentException("AmountPaid must be > 0.", nameof(amountPaid));
        }

        return new SentOutSamplePayment(id, sentOutSampleId, amountPaid, paidAtUtc, performedByUserId);
    }
}
