using TopLab.Domain.Billing;
using TopLab.Domain.Common.Ids;
using Xunit;

namespace TopLab.Domain.Tests.Billing;

public class PaymentTests
{
    [Fact]
    public void Create_Valid()
    {
        var po = PaymentOperation.Create(PaymentOperationId.Create(1), PatientId.Create(1), 200m, 1, DateTime.UtcNow);
        Assert.False(po.IsVoided);
        Assert.Equal(200m, po.Amount);
    }

    [Fact]
    public void Void_Sets()
    {
        var po = PaymentOperation.Create(PaymentOperationId.Create(1), PatientId.Create(1), 200m, 1, DateTime.UtcNow);
        po.Void();
        Assert.True(po.IsVoided);
    }

    [Fact]
    public void PriceList_Create()
    {
        var pl = PriceList.Create(PriceListId.Create(1), "Contract A");
        Assert.Equal("Contract A", pl.Name);
    }
}
