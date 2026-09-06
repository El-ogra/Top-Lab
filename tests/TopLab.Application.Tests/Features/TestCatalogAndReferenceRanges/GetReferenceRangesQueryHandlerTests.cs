using TopLab.Application.Features.TestCatalogAndReferenceRanges.Queries.GetReferenceRanges;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;

namespace TopLab.Application.Tests.Features.TestCatalogAndReferenceRanges;

public class GetReferenceRangesQueryHandlerTests
{
    private static FakeApplicationDbContext BuildDb()
    {
        var db = new FakeApplicationDbContext();
        db.ReferenceRanges.Add(ReferenceRange.Create(ReferenceRangeId.Create(1), TestId.Create(1), AgeUnit.Day, 1, 60, 0m, 100m, Sex.Male));
        db.ReferenceRanges.Add(ReferenceRange.Create(ReferenceRangeId.Create(2), TestId.Create(1), AgeUnit.Day, 61, 120, 100m, 200m, Sex.Male));
        db.ReferenceRanges.Add(ReferenceRange.Create(ReferenceRangeId.Create(3), TestId.Create(2), AgeUnit.Month, 1, 12, 10m, 50m, null, "low", "high"));
        return db;
    }

    [Fact]
    public async Task GetReferenceRanges_Empty_ReturnsEmpty()
    {
        var db = new FakeApplicationDbContext();
        var handler = new GetReferenceRangesQueryHandler(db);

        var result = await handler.Handle(new GetReferenceRangesQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }

    [Fact]
    public async Task GetReferenceRanges_ReturnsOrderedList()
    {
        var db = BuildDb();
        var handler = new GetReferenceRangesQueryHandler(db);

        var result = await handler.Handle(new GetReferenceRangesQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var ranges = result.Value!;
        Assert.Equal(3, ranges.Count);
        Assert.True(ranges[0].TestId == 1 && ranges[0].AgeMin == 1);
        Assert.True(ranges[1].TestId == 1 && ranges[1].AgeMin == 61);
        Assert.Equal(TestId.Create(2).Value, ranges[2].TestId);
    }

    [Fact]
    public async Task GetReferenceRanges_FilteredByTestId()
    {
        var db = BuildDb();
        var handler = new GetReferenceRangesQueryHandler(db);

        var result = await handler.Handle(new GetReferenceRangesQuery(TestId: 2), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var range = Assert.Single(result.Value!);
        Assert.Equal(10m, range.MinValue);
        Assert.Equal(AgeUnit.Month, range.AgeUnit);
        Assert.Equal("low", range.LowComment);
    }

    [Fact]
    public async Task GetReferenceRanges_TestIdWithNoRanges_ReturnsEmpty()
    {
        var db = BuildDb();
        var handler = new GetReferenceRangesQueryHandler(db);

        var result = await handler.Handle(new GetReferenceRangesQuery(TestId: 999), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }
}