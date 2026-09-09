using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.CreateTest;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.MapTestToAnalyte;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.UnmapTestFromAnalyte;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.TestCatalogAndReferenceRanges.Commands;

public class TestAnalyteMappingCommandTests
{
    private const string Base = "T10";

    [Fact]
    public async Task CreateSimpleTest_WithAnalyteId_MapsIt()
    {
        var db = new FakeApplicationDbContext();
        db.Analytes.Add(Analyte.Create(AnalyteId.Create(50), "Sodium", "Sodium"));

        var handler = new CreateTestCommandHandler(db);
        var result = await handler.Handle(CreateCmd(Base, ResultKind.Simple, analyeId: 50), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(50, db.Tests.Single().AnalyteId!.Value);
    }

    [Fact]
    public async Task CreateSpecializedTest_WithAnalyteId_Conflict()
    {
        var db = new FakeApplicationDbContext();
        db.Analytes.Add(Analyte.Create(AnalyteId.Create(50), "Sodium", "Sodium"));

        var handler = new CreateTestCommandHandler(db);
        var result = await handler.Handle(CreateCmd(Base, ResultKind.SpecializedProfile, analyeId: 50), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("لا يمكن ربط المادة التحليلية إلا بتحليل بسيط.", result.Error!.Message);
    }

    [Fact]
    public async Task MapSimpleTest_Success()
    {
        var db = new FakeApplicationDbContext();
        db.Tests.Add(Test.Create(TestId.Create(10), Base, Base, Base, Base, 30, 100m, ResultKind.Simple));
        db.Analytes.Add(Analyte.Create(AnalyteId.Create(50), "Sodium", "Sodium"));

        var handler = new MapTestToAnalyteCommandHandler(db);
        var result = await handler.Handle(new MapTestToAnalyteCommand(10, 50), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(AnalyteId.Create(50), db.Tests.Single().AnalyteId);
    }

    [Fact]
    public async Task MapSpecializedTest_Conflict()
    {
        var db = new FakeApplicationDbContext();
        db.Tests.Add(Test.Create(TestId.Create(10), Base, Base, Base, Base, 30, 100m, ResultKind.SpecializedProfile));
        db.Analytes.Add(Analyte.Create(AnalyteId.Create(50), "Sodium", "Sodium"));

        var handler = new MapTestToAnalyteCommandHandler(db);
        var result = await handler.Handle(new MapTestToAnalyteCommand(10, 50), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("لا يمكن ربط المادة التحليلية إلا بتحليل بسيط.", result.Error!.Message);
    }

    [Fact]
    public async Task MapMissingAnalyte_NotFound()
    {
        var db = new FakeApplicationDbContext();
        db.Tests.Add(Test.Create(TestId.Create(10), Base, Base, Base, Base, 30, 100m, ResultKind.Simple));

        var handler = new MapTestToAnalyteCommandHandler(db);
        var result = await handler.Handle(new MapTestToAnalyteCommand(10, 50), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("المادة التحليلية غير موجودة", result.Error!.Message);
    }

    [Fact]
    public async Task UnmapTest_ClearsMapping()
    {
        var db = new FakeApplicationDbContext();
        var test = Test.Create(TestId.Create(10), Base, Base, Base, Base, 30, 100m, ResultKind.Simple);
        test.MapToAnalyte(AnalyteId.Create(50));
        db.Tests.Add(test);

        var handler = new UnmapTestFromAnalyteCommandHandler(db);
        var result = await handler.Handle(new UnmapTestFromAnalyteCommand(10), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(db.Tests.Single().AnalyteId);
    }

    private static CreateTestCommand CreateCmd(string name, ResultKind kind, int? analyeId)
    {
        return new CreateTestCommand(
            name,
            name,
            name,
            "TC",
            30,
            100m,
            kind,
            kind == ResultKind.Culture,
            TestGroupId: null,
            Barcode: null,
            IsSentOut: false,
            SentOutCostPrice: null,
            LabToLabPrice: null,
            AnalyteId: analyeId);
    }
}