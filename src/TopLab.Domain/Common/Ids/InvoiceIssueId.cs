using TopLab.Domain.Common;

namespace TopLab.Domain.Common.Ids;

/// <summary>Strongly-typed identifier for InvoiceIssue.</summary>
public sealed class InvoiceIssueId : StronglyTypedId<int>
{
    private InvoiceIssueId(int value) : base(value)
    {
    }

    public static InvoiceIssueId Create(int value) => new(value);
}
