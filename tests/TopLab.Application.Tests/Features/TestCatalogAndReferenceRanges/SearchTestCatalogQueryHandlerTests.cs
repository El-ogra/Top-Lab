using TopLab.Application.Features.TestCatalogAndReferenceRanges.Queries.SearchTestCatalog;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;

namespace TopLab.Application.Tests.Features.TestCatalogAndReferenceRanges;

public class SearchTestCatalogQueryHandlerTests
{
    private static FakeApplicationDbContext BuildDb()
    {
        var db = new FakeApplicationDbContext();
        var group = TestGroup.Create(TestGroupId.Create(1), "Kidney");
        db.TestGroups.Add(group);

        db.Tests.Add(Test.Create(TestId.Create(1), "CBC", "CBC Report", "CBC Receipt", "CBC", 60, 100m, testGroupId: TestGroupId.Create(1)));
        db.Tests.Add(Test.Create(TestId.Create(2), "Creatinine", "Creatinine Report", "Creatinine Receipt", "CRE", 45, 80m, testGroupId: TestGroupId.Create(1)));
        db.Tests.Add(Test.Create(TestId.Create(3), "Urea", "Urea Report", "Urea Receipt", "URE", 45, 70m, testGroupId: TestGroupId.Create(1), isActive: false));
        db.Tests.Add(Test.Create(TestId.Create(4), "Glucose", "Glucose Report", "Glucose Receipt", "GLU", 30, 50m));
        return db;
    }

    [Fact]
    public async Task Search_PartialName_ReturnsMatches()
    {
        var db = BuildDb();
        var handler = new SearchTestCatalogQueryHandler(db);

        var result = await handler.Handle(new SearchTestCatalogQuery("Creat", null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var test = Assert.Single(result.Value!);
        Assert.Equal("Creatinine", test.Name);
        Assert.Equal("CRE", test.TestCode);
    }

    [Fact]
    public async Task Search_PartialGroupName_ReturnsMemberTests()
    {
        var db = BuildDb();
        var handler = new SearchTestCatalogQueryHandler(db);

        var result = await handler.Handle(new SearchTestCatalogQuery("Kid", null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
        Assert.All(result.Value, t => Assert.Equal("Kidney", t.TestGroupName));
    }

    [Fact]
    public async Task Search_ExactTestCode_ReturnsSingleMatch()
    {
        var db = BuildDb();
        var handler = new SearchTestCatalogQueryHandler(db);

        var result = await handler.Handle(new SearchTestCatalogQuery("CBC", null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var test = Assert.Single(result.Value!);
        Assert.Equal("CBC", test.TestCode);
    }

    [Fact]
    public async Task Search_EmptyTerm_ReturnsAllActive()
    {
        var db = BuildDb();
        var handler = new SearchTestCatalogQueryHandler(db);

        var result = await handler.Handle(new SearchTestCatalogQuery(null, null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value!.Count);
        Assert.DoesNotContain(result.Value, t => t.TestCode == "URE");
    }

    [Fact]
    public async Task Search_IncludeInactiveTrue_ReturnsInactiveToo()
    {
        var db = BuildDb();
        var handler = new SearchTestCatalogQueryHandler(db);

        var result = await handler.Handle(new SearchTestCatalogQuery(null, null, IncludeInactive: true), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(4, result.Value!.Count);
        Assert.Contains(result.Value, t => t.TestCode == "URE" && !t.IsActive);
    }

    [Fact]
    public async Task Search_TestGroupIdFilter_ReturnsOnlyGroupTests()
    {
        var db = BuildDb();
        var handler = new SearchTestCatalogQueryHandler(db);

        var result = await handler.Handle(new SearchTestCatalogQuery(null, 1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
        Assert.DoesNotContain(result.Value, t => t.TestCode == "GLU");
    }

    [Fact]
    public async Task Search_CombinedFilterGroupAndTerm_ReturnsMatch()
    {
        var db = BuildDb();
        var handler = new SearchTestCatalogQueryHandler(db);

        var result = await handler.Handle(new SearchTestCatalogQuery("Creatinine", 1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var test = Assert.Single(result.Value!);
        Assert.Equal("CRE", test.TestCode);
        Assert.Equal(1, test.TestGroupId);
    }

    [Fact]
    public async Task Search_NoMatch_ReturnsEmpty()
    {
        var db = BuildDb();
        var handler = new SearchTestCatalogQueryHandler(db);

        var result = await handler.Handle(new SearchTestCatalogQuery("zzz-zzz", null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }

    [Fact]
    public async Task Search_OrderedByName()
    {
        var db = BuildDb();
        var handler = new SearchTestCatalogQueryHandler(db);

        var result = await handler.Handle(new SearchTestCatalogQuery(null, null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { "CBC", "Creatinine", "Glucose" }, result.Value!.Select(t => t.Name).ToArray());
    }
}