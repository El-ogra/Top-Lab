using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Domain.Tests.Tests;

public class TestCommentTests
{
    [Fact]
    public void Create_Valid()
    {
        var comment = TestComment.Create(TestCommentId.Create(1), TestId.Create(5), "Sample comment");

        Assert.Equal("Sample comment", comment.CommentText);
        Assert.Equal(TestId.Create(5), comment.TestId);
    }

    [Fact]
    public void Create_TrimsText()
    {
        var comment = TestComment.Create(TestCommentId.Create(1), TestId.Create(5), "  Sample comment  ");

        Assert.Equal("Sample comment", comment.CommentText);
    }

    [Fact]
    public void Create_EmptyText_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            TestComment.Create(TestCommentId.Create(1), TestId.Create(5), " "));
    }

    [Fact]
    public void Create_ExactlyMaxLength_Accepted()
    {
        var text = new string('a', TestComment.MaxCommentTextLength);

        var comment = TestComment.Create(TestCommentId.Create(1), TestId.Create(5), text);

        Assert.Equal(TestComment.MaxCommentTextLength, comment.CommentText.Length);
    }

    [Fact]
    public void Create_ExceedsMaxLength_Throws()
    {
        var text = new string('a', TestComment.MaxCommentTextLength + 1);

        Assert.Throws<ArgumentException>(() =>
            TestComment.Create(TestCommentId.Create(1), TestId.Create(5), text));
    }

    [Fact]
    public void Update_Valid()
    {
        var comment = TestComment.Create(TestCommentId.Create(1), TestId.Create(5), "Original");

        comment.Update("Updated");

        Assert.Equal("Updated", comment.CommentText);
    }

    [Fact]
    public void Update_TrimsText()
    {
        var comment = TestComment.Create(TestCommentId.Create(1), TestId.Create(5), "Original");

        comment.Update("  Updated  ");

        Assert.Equal("Updated", comment.CommentText);
    }

    [Fact]
    public void Update_EmptyText_Throws()
    {
        var comment = TestComment.Create(TestCommentId.Create(1), TestId.Create(5), "Original");

        Assert.Throws<ArgumentException>(() => comment.Update(""));
    }

    [Fact]
    public void Update_ExceedsMaxLength_Throws()
    {
        var comment = TestComment.Create(TestCommentId.Create(1), TestId.Create(5), "Original");
        var text = new string('a', TestComment.MaxCommentTextLength + 1);

        Assert.Throws<ArgumentException>(() => comment.Update(text));
    }
}
