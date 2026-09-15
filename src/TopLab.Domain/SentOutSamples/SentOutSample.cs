using TopLab.Domain.Common;
using TopLab.Domain.Common.Ids;

namespace TopLab.Domain.SentOutSamples;

public sealed class SentOutSample : AuditableEntity<SentOutSampleId>
{
    public PatientTestId PatientTestId { get; private set; } = default!;

    public ExternalEntityId ExternalLabEntityId { get; private set; } = default!;

    public decimal CostPrice { get; private set; }

    public decimal PatientPrice { get; private set; }

    public DateTime SentAtUtc { get; private set; }

    private SentOutSample()
    {
    }

    private SentOutSample(SentOutSampleId id, PatientTestId patientTestId, ExternalEntityId externalLabEntityId, decimal costPrice, decimal patientPrice, DateTime sentAtUtc)
        : base(id)
    {
        PatientTestId = patientTestId;
        ExternalLabEntityId = externalLabEntityId;
        CostPrice = costPrice;
        PatientPrice = patientPrice;
        SentAtUtc = sentAtUtc;
    }

    public static SentOutSample Create(SentOutSampleId id, PatientTestId patientTestId, ExternalEntityId externalLabEntityId, decimal costPrice, decimal patientPrice, DateTime sentAtUtc)
    {
        if (costPrice < 0)
        {
            throw new ArgumentException("CostPrice must be >= 0.", nameof(costPrice));
        }

        if (patientPrice < 0)
        {
            throw new ArgumentException("PatientPrice must be >= 0.", nameof(patientPrice));
        }

        return new SentOutSample(id, patientTestId, externalLabEntityId, costPrice, patientPrice, sentAtUtc);
    }
}
