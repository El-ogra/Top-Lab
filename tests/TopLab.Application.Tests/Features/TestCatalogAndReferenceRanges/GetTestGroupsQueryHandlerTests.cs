using TopLab.Application.Features.TestCatalogAndReferenceRanges.Queries.GetTestGroups;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;

namespace TopLab.Application.Tests.Features.TestCatalogAndReferenceRanges;

public class GetTestGroupsQueryHandlerTests
{
    [Fact]
    public async Task GetTestGroups_Empty_ReturnsEmpty()
    {
        var db = new FakeApplicationDbContext();
        var handler = new GetTestGroupsQueryHandler(db);

        var result = await handler.Handle(new GetTestGroupsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }

    [Fact]
    public async Task GetTestGroups_ReturnsOrderedActiveOnly()
    {
        var db = new FakeApplicationDbContext();
        db.TestGroups.Add(TestGroup.Create(TestGroupId.Create(1), "Kidney"));
        db.TestGroups.Add(TestGroup.Create(TestGroupId.Create(2), "Liver"));
        db.TestGroups.Add(TestGroup.Create(TestGroupId.Create(3), "Hormones", isActive: false));

        var handler = new GetTestGroupsQueryHandler(db);
        var result = await handler.Handle(new GetTestGroupsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var groups = result.Value!;
        Assert.Equal(2, groups.Count);
        Assert.Equal(new[] { "Kidney", "Liver" }, groups.Select(g => g.Name).ToArray());
    }

    [Fact]
    public async Task GetTestGroups_IncludeInactive_ReturnsAll()
    {
        var db = new FakeApplicationDbContext();
        db.TestGroups.Add(TestGroup.Create(TestGroupId.Create(1), "Kidney"));
        db.TestGroups.Add(TestGroup.Create(TestGroupId.Create(3), "Hormones", isActive: false));

        var handler = new GetTestGroupsQueryHandler(db);
        var result = await handler.Handle(new GetTestGroupsQuery(IncludeInactive: true), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
        Assert.Contains(result.Value, g => g.Name == "Hormones" && !g.IsActive);
    }
}