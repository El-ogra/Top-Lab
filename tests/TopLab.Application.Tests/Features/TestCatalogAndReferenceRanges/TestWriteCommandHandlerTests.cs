using TopLab.Application.Common.Results;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.CreateTest;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.DeactivateTest;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.ReactivateTest;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.UpdateTest;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;

namespace TopLab.Application.Tests.Features.TestCatalogAndReferenceRanges;

public class TestWriteCommandHandlerTests
{
    private static Test SeedTest(FakeApplicationDbContext db, int id = 1, string code = "CBC", bool isActive = true)
    {
        var test = Test.Create(TestId.Create(id), "تحليل عام", "تقرير تحليل عام", "إيصال تحليل عام", code, 30, 100m, isActive: isActive);
        db.Tests.Add(test);
        return test;
    }

    // ---------- CreateTest ----------

    [Fact]
    public async Task CreateTest_HappyPath_PersistsTest()
    {
        var db = new FakeApplicationDbContext();
        var handler = new CreateTestCommandHandler(db);

        var cmd = new CreateTestCommand(
            "تحليل عام", "تقرير تحليل عام", "إيصال تحليل عام", "CBC",
            30, 150m, ResultKind.Simple, false, null, null, false, null, null);

        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var saved = db.Tests.Single();
        Assert.Equal("CBC", saved.TestCode);
        Assert.Equal(150m, saved.PatientPrice);
        Assert.Equal("تحليل عام", saved.Name);
        Assert.True(saved.IsActive);
    }

    [Fact]
    public async Task CreateTest_DuplicateTestCode_ReturnsConflict()
    {
        var db = new FakeApplicationDbContext();
        SeedTest(db, id: 1, code: "CBC");
        var handler = new CreateTestCommandHandler(db);

        var cmd = new CreateTestCommand(
            "أخرى", "تقرير", "إيصال", "CBC",
            30, 150m, ResultKind.Simple, false, null, null, false, null, null);

        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Equal("كود التحليل مستخدم بالفعل", result.Error!.Message);
    }

    [Fact]
    public async Task CreateTest_DuplicateName_ReturnsConflict()
    {
        var db = new FakeApplicationDbContext();
        SeedTest(db, id: 1, code: "CBC", isActive: true);
        var handler = new CreateTestCommandHandler(db);

        var cmd = new CreateTestCommand(
            "تحليل عام", "تقرير", "إيصال", "CBC2",
            30, 150m, ResultKind.Simple, false, null, null, false, null, null);

        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
    }

    [Fact]
    public async Task CreateTest_UnknownTestGroup_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        var handler = new CreateTestCommandHandler(db);

