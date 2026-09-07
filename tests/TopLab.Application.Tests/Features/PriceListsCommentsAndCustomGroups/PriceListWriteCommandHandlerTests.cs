using TopLab.Application.Common.Results;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.CreatePriceList;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.DeletePriceList;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.RemovePriceListItem;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.RenamePriceList;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.SetPriceListItemPrice;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.ExternalEntities;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.PriceListsCommentsAndCustomGroups;

public class PriceListWriteCommandHandlerTests
{
    private static PriceList SeedList(FakeApplicationDbContext db, int id = 1, string name = "قائمة افتراضية")
    {
        var list = PriceList.Create(PriceListId.Create(id), name);
        db.PriceLists.Add(list);
        return list;
    }

    private static Test SeedTest(int id = 1)
    {
        return Test.Create(TestId.Create(id), "CBC", "CBC Report", "CBC Receipt", $"T{id}", 60, 100m, ResultKind.Simple, false, null, null, false, null, null, true);
    }

    // ---------- CreatePriceList ----------

    [Fact]
    public async Task CreatePriceList_HappyPath_PersistsList()
    {
        var db = new FakeApplicationDbContext();
        var handler = new CreatePriceListCommandHandler(db);

        var result = await handler.Handle(new CreatePriceListCommand("قائمة افتراضية"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(db.PriceLists);
    }

    [Fact]
    public async Task CreatePriceList_DuplicateName_ReturnsConflict()
    {
        var db = new FakeApplicationDbContext();
        SeedList(db, id: 1, name: "قائمة افتراضية");
        var handler = new CreatePriceListCommandHandler(db);

        var result = await handler.Handle(new CreatePriceListCommand("قائمة افتراضية"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Equal("قائمة أسعار موجودة بالفعل", result.Error!.Message);
    }

    // ---------- RenamePriceList ----------

    [Fact]
    public async Task RenamePriceList_HappyPath_Renames()
    {
        var db = new FakeApplicationDbContext();
        SeedList(db, id: 1);
        var handler = new RenamePriceListCommandHandler(db);

        var result = await handler.Handle(new RenamePriceListCommand(1, "قائمة التأمين"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("قائمة التأمين", db.PriceLists.Single().Name);
    }

    [Fact]
    public async Task RenamePriceList_Missing_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        var handler = new RenamePriceListCommandHandler(db);

        var result = await handler.Handle(new RenamePriceListCommand(999, "X"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }

    [Fact]
    public async Task RenamePriceList_DuplicateNameExcludingSelf_ReturnsConflict()
    {
        var db = new FakeApplicationDbContext();
        SeedList(db, id: 1, name: "قائمة 1");
        SeedList(db, id: 2, name: "قائمة 2");
        var handler = new RenamePriceListCommandHandler(db);

        var result = await handler.Handle(new RenamePriceListCommand(1, "قائمة 2"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
    }

    // ---------- DeletePriceList ----------

    [Fact]
    public async Task DeletePriceList_HappyPath_Removes()
    {
        var db = new FakeApplicationDbContext();
        SeedList(db, id: 1);
        var handler = new DeletePriceListCommandHandler(db);

        var result = await handler.Handle(new DeletePriceListCommand(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(db.PriceLists);
    }

    [Fact]
    public async Task DeletePriceList_Missing_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        var handler = new DeletePriceListCommandHandler(db);

        var result = await handler.Handle(new DeletePriceListCommand(999), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }

    [Fact]
    public async Task DeletePriceList_ReferencedByExternalEntity_ReturnsConflict()
    {
        var db = new FakeApplicationDbContext();
        SeedList(db, id: 1);
        db.ExternalEntities.Add(ExternalEntity.Create(
            ExternalEntityId.Create(1),
            EntityType.ReferralOrContract,
            "Lab A",
            null, null, null, null, null, null,
            PriceListId.Create(1),
            0m));
        var handler = new DeletePriceListCommandHandler(db);

        var result = await handler.Handle(new DeletePriceListCommand(1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Equal("تعذر حذف قائمة الأسعار لارتباطها بجهات خارجية.", result.Error!.Message);
        Assert.Single(db.PriceLists);
    }

    [Fact]
    public async Task DeletePriceList_NotReferencedByExternalEntity_Succeeds()
    {
        var db = new FakeApplicationDbContext();
        SeedList(db, id: 1);
        db.ExternalEntities.Add(ExternalEntity.Create(
            ExternalEntityId.Create(1),
            EntityType.TreatingDoctor,
            "Doctor A",
            null, null, null, null, null, null,
            null,
            0m));
        var handler = new DeletePriceListCommandHandler(db);

        var result = await handler.Handle(new DeletePriceListCommand(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    // ---------- SetPriceListItemPrice ----------

    [Fact]
    public async Task SetPriceListItemPrice_HappyPath_AddsItem()
    {
        var db = new FakeApplicationDbContext();
        SeedList(db, id: 1);
        db.Tests.Add(SeedTest(1));
        var handler = new SetPriceListItemPriceCommandHandler(db);

        var result = await handler.Handle(new SetPriceListItemPriceCommand(1, 1, 25m), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(db.PriceListItems);
        Assert.Equal(25m, db.PriceListItems.Single().Price);
    }

    [Fact]
    public async Task SetPriceListItemPrice_ExistingItem_UpdatesPrice()
    {
        var db = new FakeApplicationDbContext();
        SeedList(db, id: 1);
        db.Tests.Add(SeedTest(1));
        db.PriceListItems.Add(new PriceListItem(PriceListId.Create(1), TestId.Create(1), 10m));
        var handler = new SetPriceListItemPriceCommandHandler(db);

        var result = await handler.Handle(new SetPriceListItemPriceCommand(1, 1, 30m), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(db.PriceListItems);
        Assert.Equal(30m, db.PriceListItems.Single().Price);
    }

    [Fact]
    public async Task SetPriceListItemPrice_MissingList_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        db.Tests.Add(SeedTest(1));
        var handler = new SetPriceListItemPriceCommandHandler(db);

        var result = await handler.Handle(new SetPriceListItemPriceCommand(999, 1, 25m), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }

    [Fact]
    public async Task SetPriceListItemPrice_MissingTest_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        SeedList(db, id: 1);
        var handler = new SetPriceListItemPriceCommandHandler(db);

        var result = await handler.Handle(new SetPriceListItemPriceCommand(1, 999, 25m), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }

    [Fact]
    public async Task SetPriceListItemPrice_NegativePrice_ReturnsValidation()
    {
        var db = new FakeApplicationDbContext();
        SeedList(db, id: 1);
        db.Tests.Add(SeedTest(1));
        var handler = new SetPriceListItemPriceCommandHandler(db);

        var result = await handler.Handle(new SetPriceListItemPriceCommand(1, 1, -1m), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
        Assert.Equal("السعر يجب أن يكون صفرًا أو أكثر.", result.Error!.Message);
    }

    // ---------- RemovePriceListItem ----------

    [Fact]
    public async Task RemovePriceListItem_HappyPath_RemovesItem()
    {
        var db = new FakeApplicationDbContext();
        SeedList(db, id: 1);
        db.Tests.Add(SeedTest(1));
        db.PriceListItems.Add(new PriceListItem(PriceListId.Create(1), TestId.Create(1), 10m));
        var handler = new RemovePriceListItemCommandHandler(db);

        var result = await handler.Handle(new RemovePriceListItemCommand(1, 1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(db.PriceListItems);
    }

    [Fact]
    public async Task RemovePriceListItem_AbsentItem_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        SeedList(db, id: 1);
        var handler = new RemovePriceListItemCommandHandler(db);

        var result = await handler.Handle(new RemovePriceListItemCommand(1, 999), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("التحليل غير موجود في القائمة.", result.Error!.Message);
    }

    [Fact]
    public async Task RemovePriceListItem_MissingList_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        var handler = new RemovePriceListItemCommandHandler(db);

        var result = await handler.Handle(new RemovePriceListItemCommand(999, 1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }
}
