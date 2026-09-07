using TopLab.Application.Common.Results;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.CreateTestComment;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.DeleteTestComment;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.UpdateTestComment;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.PriceListsCommentsAndCustomGroups;

public class TestCommentWriteCommandHandlerTests
{
    private static Test SeedTest(int id = 1)
    {
        return Test.Create(TestId.Create(id), "CBC", "CBC Report", "CBC Receipt", $"T{id}", 60, 100m, ResultKind.Simple, false, null, null, false, null, null, true);
    }

    // ---------- CreateTestComment ----------

    [Fact]
    public async Task CreateTestComment_HappyPath_PersistsComment()
    {
        var db = new FakeApplicationDbContext();
        db.Tests.Add(SeedTest(1));
        var handler = new CreateTestCommentCommandHandler(db);

        var result = await handler.Handle(new CreateTestCommentCommand(1, "تعليق"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(db.TestComments);
    }

    [Fact]
    public async Task CreateTestComment_MissingTest_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        var handler = new CreateTestCommentCommandHandler(db);

        var result = await handler.Handle(new CreateTestCommentCommand(999, "تعليق"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("التحليل غير موجود", result.Error!.Message);
    }

    [Fact]
    public async Task CreateTestComment_MultipleCommentsPerTest_Allowed()
    {
        var db = new FakeApplicationDbContext();
        db.Tests.Add(SeedTest(1));
        var handler = new CreateTestCommentCommandHandler(db);

        var first = await handler.Handle(new CreateTestCommentCommand(1, "First"), CancellationToken.None);
        var second = await handler.Handle(new CreateTestCommentCommand(1, "Second"), CancellationToken.None);
        var third = await handler.Handle(new CreateTestCommentCommand(1, "Third"), CancellationToken.None);

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        Assert.True(third.IsSuccess);
        Assert.Equal(3, db.TestComments.Count);
    }

    [Fact]
    public async Task CreateTestComment_EmptyText_ReturnsValidation()
    {
        var db = new FakeApplicationDbContext();
        db.Tests.Add(SeedTest(1));
        var handler = new CreateTestCommentCommandHandler(db);

        var result = await handler.Handle(new CreateTestCommentCommand(1, " "), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
    }

    [Fact]
    public async Task CreateTestComment_TextTooLong_ReturnsValidation()
    {
        var db = new FakeApplicationDbContext();
        db.Tests.Add(SeedTest(1));
        var handler = new CreateTestCommentCommandHandler(db);
        var longText = new string('a', TestComment.MaxCommentTextLength + 1);

        var result = await handler.Handle(new CreateTestCommentCommand(1, longText), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
        Assert.Equal("نص التعليق يجب ألا يتجاوز 1000 حرف.", result.Error!.Message);
    }

    // ---------- UpdateTestComment ----------

    [Fact]
    public async Task UpdateTestComment_HappyPath_Updates()
    {
        var db = new FakeApplicationDbContext();
        db.TestComments.Add(TestComment.Create(TestCommentId.Create(1), TestId.Create(1), "Original"));
        var handler = new UpdateTestCommentCommandHandler(db);

        var result = await handler.Handle(new UpdateTestCommentCommand(1, "Updated"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Updated", db.TestComments.Single().CommentText);
    }

    [Fact]
    public async Task UpdateTestComment_Missing_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        var handler = new UpdateTestCommentCommandHandler(db);

        var result = await handler.Handle(new UpdateTestCommentCommand(999, "X"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("التعليق غير موجود.", result.Error!.Message);
    }

    [Fact]
    public async Task UpdateTestComment_EmptyText_ReturnsValidation()
    {
        var db = new FakeApplicationDbContext();
        db.TestComments.Add(TestComment.Create(TestCommentId.Create(1), TestId.Create(1), "Original"));
        var handler = new UpdateTestCommentCommandHandler(db);

        var result = await handler.Handle(new UpdateTestCommentCommand(1, " "), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
    }

    // ---------- DeleteTestComment ----------

    [Fact]
    public async Task DeleteTestComment_HappyPath_Removes()
    {
        var db = new FakeApplicationDbContext();
        db.TestComments.Add(TestComment.Create(TestCommentId.Create(1), TestId.Create(1), "Original"));
        var handler = new DeleteTestCommentCommandHandler(db);

        var result = await handler.Handle(new DeleteTestCommentCommand(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(db.TestComments);
    }

    [Fact]
    public async Task DeleteTestComment_Missing_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        var handler = new DeleteTestCommentCommandHandler(db);

        var result = await handler.Handle(new DeleteTestCommentCommand(999), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }
}
