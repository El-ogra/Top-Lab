using TopLab.Application.Features.ProfileResults.Queries.GetProfileResultAmendments;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Results;
using Xunit;

namespace TopLab.Application.Tests.Features.ProfileResults;

public class GetProfileResultAmendmentsQueryHandlerTests
{
    [Fact]
    public async Task Amendments_OrderedByTimeThenId()
    {
        var db = new FakeApplicationDbContext();
        var pt = ProfileResultsSeed.AddProfileTest(db, 101, 1, 10);
        var item = ProfileResultsSeed.AddPrintedItem(db, 501, pt, 1);

        db.ProfileResultAmendments.Add(ProfileResultAmendment.Create(
            ProfileResultAmendmentId.Create(1),
            item.Id,
            7,
            new DateTime(2026, 9, 9, 10, 0, 0, DateTimeKind.Utc),
            "4",
            "mg",
            ProfileResultFlag.Low,
            "5",
            "mg",
            ProfileResultFlag.Low,
            null));
        db.ProfileResultAmendments.Add(ProfileResultAmendment.Create(
            ProfileResultAmendmentId.Create(3),
            item.Id,
            7,
            new DateTime(2026, 9, 9, 10, 0, 0, DateTimeKind.Utc),
            "5",
            "mg",
            ProfileResultFlag.Low,
            "6",
            "mg",
            ProfileResultFlag.Low,
            null));
        db.ProfileResultAmendments.Add(ProfileResultAmendment.Create(
            ProfileResultAmendmentId.Create(2),
            item.Id,
            8,
            new DateTime(2026, 9, 9, 11, 0, 0, DateTimeKind.Utc),
            "6",
            "mg",
            ProfileResultFlag.Low,
            "7",
            "mg",
            ProfileResultFlag.High,
            "خطأ إدخال"));

        var result = await new GetProfileResultAmendmentsQueryHandler(db).Handle(
            new GetProfileResultAmendmentsQuery(501),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var rows = result.Value!;
        Assert.Equal(3, rows.Count);
        Assert.Equal(new[] { 1, 3, 2 }, rows.Select(r => r.Id));
        var last = rows[2];
        Assert.Equal(8, last.AmendedByUserId);
        Assert.Equal("6", last.OldResultValue);
        Assert.Equal("7", last.NewResultValue);
        Assert.Equal(1, last.NewFlag);
        Assert.Equal("خطأ إدخال", last.Reason);
    }

    [Fact]
    public async Task Amendments_Empty_ReturnsEmptyList()
    {
        var db = new FakeApplicationDbContext();

        var result = await new GetProfileResultAmendmentsQueryHandler(db).Handle(
            new GetProfileResultAmendmentsQuery(501),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }
}
