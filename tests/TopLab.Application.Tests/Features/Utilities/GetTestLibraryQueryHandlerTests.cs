using TopLab.Application.Features.Utilities.Queries.GetTestLibrary;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.Utilities;

public class GetTestLibraryQueryHandlerTests
{
    private static GetTestLibraryQueryHandler Handler(FakeApplicationDbContext db) => new(db);

    private static Test MakeTest(int id, string name, string code, int? groupId)
    {
        return Test.Create(
            TestId.Create(id),
            name,
            name,
            name,
            code,
            30,
            10m,
            testGroupId: groupId.HasValue ? TestGroupId.Create(groupId.Value) : null);
    }

    [Fact]
    public async Task Projection_Shape_AndDictionaryGroupNames()
    {
        var db = new FakeApplicationDbContext();
        db.TestGroups.Add(TestGroup.Create(TestGroupId.Create(1), "دم"));
        db.Tests.Add(MakeTest(10, "CBC", "C01", 1));
        db.Tests.Add(MakeTest(11, "Vitamin D", "V01", null));

        var result = await Handler(db).Handle(
            new GetTestLibraryQuery(null, null),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
        var cbc = result.Value.Single(t => t.TestId == 10);
        Assert.Equal("CBC", cbc.Name);
        Assert.Equal("C01", cbc.TestCode);
        Assert.Equal("دم", cbc.GroupName);
        Assert.Null(result.Value.Single(t => t.TestId == 11).GroupName);
    }

    [Fact]
    public async Task NameFilter_AndGroupFilter()
    {
        var db = new FakeApplicationDbContext();
        db.TestGroups.Add(TestGroup.Create(TestGroupId.Create(1), "دم"));
        db.TestGroups.Add(TestGroup.Create(TestGroupId.Create(2), "كيمياء"));
        db.Tests.Add(MakeTest(10, "CBC", "C01", 1));
        db.Tests.Add(MakeTest(11, "Glucose", "G01", 2));

        var byName = await Handler(db).Handle(new GetTestLibraryQuery("CB", null), CancellationToken.None);
        var byGroup = await Handler(db).Handle(new GetTestLibraryQuery(null, 2), CancellationToken.None);

        Assert.True(byName.IsSuccess);
        Assert.True(byGroup.IsSuccess);
        Assert.Single(byName.Value!);
        Assert.Equal("CBC", byName.Value!.Single().Name);
        Assert.Single(byGroup.Value!);
        Assert.Equal("Glucose", byGroup.Value!.Single().Name);
    }

    [Fact]
    public async Task UnknownGroup_ReturnsNotFound_FrozenMessage()
    {
        var db = new FakeApplicationDbContext();
        var result = await Handler(db).Handle(new GetTestLibraryQuery(null, 999), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("مجموعة التحاليل غير موجودة.", result.Error!.Message);
    }

    [Fact]
    public async Task EmptyCatalog_ReturnsEmptyList()
    {
        var db = new FakeApplicationDbContext();
        var result = await Handler(db).Handle(new GetTestLibraryQuery(null, null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }

    [Fact]
    public void Validator_RejectsOverlongNameFilter()
    {
        var validator = new GetTestLibraryQueryValidator();
        Assert.False(validator.Validate(new GetTestLibraryQuery(new string('x', 201), null)).IsValid);
    }
}
