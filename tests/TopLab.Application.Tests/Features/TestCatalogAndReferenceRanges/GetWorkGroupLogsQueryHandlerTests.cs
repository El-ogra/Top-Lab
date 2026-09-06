using TopLab.Application.Features.TestCatalogAndReferenceRanges.Queries.GetWorkGroupLogs;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;

namespace TopLab.Application.Tests.Features.TestCatalogAndReferenceRanges;

public class GetWorkGroupLogsQueryHandlerTests
{
    [Fact]
    public async Task GetWorkGroupLogs_Empty_ReturnsEmpty()
    {
        var db = new FakeApplicationDbContext();
        var handler = new GetWorkGroupLogsQueryHandler(db);

        var result = await handler.Handle(new GetWorkGroupLogsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }

    [Fact]
    public async Task GetWorkGroupLogs_LogWithItems_ReturnsItemsWithTestNames()
    {
        var db = new FakeApplicationDbContext();
        db.Tests.Add(Test.Create(TestId.Create(1), "CBC", "CBC Report", "CBC Receipt", "CBC", 60, 100m));
        db.Tests.Add(Test.Create(TestId.Create(2), "Creatinine", "Creatinine Report", "Creatinine Receipt", "CRE", 45, 80m));
        var log = WorkGroupLog.Create(WorkGroupLogId.Create(1), "Main Log");
        db.WorkGroupLogs.Add(log);
        db.WorkGroupLogItems.Add(WorkGroupLogItem.Create(WorkGroupLogId.Create(1), TestId.Create(1)));
        db.WorkGroupLogItems.Add(WorkGroupLogItem.Create(WorkGroupLogId.Create(1), TestId.Create(2)));

        var handler = new GetWorkGroupLogsQueryHandler(db);
        var result = await handler.Handle(new GetWorkGroupLogsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var dto = Assert.Single(result.Value!);
        Assert.Equal("Main Log", dto.Name);
        Assert.Equal(2, dto.Items.Count);
        Assert.Equal("CBC", dto.Items.Single(i => i.TestId == 1).TestName);
        Assert.Equal("Creatinine", dto.Items.Single(i => i.TestId == 2).TestName);
    }

    [Fact]
    public async Task GetWorkGroupLogs_LogWithoutItems_ReturnsEmptyItems()
    {
        var db = new FakeApplicationDbContext();
        db.WorkGroupLogs.Add(WorkGroupLog.Create(WorkGroupLogId.Create(1), "Empty Log"));

        var handler = new GetWorkGroupLogsQueryHandler(db);
        var result = await handler.Handle(new GetWorkGroupLogsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var dto = Assert.Single(result.Value!);
        Assert.Empty(dto.Items);
    }
}