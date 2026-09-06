using TopLab.Application.Common.Results;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.CreateTestGroup;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.DeactivateTestGroup;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.ReactivateTestGroup;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.UpdateTestGroup;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;

namespace TopLab.Application.Tests.Features.TestCatalogAndReferenceRanges;

public class TestGroupWriteCommandHandlerTests
{
    private static TestGroup SeedGroup(FakeApplicationDbContext db, int id = 1, string name = "الهيماتولوجيا", bool isActive = true)
    {
        var group = TestGroup.Create(TestGroupId.Create(id), name, isActive);
        db.TestGroups.Add(group);
        return group;
    }

    private static Test CreateInGroup(int id, TestGroupId groupId, bool isActive = true)
    {
        return Test.Create(TestId.Create(id), $"تحليل {id}", "تقرير", "إيصال", $"T{id}", 30, 100m, testGroupId: groupId, isActive: isActive);
    }

    // ---------- CreateTestGroup ----------

    [Fact]
    public async Task CreateTestGroup_HappyPath_PersistsGroup()
    {
        var db = new FakeApplicationDbContext();
        var handler = new CreateTestGroupCommandHandler(db);

        var result = await handler.Handle(new CreateTestGroupCommand("الهيماتولوجيا"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var saved = db.TestGroups.Single();
        Assert.Equal("الهيماتولوجيا", saved.Name);
        Assert.True(saved.IsActive);
    }

    [Fact]
    public async Task CreateTestGroup_DuplicateName_ReturnsConflict()
    {
        var db = new FakeApplicationDbContext();
        SeedGroup(db, id: 1, name: "الهيماتولوجيا");
        var handler = new CreateTestGroupCommandHandler(db);

        var result = await handler.Handle(new CreateTestGroupCommand("الهيماتولوجيا"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Equal("مجموعة التحاليل موجودة بالفعل", result.Error!.Message);
    }

    // ---------- UpdateTestGroup ----------

    [Fact]
    public async Task UpdateTestGroup_HappyPath_Renames()
    {
        var db = new FakeApplicationDbContext();
        SeedGroup(db, id: 1);
        var handler = new UpdateTestGroupCommandHandler(db);

        var result = await handler.Handle(new UpdateTestGroupCommand(1, "الكيمياء"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("الكيمياء", db.TestGroups.Single(g => g.Id.Value == 1).Name);
    }

    [Fact]
    public async Task UpdateTestGroup_NotFound_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        var handler = new UpdateTestGroupCommandHandler(db);

        var result = await handler.Handle(new UpdateTestGroupCommand(999, "الكيمياء"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("مجموعة التحاليل غير موجودة", result.Error!.Message);
    }

    // ---------- DeactivateTestGroup (cascade) ----------

    [Fact]
    public async Task DeactivateTestGroup_HappyPath_CascadesToActiveMembers()
    {
        var db = new FakeApplicationDbContext();
        var group = SeedGroup(db, id: 1);
        db.Tests.Add(CreateInGroup(1, group.Id, isActive: true));
        db.Tests.Add(CreateInGroup(2, group.Id, isActive: false));
        db.Tests.Add(CreateInGroup(3, null!, isActive: true));
        var handler = new DeactivateTestGroupCommandHandler(db);

        var result = await handler.Handle(new DeactivateTestGroupCommand(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(db.TestGroups.Single(g => g.Id.Value == 1).IsActive);
        Assert.False(db.Tests.Single(t => t.Id.Value == 1).IsActive);
        Assert.False(db.Tests.Single(t => t.Id.Value == 2).IsActive);
        Assert.True(db.Tests.Single(t => t.Id.Value == 3).IsActive);
        Assert.Equal(1, db.SaveChangesCallCount);
    }

    [Fact]
    public async Task DeactivateTestGroup_AlreadyInactive_ReturnsConflict()
    {
        var db = new FakeApplicationDbContext();
        SeedGroup(db, id: 1, isActive: false);
        var handler = new DeactivateTestGroupCommandHandler(db);

        var result = await handler.Handle(new DeactivateTestGroupCommand(1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Equal("المجموعة غير نشطة بالفعل", result.Error!.Message);
        Assert.Equal(0, db.SaveChangesCallCount);
    }

    [Fact]
    public async Task DeactivateTestGroup_NotFound_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        var handler = new DeactivateTestGroupCommandHandler(db);

        var result = await handler.Handle(new DeactivateTestGroupCommand(999), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }

    // ---------- ReactivateTestGroup (non-cascading) ----------

    [Fact]
    public async Task ReactivateTestGroup_HappyPath_MembersKeepInactiveState()
    {
        var db = new FakeApplicationDbContext();
        var group = SeedGroup(db, id: 1, isActive: false);
        db.Tests.Add(CreateInGroup(1, group.Id, isActive: true));
        db.Tests.Add(CreateInGroup(2, group.Id, isActive: false));
        var handler = new ReactivateTestGroupCommandHandler(db);

        var result = await handler.Handle(new ReactivateTestGroupCommand(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(db.TestGroups.Single(g => g.Id.Value == 1).IsActive);
        Assert.True(db.Tests.Single(t => t.Id.Value == 1).IsActive);
        Assert.False(db.Tests.Single(t => t.Id.Value == 2).IsActive);
    }

    [Fact]
    public async Task ReactivateTestGroup_AlreadyActive_ReturnsConflict()
    {
        var db = new FakeApplicationDbContext();
        SeedGroup(db, id: 1, isActive: true);
        var handler = new ReactivateTestGroupCommandHandler(db);

        var result = await handler.Handle(new ReactivateTestGroupCommand(1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Equal("المجموعة نشطة بالفعل", result.Error!.Message);
    }

    [Fact]
    public async Task ReactivateTestGroup_NotFound_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        var handler = new ReactivateTestGroupCommandHandler(db);

        var result = await handler.Handle(new ReactivateTestGroupCommand(999), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }
}