        var cmd = new CreateTestCommand(
            "تحليل عام", "تقرير", "إيصال", "CBC",
            30, 150m, ResultKind.Simple, false, 99, null, false, null, null);

        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("مجموعة التحاليل غير موجودة", result.Error!.Message);
    }

    [Fact]
    public async Task CreateTest_SavesSentOutCostPrice_WhenSentOut()
    {
        var db = new FakeApplicationDbContext();
        var handler = new CreateTestCommandHandler(db);

        var cmd = new CreateTestCommand(
            "تحليل عام", "تقرير", "إيصال", "CBC",
            30, 150m, ResultKind.Simple, false, null, null, true, 500m, null);

        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var saved = db.Tests.Single();
        Assert.True(saved.IsSentOut);
        Assert.Equal(500m, saved.SentOutCostPrice);
    }

    [Fact]
    public async Task CreateTest_UniqueViolationOnSave_ReturnsConflict()
    {
        var db = new UniqueViolationFakeApplicationDbContext { ThrowUniqueViolation = true };
        var handler = new CreateTestCommandHandler(db);

        var cmd = new CreateTestCommand(
            "تحليل عام", "تقرير", "إيصال", "CBC",
            30, 150m, ResultKind.Simple, false, null, null, false, null, null);

        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Equal("كود التحليل مستخدم بالفعل", result.Error!.Message);
    }

    // ---------- UpdateTest ----------

    [Fact]
    public async Task UpdateTest_HappyPath_UpdatesInPlace()
    {
        var db = new FakeApplicationDbContext();
        SeedTest(db, id: 1, code: "CBC");
        var handler = new UpdateTestCommandHandler(db);

        var cmd = new UpdateTestCommand(
            1, "تحليل معدل", "تقرير معدل", "إيصال معدل", "CBC2",
            45, 200m, null, "0099", false, null, null);

        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var saved = db.Tests.Single(t => t.Id.Value == 1);
        Assert.Equal("تحليل معدل", saved.Name);
        Assert.Equal("CBC2", saved.TestCode);
        Assert.Equal(45, saved.CompletionDurationMinutes);
        Assert.Equal(200m, saved.PatientPrice);
        Assert.Equal("0099", saved.Barcode);
    }

    [Fact]
    public async Task UpdateTest_NotFound_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        var handler = new UpdateTestCommandHandler(db);

        var cmd = new UpdateTestCommand(
            999, "تحليل معدل", "تقرير", "إيصال", "CBC2",
            45, 200m, null, null, false, null, null);

        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("التحليل غير موجود", result.Error!.Message);
    }

    [Fact]
    public async Task UpdateTest_UnknownTestGroup_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        SeedTest(db, id: 1);
        var handler = new UpdateTestCommandHandler(db);

        var cmd = new UpdateTestCommand(
            1, "تحليل معدل", "تقرير", "إيصال", "CBC2",
            45, 200m, 99, null, false, null, null);

        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }

    [Fact]
    public async Task UpdateTest_UniqueViolationOnSave_ReturnsConflict()
    {
        var db = new UniqueViolationFakeApplicationDbContext { ThrowUniqueViolation = true };
        SeedTest(db.Inner, id: 1, code: "CBC");
        var handler = new UpdateTestCommandHandler(db);

        var cmd = new UpdateTestCommand(
            1, "تحليل معدل", "تقرير", "إيصال", "CBC-DUP",
            45, 200m, null, null, false, null, null);

        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
    }

    // ---------- DeactivateTest ----------

    [Fact]
    public async Task DeactivateTest_HappyPath_SetsInactive()
    {
        var db = new FakeApplicationDbContext();
        SeedTest(db, id: 1);
        var handler = new DeactivateTestCommandHandler(db);

        var result = await handler.Handle(new DeactivateTestCommand(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(db.Tests.Single(t => t.Id.Value == 1).IsActive);
    }

    [Fact]
    public async Task DeactivateTest_AlreadyInactive_ReturnsConflict()
    {
        var db = new FakeApplicationDbContext();
        SeedTest(db, id: 1, isActive: false);
        var handler = new DeactivateTestCommandHandler(db);

        var result = await handler.Handle(new DeactivateTestCommand(1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Equal("التحليل غير نشط بالفعل", result.Error!.Message);
    }

    [Fact]
    public async Task DeactivateTest_NotFound_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        var handler = new DeactivateTestCommandHandler(db);

        var result = await handler.Handle(new DeactivateTestCommand(999), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }

    // ---------- ReactivateTest ----------

    [Fact]
    public async Task ReactivateTest_HappyPath_SetsActive()
    {
        var db = new FakeApplicationDbContext();
        SeedTest(db, id: 1, isActive: false);
        var handler = new ReactivateTestCommandHandler(db);

        var result = await handler.Handle(new ReactivateTestCommand(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(db.Tests.Single(t => t.Id.Value == 1).IsActive);
    }

    [Fact]
    public async Task ReactivateTest_AlreadyActive_ReturnsConflict()
    {
        var db = new FakeApplicationDbContext();
        SeedTest(db, id: 1, isActive: true);
        var handler = new ReactivateTestCommandHandler(db);

        var result = await handler.Handle(new ReactivateTestCommand(1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Equal("التحليل نشط بالفعل", result.Error!.Message);
    }

    [Fact]
    public async Task ReactivateTest_NotFound_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        var handler = new ReactivateTestCommandHandler(db);

        var result = await handler.Handle(new ReactivateTestCommand(999), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }
}