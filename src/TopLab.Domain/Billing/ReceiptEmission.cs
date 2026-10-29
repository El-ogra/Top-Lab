using TopLab.Domain.Common;
using TopLab.Domain.Common.Ids;

namespace TopLab.Domain.Billing;

public sealed class ReceiptEmission : Entity<ReceiptEmissionId>
{
    public PatientId PatientId { get; private set; } = default!;
    public DateTime IssuedAtUtc { get; private set; }
    public int IssuedByUserId { get; private set; }
    public decimal TotalCharged { get; private set; }
    public decimal TotalPaid { get; private set; }

    private ReceiptEmission() { }

    private ReceiptEmission(ReceiptEmissionId id, PatientId patientId, DateTime issuedAtUtc, int issuedByUserId, decimal totalCharged, decimal totalPaid)
        : base(id)
    {
        PatientId = patientId;
        IssuedAtUtc = issuedAtUtc;
        IssuedByUserId = issuedByUserId;
        TotalCharged = totalCharged;
        TotalPaid = totalPaid;
    }

    public static ReceiptEmission Create(ReceiptEmissionId id, PatientId patientId, DateTime issuedAtUtc, int issuedByUserId, decimal totalCharged, decimal totalPaid)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(patientId);
        return new ReceiptEmission(id, patientId, issuedAtUtc, issuedByUserId, totalCharged, totalPaid);
    }
}