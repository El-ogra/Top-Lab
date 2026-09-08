using TopLab.Domain.Billing;
using TopLab.Domain.Common.Enums;
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
    public void Create_NegativeAmount_Rejected()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            PaymentOperation.Create(PaymentOperationId.Create(1), PatientId.Create(1), -1m, 1, DateTime.UtcNow));
        Assert.Equal("amount", ex.ParamName);
    }

    [Fact]
    public void Create_NegativeDiscount_Rejected()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            PaymentOperation.Create(PaymentOperationId.Create(1), PatientId.Create(1), 200m, 1, DateTime.UtcNow, -5m));
        Assert.Equal("discountAmount", ex.ParamName);
    }

    [Fact]
    public void Create_DiscountAboveAmount_Rejected()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            PaymentOperation.Create(PaymentOperationId.Create(1), PatientId.Create(1), 200m, 1, DateTime.UtcNow, 201m));
        Assert.Equal("discountAmount", ex.ParamName);
    }

    [Fact]
    public void Create_DiscountOnExtraCharge_Rejected()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            PaymentOperation.Create(PaymentOperationId.Create(1), PatientId.Create(1), 200m, 1, DateTime.UtcNow, 10m, true));
        Assert.Equal("discountAmount", ex.ParamName);
    }

    [Fact]
    public void Create_FullSettlementWithZeroAmount_Rejected()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            PaymentOperation.Create(PaymentOperationId.Create(1), PatientId.Create(1), 0m, 1, DateTime.UtcNow, null, false, OperationType.FullSettlement));
        Assert.Equal("amount", ex.ParamName);
    }

    [Fact]
    public void Void_ThenReissue_VoidedRowSurvivesAndIsExcludedFromTotals()
    {
        var voided = PaymentOperation.Create(PaymentOperationId.Create(1), PatientId.Create(1), 200m, 1, DateTime.UtcNow);
        voided.Void();
        var reissue = PaymentOperation.Create(PaymentOperationId.Create(2), PatientId.Create(1), 200m, 1, DateTime.UtcNow);

        Assert.True(voided.IsVoided);
        Assert.False(reissue.IsVoided);

        var ops = new List<PaymentOperation> { voided, reissue };
        Assert.Equal(200m, PatientAccountCalculator.TotalPaid(ops));
    }

    [Fact]
    public void Void_IsIdempotent()
    {
        var po = PaymentOperation.Create(PaymentOperationId.Create(1), PatientId.Create(1), 200m, 1, DateTime.UtcNow);
        po.Void();
        po.Void();
        Assert.True(po.IsVoided);
    }

    [Fact]
    public void IsEffectivelyZero_FlagsOnlyNonVoidedZeroRows()
    {
        var zero = PaymentOperation.Create(PaymentOperationId.Create(1), PatientId.Create(1), 0m, 1, DateTime.UtcNow);
        var paid = PaymentOperation.Create(PaymentOperationId.Create(2), PatientId.Create(1), 200m, 1, DateTime.UtcNow);
        var voidedZero = PaymentOperation.Create(PaymentOperationId.Create(3), PatientId.Create(1), 0m, 1, DateTime.UtcNow);
        voidedZero.Void();

        Assert.True(zero.IsEffectivelyZero);
        Assert.False(paid.IsEffectivelyZero);
        Assert.False(voidedZero.IsEffectivelyZero);
    }

    [Fact]
    public void PriceList_Create()
    {
        var pl = PriceList.Create(PriceListId.Create(1), "Contract A");
        Assert.Equal("Contract A", pl.Name);
    }
}
