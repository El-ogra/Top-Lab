using TopLab.Application.Common.Results;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.CreateWorkGroupLog;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.RenameWorkGroupLog;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.SaveWorkGroupLogItems;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;

namespace TopLab.Application.Tests.Features.TestCatalogAndReferenceRanges;

public class WorkGroupLogWriteCommandHandlerTests
{
    private static WorkGroupLog SeedLog(FakeApplicationDbContext db, int id = 1, string name = "ورشة المناعة")
    {
        var log = WorkGroupLog.Create(WorkGroupLogId.Create(id), name);
        db.WorkGroupLogs.Add(log);
        return log;
    }

    private static void SeedTest(FakeApplicationDbContext db, int id)
    {
        db.Tests.Add(Test.Create(TestId.Create(id), $"تحليل {id}", "تقرير", "إيصال", $"T{id}", 30, 100m));
    }

    private static void SeedItem(FakeApplicationDbContext db, WorkGroupLogId logId, TestId testId)
    {
        db.WorkGroupLogItems.Add(WorkGroupLogItem.Create(logId, testId));
    }

    // ---------- CreateWorkGroupLog ----------

    [Fact]
    public async Task CreateWorkGroupLog_HappyPath_PersistsLog()
    {
        var db = new FakeApplicationDbContext();
        var handler = new CreateWorkGroupLogCommandHandler(db);

        var result = await handler.Handle(new CreateWorkGroupLogCommand("ورشة المناعة"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("ورشة المناعة", db.WorkGroupLogs.Single().Name);
    }

    [Fact]
    public async Task CreateWorkGroupLog_DuplicateName_ReturnsConflict()
    {
        var db = new FakeApplicationDbContext();
        SeedLog(db, id: 1, name: "ورشة المناعة");
        var handler = new CreateWorkGroupLogCommandHandler(db);

        var result = await handler.Handle(new CreateWorkGroupLogCommand("ورشة المناعة"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Equal("مجموعة العمل موجودة بالفعل", result.Error!.Message);
    }

    // ---------- RenameWorkGroupLog ----------

    [Fact]
    public async Task RenameWorkGroupLog_HappyPath_Renames()
    {
        var db = new FakeApplicationDbContext();
        SeedLog(db, id: 1);
        var handler = new RenameWorkGroupLogCommandHandler(db);

        var result = await handler.Handle(new RenameWorkGroupLogCommand(1, "ورشة الكيمياء"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("ورشة الكيمياء", db.WorkGroupLogs.Single(l => l.Id.Value == 1).Name);
    }

    [Fact]
    public async Task RenameWorkGroupLog_NotFound_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        var handler = new RenameWorkGroupLogCommandHandler(db);

        var result = await handler.Handle(new RenameWorkGroupLogCommand(999, "ورشة الكيمياء"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("مجموعة العمل غير موجودة", result.Error!.Message);
    }

    // ---------- SaveWorkGroupLogItems (atomic replace) ----------

    [Fact]
    public async Task SaveWorkGroupLogItems_HappyPath_AtomicallyReplacesRows()
    {
        var db = new FakeApplicationDbContext();
        var log = SeedLog(db, id: 1);
        SeedTest(db, 1);
        SeedTest(db, 2);
        SeedTest(db, 3);
        SeedItem(db, log.Id, TestId.Create(1));
        SeedItem(db, log.Id, TestId.Create(2));
        var handler = new SaveWorkGroupLogItemsCommandHandler(db);

        var result = await handler.Handle(new SaveWorkGroupLogItemsCommand(1, new[] { 2, 3 }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, db.SaveChangesCallCount);
        Assert.Equal(2, db.WorkGroupLogItems.Count);
        Assert.DoesNotContain(db.WorkGroupLogItems, i => i.TestId.Value == 1);
        Assert.Contains(db.WorkGroupLogItems, i => i.TestId.Value == 2);
        Assert.Contains(db.WorkGroupLogItems, i => i.TestId.Value == 3);
        Assert.Equal(new[] { 2, 3 }, log.Items.Select(i => i.TestId.Value).OrderBy(v => v));
    }

    [Fact]
    public async Task SaveWorkGroupLogItems_UnknownTestId_ReturnsValidation()
    {
        var db = new FakeApplicationDbContext();
        SeedLog(db, id: 1);
        SeedTest(db, 1);
        var handler = new SaveWorkGroupLogItemsCommandHandler(db);

        var result = await handler.Handle(new SaveWorkGroupLogItemsCommand(1, new[] { 1, 99 }), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
        Assert.Equal("معرف التحليل غير معروف: 99", result.Error!.Message);
        Assert.Equal(0, db.SaveChangesCallCount);
        Assert.Empty(db.WorkGroupLogItems);
    }

    [Fact]
    public async Task SaveWorkGroupLogItems_LogNotFound_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        var handler = new SaveWorkGroupLogItemsCommandHandler(db);

        var result = await handler.Handle(new SaveWorkGroupLogItemsCommand(999, new[] { 1 }), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }
}