using Microsoft.EntityFrameworkCore;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.SetPriceListItemPrice;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.RemovePriceListItem;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;
using TopLab.Infrastructure.Persistence;
using TopLab.Infrastructure.Tests.Common;
using Xunit;

namespace TopLab.Infrastructure.Tests.Persistence;

public class PriceListItemPersistenceTests
{
    private static (IApplicationDbContext db, ApplicationDbContext concrete) BuildContextWithTestAndList()
    {
        var options = InMemoryContextFactory.Create();
        var concrete = new ApplicationDbContext(options);
        var test = Test.Create(TestId.Create(1), "CBC", "CBC Report", "CBC Receipt", "CBC01", 60, 100m, ResultKind.Simple, false, null, null, false, null, null, true);
        concrete.Tests.Add(test);
        var list = PriceList.Create(PriceListId.Create(1), "Default");
        concrete.PriceLists.Add(list);
        concrete.SaveChanges();
        return (concrete, concrete);
    }

    [Fact]
    public async Task SetPriceListItemPrice_OnEmptyList_InsertsOneRow()
    {
        var (db, concrete) = BuildContextWithTestAndList();
        var handler = new SetPriceListItemPriceCommandHandler(db);

        var result = await handler.Handle(new SetPriceListItemPriceCommand(1, 1, 25m), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var rows = concrete.PriceListItems.ToList();
        Assert.Single(rows);
        Assert.Equal(25m, rows[0].Price);
    }

    [Fact]
    public async Task SetPriceListItemPrice_CalledTwice_UpdatesSameRow_NoDoubleTracking()
    {
        var (db, concrete) = BuildContextWithTestAndList();
        var handler = new SetPriceListItemPriceCommandHandler(db);

        var first = await handler.Handle(new SetPriceListItemPriceCommand(1, 1, 10m), CancellationToken.None);
        var second = await handler.Handle(new SetPriceListItemPriceCommand(1, 1, 30m), CancellationToken.None);

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        var rows = concrete.PriceListItems.ToList();
        Assert.Single(rows);
        Assert.Equal(30m, rows[0].Price);
    }

    [Fact]
    public async Task RemovePriceListItem_DeletesExistingRow()
    {
        var (db, concrete) = BuildContextWithTestAndList();
        concrete.PriceListItems.Add(new PriceListItem(PriceListId.Create(1), TestId.Create(1), 10m));
        concrete.SaveChanges();

        var handler = new RemovePriceListItemCommandHandler(db);
        var result = await handler.Handle(new RemovePriceListItemCommand(1, 1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(concrete.PriceListItems.ToList());
    }

    [Fact]
    public async Task SetPriceListItemPrice_NegativePrice_ReturnsValidation()
    {
        var (db, concrete) = BuildContextWithTestAndList();
        var handler = new SetPriceListItemPriceCommandHandler(db);

        var result = await handler.Handle(new SetPriceListItemPriceCommand(1, 1, -1m), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
    }
}
