using TopLab.Application.Common.Results;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Queries.GetTestComments;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.PriceListsCommentsAndCustomGroups;

public class GetTestCommentsQueryHandlerTests
{
    [Fact]
    public async Task GetTestComments_NoFilter_ReturnsAll()
    {
        var db = new FakeApplicationDbContext();
        db.Tests.Add(Test.Create(TestId.Create(1), "CBC", "CBC Report", "CBC Receipt", "CBC01", 60, 100m, ResultKind.Simple, false, null, null, false, null, null, true));
        db.Tests.Add(Test.Create(TestId.Create(2), "Creatinine", "Creatinine Report", "Creatinine Receipt", "CRE01", 45, 80m, ResultKind.Simple, false, null, null, false, null, null, true));
        db.TestComments.Add(TestComment.Create(TestCommentId.Create(1), TestId.Create(1), "First CBC comment"));
        db.TestComments.Add(TestComment.Create(TestCommentId.Create(2), TestId.Create(2), "First Creatinine comment"));
        db.TestComments.Add(TestComment.Create(TestCommentId.Create(3), TestId.Create(1), "Second CBC comment"));

        var handler = new GetTestCommentsQueryHandler(db);
        var result = await handler.Handle(new GetTestCommentsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value!.Count);
    }

    [Fact]
    public async Task GetTestComments_FilterByTestId_ReturnsOnlyThatTest()
    {
        var db = new FakeApplicationDbContext();
        db.Tests.Add(Test.Create(TestId.Create(1), "CBC", "CBC Report", "CBC Receipt", "CBC01", 60, 100m, ResultKind.Simple, false, null, null, false, null, null, true));
        db.Tests.Add(Test.Create(TestId.Create(2), "Creatinine", "Creatinine Report", "Creatinine Receipt", "CRE01", 45, 80m, ResultKind.Simple, false, null, null, false, null, null, true));
        db.TestComments.Add(TestComment.Create(TestCommentId.Create(1), TestId.Create(1), "First CBC comment"));
        db.TestComments.Add(TestComment.Create(TestCommentId.Create(2), TestId.Create(2), "First Creatinine comment"));
        db.TestComments.Add(TestComment.Create(TestCommentId.Create(3), TestId.Create(1), "Second CBC comment"));

        var handler = new GetTestCommentsQueryHandler(db);
        var result = await handler.Handle(new GetTestCommentsQuery(TestId: 1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
        Assert.All(result.Value!, c => Assert.Equal(1, c.TestId));
        Assert.All(result.Value!, c => Assert.Equal("CBC", c.TestName));
    }

    [Fact]
    public async Task GetTestComments_FilterByMissingTest_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        var handler = new GetTestCommentsQueryHandler(db);

        var result = await handler.Handle(new GetTestCommentsQuery(TestId: 999), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }

    [Fact]
    public async Task GetTestComments_MultipleCommentsPerTest_AllReturned()
    {
        var db = new FakeApplicationDbContext();
        db.Tests.Add(Test.Create(TestId.Create(1), "CBC", "CBC Report", "CBC Receipt", "CBC01", 60, 100m, ResultKind.Simple, false, null, null, false, null, null, true));
        db.TestComments.Add(TestComment.Create(TestCommentId.Create(1), TestId.Create(1), "Comment A"));
        db.TestComments.Add(TestComment.Create(TestCommentId.Create(2), TestId.Create(1), "Comment B"));
        db.TestComments.Add(TestComment.Create(TestCommentId.Create(3), TestId.Create(1), "Comment C"));

        var handler = new GetTestCommentsQueryHandler(db);
        var result = await handler.Handle(new GetTestCommentsQuery(TestId: 1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value!.Count);
    }
}
