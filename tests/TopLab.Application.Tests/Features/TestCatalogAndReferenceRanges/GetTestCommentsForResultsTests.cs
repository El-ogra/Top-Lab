using TopLab.Application.Features.TestCatalogAndReferenceRanges.Queries.GetTestCommentsForResults;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.TestCatalogAndReferenceRanges;

/// <summary>W-02 S8 (WP-13): aggregated comment read — one query, no N+1.</summary>
public class GetTestCommentsForResultsTests
{
    private static FakeApplicationDbContext Seed()
    {
        var db = new FakeApplicationDbContext();
        db.TestComments.Add(TestComment.Create(TestCommentId.Create(1), TestId.Create(2), "أ"));
        db.TestComments.Add(TestComment.Create(TestCommentId.Create(2), TestId.Create(2), "ب"));
        db.TestComments.Add(TestComment.Create(TestCommentId.Create(3), TestId.Create(3), "ج"));
        return db;
    }

    [Fact]
    public async Task GetTestCommentsForResults_ReturnsCommentsForEachTest()
    {
        var result = await new GetTestCommentsForResultsQueryHandler(Seed())
            .Handle(new GetTestCommentsForResultsQuery(new[] { 2, 3 }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
        Assert.Equal(new[] { "أ", "ب" }, result.Value.First(t => t.TestId == 2).Comments);
        Assert.Equal(new[] { "ج" }, result.Value.First(t => t.TestId == 3).Comments);
    }

    [Fact]
    public async Task GetTestCommentsForResults_NoTestIds_ReturnsEmpty()
    {
        var result = await new GetTestCommentsForResultsQueryHandler(Seed())
            .Handle(new GetTestCommentsForResultsQuery(Array.Empty<int>()), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }

    [Fact]
    public async Task GetTestCommentsForResults_UnknownTestId_ReturnsEmpty()
    {
        var result = await new GetTestCommentsForResultsQueryHandler(Seed())
            .Handle(new GetTestCommentsForResultsQuery(new[] { 99 }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var only = Assert.Single(result.Value!);
        Assert.Equal(99, only.TestId);
        Assert.Empty(only.Comments);
    }
}
