using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Queries.GetCustomGroups;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.PriceListsCommentsAndCustomGroups;

public class GetCustomGroupsQueryHandlerTests
{
    [Fact]
    public async Task GetCustomGroups_Empty_ReturnsEmpty()
    {
        var db = new FakeApplicationDbContext();
        var handler = new GetCustomGroupsQueryHandler(db);

        var result = await handler.Handle(new GetCustomGroupsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }

    [Fact]
    public async Task GetCustomGroups_WithItems_ReturnsItemCounts()
    {
        var db = new FakeApplicationDbContext();
        db.CustomGroups.Add(CustomGroup.Create(CustomGroupId.Create(1), "Checkup"));
        db.CustomGroups.Add(CustomGroup.Create(CustomGroupId.Create(2), "Panel"));
        db.CustomGroupItems.Add(new CustomGroupItem(CustomGroupId.Create(1), TestId.Create(100), 50m));
        db.CustomGroupItems.Add(new CustomGroupItem(CustomGroupId.Create(1), TestId.Create(101), 75m));
        db.CustomGroupItems.Add(new CustomGroupItem(CustomGroupId.Create(2), TestId.Create(100), 60m));

        var handler = new GetCustomGroupsQueryHandler(db);
        var result = await handler.Handle(new GetCustomGroupsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var dtos = result.Value!;
        Assert.Equal(2, dtos.Count);
        Assert.Equal("Checkup", dtos[0].Name);
        Assert.Equal(2, dtos[0].ItemCount);
        Assert.Equal("Panel", dtos[1].Name);
        Assert.Equal(1, dtos[1].ItemCount);
    }

    [Fact]
    public async Task GetCustomGroups_OrdersByName()
    {
        var db = new FakeApplicationDbContext();
        db.CustomGroups.Add(CustomGroup.Create(CustomGroupId.Create(1), "Zeta"));
        db.CustomGroups.Add(CustomGroup.Create(CustomGroupId.Create(2), "Alpha"));
        db.CustomGroups.Add(CustomGroup.Create(CustomGroupId.Create(3), "Mu"));

        var handler = new GetCustomGroupsQueryHandler(db);
        var result = await handler.Handle(new GetCustomGroupsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { "Alpha", "Mu", "Zeta" }, result.Value!.Select(d => d.Name).ToArray());
    }
}
