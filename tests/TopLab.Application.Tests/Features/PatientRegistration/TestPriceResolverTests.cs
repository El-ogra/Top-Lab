using TopLab.Application.Features.PatientRegistration.Common;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.ExternalEntities;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.PatientRegistration;

public class TestPriceResolverTests
{
    private static ExternalEntity ReferralWithPriceList(int priceListIdValue)
    {
        return ExternalEntity.Create(
            ExternalEntityId.Create(1),
            EntityType.ReferralOrContract,
            "Alpha Lab",
            priceListId: PriceListId.Create(priceListIdValue),
            generatedIdCode: "ABC-1");
    }

    private static IReadOnlyDictionary<TestId, decimal> Items(params (int TestId, decimal Price)[] rows)
    {
        return rows.ToDictionary(r => TestId.Create(r.TestId), r => r.Price);
    }

    [Fact]
    public void CashAccount_UsesPatientPrice()
    {
        var price = TestPriceResolver.Resolve(
            AccountType.Individual,
            testPatientPrice: 100m,
            testLabToLabPrice: 50m,
            referralEntity: null,
            priceListItems: Items(),
            customGroupItemPrices: Items(),
            testId: TestId.Create(10));

        Assert.Equal(100m, price);
    }

    [Fact]
    public void LabToLab_WithLabPrice_UsesLabToLabPrice()
    {
        var price = TestPriceResolver.Resolve(
            AccountType.LabToLab,
            testPatientPrice: 100m,
            testLabToLabPrice: 50m,
            referralEntity: null,
            priceListItems: Items(),
            customGroupItemPrices: Items(),
            testId: TestId.Create(10));

        Assert.Equal(50m, price);
    }

    [Fact]
    public void LabToLab_WithoutLabPrice_FallsBackToPatientPrice()
    {
        var price = TestPriceResolver.Resolve(
            AccountType.LabToLab,
            testPatientPrice: 100m,
            testLabToLabPrice: null,
            referralEntity: null,
            priceListItems: Items(),
            customGroupItemPrices: Items(),
            testId: TestId.Create(10));

        Assert.Equal(100m, price);
    }

    [Fact]
    public void Contract_WithPriceListHit_UsesPriceListPrice()
    {
        var price = TestPriceResolver.Resolve(
            AccountType.Contracts,
            testPatientPrice: 100m,
            testLabToLabPrice: 50m,
            referralEntity: ReferralWithPriceList(1),
            priceListItems: Items((10, 75m)),
            customGroupItemPrices: Items(),
            testId: TestId.Create(10));

        Assert.Equal(75m, price);
    }

    [Fact]
    public void Contract_WithPriceListHit_OverridesAccountType()
    {
        var price = TestPriceResolver.Resolve(
            AccountType.LabToLab,
            testPatientPrice: 100m,
            testLabToLabPrice: 50m,
            referralEntity: ReferralWithPriceList(1),
            priceListItems: Items((10, 75m)),
            customGroupItemPrices: Items(),
            testId: TestId.Create(10));

        Assert.Equal(75m, price);
    }

    [Fact]
    public void FreeAccount_UsesPatientPrice()
    {
        var price = TestPriceResolver.Resolve(
            AccountType.Free,
            testPatientPrice: 100m,
            testLabToLabPrice: 200m,
            referralEntity: null,
            priceListItems: Items(),
            customGroupItemPrices: Items(),
            testId: TestId.Create(10));

        Assert.Equal(100m, price);
    }

    [Fact]
    public void CustomGroup_WithoutContract_UsesGroupPrice()
    {
        var price = TestPriceResolver.Resolve(
            AccountType.Individual,
            testPatientPrice: 100m,
            testLabToLabPrice: 50m,
            referralEntity: null,
            priceListItems: Items(),
            customGroupItemPrices: Items((10, 80m)),
            testId: TestId.Create(10));

        Assert.Equal(80m, price);
    }

    [Fact]
    public void CustomGroup_WithContract_PriceListWinsOverGroup()
    {
        var price = TestPriceResolver.Resolve(
            AccountType.Contracts,
            testPatientPrice: 100m,
            testLabToLabPrice: 50m,
            referralEntity: ReferralWithPriceList(1),
            priceListItems: Items((10, 75m)),
            customGroupItemPrices: Items((10, 80m)),
            testId: TestId.Create(10));

        Assert.Equal(75m, price);
    }
}