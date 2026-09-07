using TopLab.Application.Common.Results;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Queries.GetCustomGroupById;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.PriceListsCommentsAndCustomGroups;

public class GetCustomGroupByIdQueryHandlerTests
{
    [Fact]
    public async Task GetCustomGroupById_Found_ReturnsDetailWithItemsAndJoinedTestNames()
    {
        var db = new FakeApplicationDbContext();
        db.Tests.Add(Test.Create(TestId.Create(1), "CBC", "CBC Report", "CBC Receipt", "CBC01", 60, 100m, ResultKind.Simple, false, null, null, false, null, null, true));
        db.Tests.Add(Test.Create(TestId.Create(2), "Creatinine", "Creatinine Report", "Creatinine Receipt", "CRE01", 45, 80m, ResultKind.Simple, false, null, null, false, null, null, true));
        db.CustomGroups.Add(CustomGroup.Create(CustomGroupId.Create(1), "Checkup"));
        db.CustomGroupItems.Add(new CustomGroupItem(CustomGroupId.Create(1), TestId.Create(1), 50m));
        db.CustomGroupItems.Add(new CustomGroupItem(CustomGroupId.Create(1), TestId.Create(2), 75m));

        var handler = new GetCustomGroupByIdQueryHandler(db);
        var result = await handler.Handle(new GetCustomGroupByIdQuery(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var dto = result.Value!;
        Assert.Equal("Checkup", dto.Name);
        Assert.Equal(2, dto.Items.Count);
        Assert.Contains(dto.Items, i => i.TestId == 1 && i.TestName == "CBC" && i.TestCode == "CBC01" && i.Price == 50m);
        Assert.Contains(dto.Items, i => i.TestId == 2 && i.TestName == "Creatinine" && i.TestCode == "CRE01" && i.Price == 75m);
    }

    [Fact]
    public async Task GetCustomGroupById_NoItems_ReturnsEmptyItems()
    {
        var db = new FakeApplicationDbContext();
        db.CustomGroups.Add(CustomGroup.Create(CustomGroupId.Create(1), "Empty"));

        var handler = new GetCustomGroupByIdQueryHandler(db);
        var result = await handler.Handle(new GetCustomGroupByIdQuery(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Items);
    }

    [Fact]
    public async Task GetCustomGroupById_Missing_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        var handler = new GetCustomGroupByIdQueryHandler(db);

        var result = await handler.Handle(new GetCustomGroupByIdQuery(999), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }
}
