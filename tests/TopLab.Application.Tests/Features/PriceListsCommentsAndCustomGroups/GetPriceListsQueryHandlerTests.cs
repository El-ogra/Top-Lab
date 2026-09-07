using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Queries.GetPriceLists;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.PriceListsCommentsAndCustomGroups;

public class GetPriceListsQueryHandlerTests
{
    [Fact]
    public async Task GetPriceLists_Empty_ReturnsEmpty()
    {
        var db = new FakeApplicationDbContext();
        var handler = new GetPriceListsQueryHandler(db);

        var result = await handler.Handle(new GetPriceListsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }

    [Fact]
    public async Task GetPriceLists_WithItems_ReturnsItemCounts()
    {
        var db = new FakeApplicationDbContext();
        db.PriceLists.Add(PriceList.Create(PriceListId.Create(1), "Default"));
        db.PriceLists.Add(PriceList.Create(PriceListId.Create(2), "Insurance"));
        db.PriceListItems.Add(new PriceListItem(PriceListId.Create(1), TestId.Create(100), 50m));
        db.PriceListItems.Add(new PriceListItem(PriceListId.Create(1), TestId.Create(101), 75m));
        db.PriceListItems.Add(new PriceListItem(PriceListId.Create(1), TestId.Create(102), 100m));
        db.PriceListItems.Add(new PriceListItem(PriceListId.Create(2), TestId.Create(100), 60m));

        var handler = new GetPriceListsQueryHandler(db);
        var result = await handler.Handle(new GetPriceListsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var dtos = result.Value!;
        Assert.Equal(2, dtos.Count);
        Assert.Equal("Default", dtos[0].Name);
        Assert.Equal(3, dtos[0].ItemCount);
        Assert.Equal("Insurance", dtos[1].Name);
        Assert.Equal(1, dtos[1].ItemCount);
    }

    [Fact]
    public async Task GetPriceLists_OrdersByName()
    {
        var db = new FakeApplicationDbContext();
        db.PriceLists.Add(PriceList.Create(PriceListId.Create(1), "Zeta"));
        db.PriceLists.Add(PriceList.Create(PriceListId.Create(2), "Alpha"));
        db.PriceLists.Add(PriceList.Create(PriceListId.Create(3), "Mu"));

        var handler = new GetPriceListsQueryHandler(db);
        var result = await handler.Handle(new GetPriceListsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { "Alpha", "Mu", "Zeta" }, result.Value!.Select(d => d.Name).ToArray());
    }
}
