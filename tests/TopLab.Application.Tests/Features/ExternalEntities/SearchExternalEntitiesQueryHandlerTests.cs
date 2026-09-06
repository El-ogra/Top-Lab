using TopLab.Application.Features.ExternalEntities.Queries.SearchExternalEntities;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.ExternalEntities;
using Xunit;

namespace TopLab.Application.Tests.Features.ExternalEntities;

public class SearchExternalEntitiesQueryHandlerTests
{
    private static FakeApplicationDbContext BuildDb()
    {
        var db = new FakeApplicationDbContext();
        db.Add(PriceList.Create(PriceListId.Create(1), "Standard"));

        db.Add(ExternalEntity.Create(
            ExternalEntityId.Create(1), EntityType.TreatingDoctor, "Dr. Ahmed",
            city: "Cairo", phone: "0100111222", discountOrCommissionPercent: 10m));
        db.Add(ExternalEntity.Create(
            ExternalEntityId.Create(2), EntityType.ReferralOrContract, "Delta Contract",
            city: "Tanta", phone: "0403334444", priceListId: PriceListId.Create(1)));
        var lab = ExternalEntity.Create(
            ExternalEntityId.Create(3), EntityType.PartnerLab, "Alpha Lab",
            city: "Alexandria", phone: "035556666", priceListId: PriceListId.Create(1));
        lab.RegenerateIdCode("LAB001");
        db.Add(lab);
        return db;
    }

    [Fact]
    public async Task Search_ByType_ReturnsOnlyRequestedType()
    {
        var handler = new SearchExternalEntitiesQueryHandler(BuildDb());

        var result = await handler.Handle(
            new SearchExternalEntitiesQuery(EntityType.TreatingDoctor, null, 1, 10), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Value!);
        Assert.Equal("Dr. Ahmed", item.Name);
        Assert.Equal(EntityType.TreatingDoctor, item.EntityType);
        Assert.Equal("Cairo", item.City);
        Assert.Equal("0100111222", item.Phone);
        Assert.Null(item.PriceListId);
        Assert.Null(item.PriceListName);
        Assert.Equal(10m, item.DiscountOrCommissionPercent);
        Assert.Null(item.GeneratedIdCode);
    }

    [Fact]
    public async Task Search_PartialName_ReturnsMatches()
    {
        var handler = new SearchExternalEntitiesQueryHandler(BuildDb());

        var result = await handler.Handle(
            new SearchExternalEntitiesQuery(null, "Delta", 1, 10), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Value!);
        Assert.Equal("Delta Contract", item.Name);
        Assert.Equal(1, item.PriceListId);
        Assert.Equal("Standard", item.PriceListName);
    }

    [Fact]
    public async Task Search_PartialCity_ReturnsMatches()
    {
        var handler = new SearchExternalEntitiesQueryHandler(BuildDb());

        var result = await handler.Handle(
            new SearchExternalEntitiesQuery(null, "Cairo", 1, 10), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
    }

    [Fact]
    public async Task Search_PartialPhone_ReturnsMatches()
    {
        var handler = new SearchExternalEntitiesQueryHandler(BuildDb());

        var result = await handler.Handle(
            new SearchExternalEntitiesQuery(null, "040333", 1, 10), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Value!);
        Assert.Equal("Delta Contract", item.Name);
    }

    [Fact]
    public async Task Search_PartialCode_ReturnsMatches()
    {
        var handler = new SearchExternalEntitiesQueryHandler(BuildDb());

        var result = await handler.Handle(
            new SearchExternalEntitiesQuery(null, "LAB", 1, 10), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Value!);
        Assert.Equal("Alpha Lab", item.Name);
        Assert.Equal("LAB001", item.GeneratedIdCode);
    }

    [Fact]
    public async Task Search_EmptyTerm_ReturnsAll_OrderedByName()
    {
        var handler = new SearchExternalEntitiesQueryHandler(BuildDb());

        var result = await handler.Handle(
            new SearchExternalEntitiesQuery(null, "   ", 1, 10), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value!.Count);
        Assert.Equal("Alpha Lab", result.Value[0].Name);
        Assert.Equal("Delta Contract", result.Value[1].Name);
        Assert.Equal("Dr. Ahmed", result.Value[2].Name);
    }

    [Fact]
    public async Task Search_Paging_ReturnsRequestedPage()
    {
        var handler = new SearchExternalEntitiesQueryHandler(BuildDb());

        var page1 = await handler.Handle(
            new SearchExternalEntitiesQuery(null, null, 1, 2), CancellationToken.None);
        var page2 = await handler.Handle(
            new SearchExternalEntitiesQuery(null, null, 2, 2), CancellationToken.None);

        Assert.True(page1.IsSuccess);
        Assert.True(page2.IsSuccess);
        Assert.Equal(2, page1.Value!.Count);
        var single = Assert.Single(page2.Value!);
        Assert.Equal("Dr. Ahmed", single.Name);
    }

    [Fact]
    public async Task Search_NoMatch_ReturnsEmpty()
    {
        var handler = new SearchExternalEntitiesQueryHandler(BuildDb());

        var result = await handler.Handle(
            new SearchExternalEntitiesQuery(null, "Zzz", 1, 10), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public void Validator_InvalidPaging_Fails(int page, int pageSize)
    {
        var validator = new SearchExternalEntitiesQueryValidator();

        var result = validator.Validate(new SearchExternalEntitiesQuery(null, null, page, pageSize));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validator_UnknownEntityType_Fails()
    {
        var validator = new SearchExternalEntitiesQueryValidator();

        var result = validator.Validate(new SearchExternalEntitiesQuery((EntityType)99, null, 1, 10));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validator_ValidQuery_Passes()
    {
        var validator = new SearchExternalEntitiesQueryValidator();

        var result = validator.Validate(new SearchExternalEntitiesQuery(EntityType.PartnerLab, "Alpha", 1, 10));

        Assert.True(result.IsValid);
    }
}
