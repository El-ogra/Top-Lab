using TopLab.Domain.Common;
using TopLab.Domain.Common.Ids;

namespace TopLab.Domain.Tests;

public sealed class TestComment : Entity<TestCommentId>
{
    public TestId TestId { get; private set; } = default!;

    public string CommentText { get; private set; } = default!;

    private TestComment()
    {
    }

    private TestComment(TestCommentId id, TestId testId, string commentText)
        : base(id)
    {
        TestId = testId;
        CommentText = commentText;
    }

    public static TestComment Create(TestCommentId id, TestId testId, string commentText)
    {
        if (string.IsNullOrWhiteSpace(commentText))
        {
            throw new ArgumentException("CommentText is required.", nameof(commentText));
        }

        return new TestComment(id, testId, commentText.Trim());
    }
}
