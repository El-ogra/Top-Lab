using TopLab.Application.Common.Results;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Queries.GetPriceListById;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.PriceListsCommentsAndCustomGroups;

public class GetPriceListByIdQueryHandlerTests
{
    [Fact]
    public async Task GetPriceListById_Found_ReturnsDetailWithItemsAndJoinedTestNames()
    {
        var db = new FakeApplicationDbContext();
        db.Tests.Add(Test.Create(TestId.Create(1), "CBC", "CBC Report", "CBC Receipt", "CBC01", 60, 100m, ResultKind.Simple, false, null, null, false, null, null, true));
        db.Tests.Add(Test.Create(TestId.Create(2), "Creatinine", "Creatinine Report", "Creatinine Receipt", "CRE01", 45, 80m, ResultKind.Simple, false, null, null, false, null, null, true));
        db.PriceLists.Add(PriceList.Create(PriceListId.Create(1), "Default"));
        db.PriceListItems.Add(new PriceListItem(PriceListId.Create(1), TestId.Create(1), 50m));
        db.PriceListItems.Add(new PriceListItem(PriceListId.Create(1), TestId.Create(2), 75m));

        var handler = new GetPriceListByIdQueryHandler(db);
        var result = await handler.Handle(new GetPriceListByIdQuery(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var dto = result.Value!;
        Assert.Equal("Default", dto.Name);
        Assert.Equal(2, dto.Items.Count);
        Assert.Contains(dto.Items, i => i.TestId == 1 && i.TestName == "CBC" && i.TestCode == "CBC01" && i.Price == 50m);
        Assert.Contains(dto.Items, i => i.TestId == 2 && i.TestName == "Creatinine" && i.TestCode == "CRE01" && i.Price == 75m);
    }

    [Fact]
    public async Task GetPriceListById_NoItems_ReturnsEmptyItems()
    {
        var db = new FakeApplicationDbContext();
        db.PriceLists.Add(PriceList.Create(PriceListId.Create(1), "Empty"));

        var handler = new GetPriceListByIdQueryHandler(db);
        var result = await handler.Handle(new GetPriceListByIdQuery(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Items);
    }

    [Fact]
    public async Task GetPriceListById_Missing_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        var handler = new GetPriceListByIdQueryHandler(db);

        var result = await handler.Handle(new GetPriceListByIdQuery(999), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }
}
