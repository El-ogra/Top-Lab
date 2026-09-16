using TopLab.Domain.Billing;
using TopLab.Domain.Common.Ids;
using Xunit;

namespace TopLab.Domain.Tests.Billing;

public class InvoiceIssueTests
{
    private static InvoiceIssue Issue(
        int number = 1,
        decimal charged = 150m,
        decimal discount = 15m,
        int items = 2)
    {
        return InvoiceIssue.Create(
            InvoiceIssueId.Create(0),
            PatientId.Create(7),
            number,
            new DateTime(2026, 9, 16, 10, 0, 0, DateTimeKind.Utc),
            3,
            charged,
            discount,
            items);
    }

    [Fact]
    public void Create_HappyPath_SetsAllFields()
    {
        var issue = Issue();

        Assert.Equal(7, issue.PatientId.Value);
        Assert.Equal(1, issue.InvoiceNumber);
        Assert.Equal(3, issue.IssuedByUserId);
        Assert.Equal(150m, issue.TotalCharged);
        Assert.Equal(15m, issue.TotalDiscount);
        Assert.Equal(2, issue.ItemCount);
    }

    [Fact]
    public void Create_ZeroInvoiceNumber_Throws()
    {
        Assert.Throws<ArgumentException>(() => Issue(number: 0));
    }

    [Fact]
    public void Create_NegativeInvoiceNumber_Throws()
    {
        Assert.Throws<ArgumentException>(() => Issue(number: -5));
    }

    [Fact]
    public void Create_NegativeTotalCharged_Throws()
    {
        Assert.Throws<ArgumentException>(() => Issue(charged: -1m));
    }

    [Fact]
    public void Create_NegativeTotalDiscount_Throws()
    {
        Assert.Throws<ArgumentException>(() => Issue(discount: -1m));
    }

    [Fact]
    public void Create_NegativeItemCount_Throws()
    {
        Assert.Throws<ArgumentException>(() => Issue(items: -1));
    }

    [Fact]
    public void Create_EmptyVisit_Allowed()
    {
        var issue = Issue(charged: 0m, discount: 0m, items: 0);

        Assert.Equal(0, issue.ItemCount);
        Assert.Equal(0m, issue.TotalCharged);
    }
}
