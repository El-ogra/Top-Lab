using TopLab.Application.Common.Results;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.CreateCustomGroup;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.DeleteCustomGroup;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.RemoveCustomGroupItem;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.RenameCustomGroup;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.SetCustomGroupItemPrice;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.PriceListsCommentsAndCustomGroups;

public class CustomGroupWriteCommandHandlerTests
{
    private static CustomGroup SeedGroup(FakeApplicationDbContext db, int id = 1, string name = "فحص شامل")
    {
        var group = CustomGroup.Create(CustomGroupId.Create(id), name);
        db.CustomGroups.Add(group);
        return group;
    }

    private static Test SeedTest(int id = 1)
    {
        return Test.Create(TestId.Create(id), "CBC", "CBC Report", "CBC Receipt", $"T{id}", 60, 100m, ResultKind.Simple, false, null, null, false, null, null, true);
    }

    // ---------- CreateCustomGroup ----------

    [Fact]
    public async Task CreateCustomGroup_HappyPath_PersistsGroup()
    {
        var db = new FakeApplicationDbContext();
        var handler = new CreateCustomGroupCommandHandler(db);

        var result = await handler.Handle(new CreateCustomGroupCommand("فحص شامل"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(db.CustomGroups);
    }

    [Fact]
    public async Task CreateCustomGroup_DuplicateName_ReturnsConflict()
    {
        var db = new FakeApplicationDbContext();
        SeedGroup(db, id: 1, name: "فحص شامل");
        var handler = new CreateCustomGroupCommandHandler(db);

        var result = await handler.Handle(new CreateCustomGroupCommand("فحص شامل"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Equal("المجموعة موجودة بالفعل", result.Error!.Message);
    }

    // ---------- RenameCustomGroup ----------

    [Fact]
    public async Task RenameCustomGroup_HappyPath_Renames()
    {
        var db = new FakeApplicationDbContext();
        SeedGroup(db, id: 1);
        var handler = new RenameCustomGroupCommandHandler(db);

        var result = await handler.Handle(new RenameCustomGroupCommand(1, "فحص موسع"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("فحص موسع", db.CustomGroups.Single().Name);
    }

    [Fact]
    public async Task RenameCustomGroup_Missing_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        var handler = new RenameCustomGroupCommandHandler(db);

        var result = await handler.Handle(new RenameCustomGroupCommand(999, "X"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }

    [Fact]
    public async Task RenameCustomGroup_DuplicateNameExcludingSelf_ReturnsConflict()
    {
        var db = new FakeApplicationDbContext();
        SeedGroup(db, id: 1, name: "G1");
        SeedGroup(db, id: 2, name: "G2");
        var handler = new RenameCustomGroupCommandHandler(db);

        var result = await handler.Handle(new RenameCustomGroupCommand(1, "G2"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
    }

    // ---------- DeleteCustomGroup ----------

    [Fact]
    public async Task DeleteCustomGroup_HappyPath_Removes()
    {
        var db = new FakeApplicationDbContext();
        SeedGroup(db, id: 1);
        var handler = new DeleteCustomGroupCommandHandler(db);

        var result = await handler.Handle(new DeleteCustomGroupCommand(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(db.CustomGroups);
    }

    [Fact]
    public async Task DeleteCustomGroup_Missing_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        var handler = new DeleteCustomGroupCommandHandler(db);

        var result = await handler.Handle(new DeleteCustomGroupCommand(999), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }

    // ---------- SetCustomGroupItemPrice ----------

    [Fact]
    public async Task SetCustomGroupItemPrice_HappyPath_AddsItem()
    {
        var db = new FakeApplicationDbContext();
        SeedGroup(db, id: 1);
        db.Tests.Add(SeedTest(1));
        var handler = new SetCustomGroupItemPriceCommandHandler(db);

        var result = await handler.Handle(new SetCustomGroupItemPriceCommand(1, 1, 25m), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(db.CustomGroupItems);
    }

    [Fact]
    public async Task SetCustomGroupItemPrice_ExistingItem_UpdatesPrice()
    {
        var db = new FakeApplicationDbContext();
        SeedGroup(db, id: 1);
        db.Tests.Add(SeedTest(1));
        db.CustomGroupItems.Add(new CustomGroupItem(CustomGroupId.Create(1), TestId.Create(1), 10m));
        var handler = new SetCustomGroupItemPriceCommandHandler(db);

        var result = await handler.Handle(new SetCustomGroupItemPriceCommand(1, 1, 30m), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(db.CustomGroupItems);
        Assert.Equal(30m, db.CustomGroupItems.Single().Price);
    }

    [Fact]
    public async Task SetCustomGroupItemPrice_MissingGroup_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        db.Tests.Add(SeedTest(1));
        var handler = new SetCustomGroupItemPriceCommandHandler(db);

        var result = await handler.Handle(new SetCustomGroupItemPriceCommand(999, 1, 25m), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }

    [Fact]
    public async Task SetCustomGroupItemPrice_MissingTest_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        SeedGroup(db, id: 1);
        var handler = new SetCustomGroupItemPriceCommandHandler(db);

        var result = await handler.Handle(new SetCustomGroupItemPriceCommand(1, 999, 25m), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }

    [Fact]
    public async Task SetCustomGroupItemPrice_NegativePrice_ReturnsValidation()
    {
        var db = new FakeApplicationDbContext();
        SeedGroup(db, id: 1);
        db.Tests.Add(SeedTest(1));
        var handler = new SetCustomGroupItemPriceCommandHandler(db);

        var result = await handler.Handle(new SetCustomGroupItemPriceCommand(1, 1, -1m), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
    }

    // ---------- RemoveCustomGroupItem ----------

    [Fact]
    public async Task RemoveCustomGroupItem_HappyPath_RemovesItem()
    {
        var db = new FakeApplicationDbContext();
        SeedGroup(db, id: 1);
        db.Tests.Add(SeedTest(1));
        db.CustomGroupItems.Add(new CustomGroupItem(CustomGroupId.Create(1), TestId.Create(1), 10m));
        var handler = new RemoveCustomGroupItemCommandHandler(db);

        var result = await handler.Handle(new RemoveCustomGroupItemCommand(1, 1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(db.CustomGroupItems);
    }

    [Fact]
    public async Task RemoveCustomGroupItem_AbsentItem_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        SeedGroup(db, id: 1);
        var handler = new RemoveCustomGroupItemCommandHandler(db);

        var result = await handler.Handle(new RemoveCustomGroupItemCommand(1, 999), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("التحليل غير موجود في المجموعة.", result.Error!.Message);
    }

    [Fact]
    public async Task RemoveCustomGroupItem_MissingGroup_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        var handler = new RemoveCustomGroupItemCommandHandler(db);

        var result = await handler.Handle(new RemoveCustomGroupItemCommand(999, 1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }
}
