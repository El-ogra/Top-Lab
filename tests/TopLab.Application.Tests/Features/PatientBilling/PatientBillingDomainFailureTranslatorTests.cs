using TopLab.Application.Features.PatientBilling.Common;
using Xunit;

namespace TopLab.Application.Tests.Features.PatientBilling;

public class PatientBillingDomainFailureTranslatorTests
{
    [Fact]
    public void NegativeAmount_MapsToFrozenMessage()
    {
        var message = DomainFailureTranslator.Translate(
            new ArgumentException("Amount must be >= 0.", "amount"));

        Assert.Equal("المبلغ يجب أن يكون صفرًا أو أكثر.", message);
    }

    [Fact]
    public void DiscountGuard_MapsToFrozenMessage()
    {
        Assert.Equal(
            "الخصم يجب أن يكون صفرًا أو أكثر ولا يتجاوز مبلغ العملية.",
            DomainFailureTranslator.Translate(new ArgumentException("Discount must be >= 0.", "discountAmount")));
        Assert.Equal(
            "الخصم يجب أن يكون صفرًا أو أكثر ولا يتجاوز مبلغ العملية.",
            DomainFailureTranslator.Translate(
                new ArgumentException("Discount cannot exceed the operation amount.", "discountAmount")));
    }

    [Fact]
    public void DiscountOnExtraCharge_MapsToFrozenMessage()
    {
        var message = DomainFailureTranslator.Translate(
            new ArgumentException("An extra charge cannot carry a discount.", "discountAmount"));

        Assert.Equal("لا يمكن إضافة خصم على مبلغ إضافي.", message);
    }

    [Fact]
    public void ZeroSettlement_MapsToFrozenMessage()
    {
        var message = DomainFailureTranslator.Translate(
            new ArgumentException("Settlement amount must be > 0.", "amount"));

        Assert.Equal("مبلغ التسوية يجب أن يكون أكبر من صفر.", message);
    }
}
