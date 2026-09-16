using TopLab.Domain.Common;
using TopLab.Domain.Common.Ids;

namespace TopLab.Domain.Billing;

/// <summary>
/// Invoice issue record (S-01 slice S3, Settled Decision SD-3).
/// An invoice is an itemized, sequentially numbered statement of services
/// (what was charged). Every physical print allocates a new numbered issue;
/// reprints never reuse a number. Immutable after creation.
/// <c>PatientId</c> is an application-level reference (deliberate no-FK link,
/// per the repo's FK-matrix convention) — deleting a patient never cascades
/// here and no model FK exists to assert on.
/// </summary>
public sealed class InvoiceIssue : Entity<InvoiceIssueId>
{
    public PatientId PatientId { get; private set; } = default!;

    public int InvoiceNumber { get; private set; }

    public DateTime IssuedAtUtc { get; private set; }

    public int IssuedByUserId { get; private set; }

    public decimal TotalCharged { get; private set; }

    public decimal TotalDiscount { get; private set; }

    public int ItemCount { get; private set; }

    private InvoiceIssue()
    {
    }

    private InvoiceIssue(
        InvoiceIssueId id,
        PatientId patientId,
        int invoiceNumber,
        DateTime issuedAtUtc,
        int issuedByUserId,
        decimal totalCharged,
        decimal totalDiscount,
        int itemCount)
        : base(id)
    {
        PatientId = patientId;
        InvoiceNumber = invoiceNumber;
        IssuedAtUtc = issuedAtUtc;
        IssuedByUserId = issuedByUserId;
        TotalCharged = totalCharged;
        TotalDiscount = totalDiscount;
        ItemCount = itemCount;
    }

    public static InvoiceIssue Create(
        InvoiceIssueId id,
        PatientId patientId,
        int invoiceNumber,
        DateTime issuedAtUtc,
        int issuedByUserId,
        decimal totalCharged,
        decimal totalDiscount,
        int itemCount)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(patientId);

        if (invoiceNumber < 1)
        {
            throw new ArgumentException("Invoice number must be positive.", nameof(invoiceNumber));
        }

        if (totalCharged < 0m)
        {
            throw new ArgumentException("Total charged cannot be negative.", nameof(totalCharged));
        }

        if (totalDiscount < 0m)
        {
            throw new ArgumentException("Total discount cannot be negative.", nameof(totalDiscount));
        }

        if (itemCount < 0)
        {
            throw new ArgumentException("Item count cannot be negative.", nameof(itemCount));
        }

        return new InvoiceIssue(id, patientId, invoiceNumber, issuedAtUtc, issuedByUserId, totalCharged, totalDiscount, itemCount);
    }
}
