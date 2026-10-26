using TopLab.Domain.Billing;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using Xunit;

namespace TopLab.Domain.Tests.Billing;

public class PaymentOperationRefundTests
{
    private static PaymentOperation Make(
        decimal amount = 0m,
        OperationType operationType = OperationType.Payment,
        decimal? refundAmount = null,
        decimal? discountAmount = null,
        bool isExtraCharge = false) =>
        PaymentOperation.Create(
            PaymentOperationId.Create(1),
            PatientId.Create(1),
            amount,
            1,
            DateTime.UtcNow,
            discountAmount,
            isExtraCharge,
            operationType,
            refundAmount);

    [Fact]
    public void Refund_StoresRefundAmount()
    {
        var op = Make(operationType: OperationType.Refund, refundAmount: 25m);

        Assert.Equal(OperationType.Refund, op.OperationType);
        Assert.Equal(25m, op.RefundAmount);
        Assert.Equal(0m, op.Amount);
    }

    [Fact]
    public void Refund_RequiresPositiveRefundAmount()
    {
        Assert.Throws<ArgumentException>(() => Make(operationType: OperationType.Refund, refundAmount: null));
        Assert.Throws<ArgumentException>(() => Make(operationType: OperationType.Refund, refundAmount: 0m));
        Assert.Throws<ArgumentException>(() => Make(operationType: OperationType.Refund, refundAmount: -1m));
    }

    [Fact]
    public void Refund_RequiresZeroAmount()
    {
        Assert.Throws<ArgumentException>(() => Make(amount: 10m, operationType: OperationType.Refund, refundAmount: 5m));
    }

    [Fact]
    public void NonRefund_RejectsRefundAmount()
    {
        Assert.Throws<ArgumentException>(() => Make(amount: 10m, operationType: OperationType.Payment, refundAmount: 5m));
        Assert.Throws<ArgumentException>(() => Make(amount: 10m, operationType: OperationType.Correction, refundAmount: 5m));
        Assert.Throws<ArgumentException>(() => Make(amount: 10m, operationType: OperationType.FullSettlement, refundAmount: 5m));
    }

    [Fact]
    public void RefundAmount_NullForNonRefund()
    {
        var op = Make(amount: 10m, operationType: OperationType.Payment);
        Assert.Null(op.RefundAmount);
    }
}
