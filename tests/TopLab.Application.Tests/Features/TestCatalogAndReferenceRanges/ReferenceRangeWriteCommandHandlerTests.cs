using TopLab.Application.Common.Results;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.CreateReferenceRange;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.DeleteReferenceRange;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.UpdateReferenceRange;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;

namespace TopLab.Application.Tests.Features.TestCatalogAndReferenceRanges;

public class ReferenceRangeWriteCommandHandlerTests
{
    private static void SeedTest(FakeApplicationDbContext db, int id = 1)
    {
        db.Tests.Add(Test.Create(TestId.Create(id), $"تحليل {id}", "تقرير", "إيصال", $"T{id}", 30, 100m));
    }

    private static ReferenceRange SeedRange(FakeApplicationDbContext db, int id = 1, int testId = 1)
    {
        var range = ReferenceRange.Create(ReferenceRangeId.Create(id), TestId.Create(testId), AgeUnit.Year, 0, 120, 0m, 100m);
        db.ReferenceRanges.Add(range);
        return range;
    }

    // ---------- CreateReferenceRange ----------

    [Fact]
    public async Task CreateReferenceRange_HappyPath_PersistsRange()
    {
        var db = new FakeApplicationDbContext();
        SeedTest(db, 1);
        var handler = new CreateReferenceRangeCommandHandler(db);

        var cmd = new CreateReferenceRangeCommand(1, Sex.Female, AgeUnit.Year, 18, 65, 3.0m, 5.0m, "تعليق منخفض", "تعليق مرتفع");

        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var saved = db.ReferenceRanges.Single();
        Assert.Equal(1, saved.TestId.Value);
        Assert.Equal(Sex.Female, saved.Sex);
        Assert.Equal(AgeUnit.Year, saved.AgeUnit);
        Assert.Equal(18, saved.AgeMin);
        Assert.Equal(65, saved.AgeMax);
        Assert.Equal(3.0m, saved.MinValue);
        Assert.Equal(5.0m, saved.MaxValue);
    }

    [Fact]
    public async Task CreateReferenceRange_UnknownTest_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        var handler = new CreateReferenceRangeCommandHandler(db);

        var cmd = new CreateReferenceRangeCommand(999, null, AgeUnit.Year, 0, 120, 0m, 100m, null, null);

        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("التحليل غير موجود", result.Error!.Message);
    }

    // ---------- UpdateReferenceRange ----------

    [Fact]
    public async Task UpdateReferenceRange_HappyPath_UpdatesInPlace()
    {
        var db = new FakeApplicationDbContext();
        SeedRange(db, id: 1, testId: 1);
        var handler = new UpdateReferenceRangeCommandHandler(db);

        var cmd = new UpdateReferenceRangeCommand(1, Sex.Male, AgeUnit.Day, 10, 40, 1.0m, 2.0m, null, "ملاحظة");

        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var saved = db.ReferenceRanges.Single(r => r.Id.Value == 1);
        Assert.Equal(Sex.Male, saved.Sex);
        Assert.Equal(AgeUnit.Day, saved.AgeUnit);
        Assert.Equal(10, saved.AgeMin);
        Assert.Equal(40, saved.AgeMax);
        Assert.Equal(1.0m, saved.MinValue);
        Assert.Equal(2.0m, saved.MaxValue);
        Assert.Equal("ملاحظة", saved.HighComment);
    }

    [Fact]
    public async Task UpdateReferenceRange_NotFound_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        var handler = new UpdateReferenceRangeCommandHandler(db);

        var cmd = new UpdateReferenceRangeCommand(999, null, AgeUnit.Year, 0, 120, 0m, 100m, null, null);

        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("النطاق المرجعي غير موجود", result.Error!.Message);
    }

    // ---------- DeleteReferenceRange (live row only) ----------

    [Fact]
    public async Task DeleteReferenceRange_HappyPath_RemovesLiveRow()
    {
        var db = new FakeApplicationDbContext();
        SeedRange(db, id: 1);

        var handler = new DeleteReferenceRangeCommandHandler(db);
        var result = await handler.Handle(new DeleteReferenceRangeCommand(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(db.ReferenceRanges);
        Assert.Equal(1, db.SaveChangesCallCount);
    }

    [Fact]
    public async Task DeleteReferenceRange_NotFound_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        var handler = new DeleteReferenceRangeCommandHandler(db);

        var result = await handler.Handle(new DeleteReferenceRangeCommand(999), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal(0, db.SaveChangesCallCount);
    }
}