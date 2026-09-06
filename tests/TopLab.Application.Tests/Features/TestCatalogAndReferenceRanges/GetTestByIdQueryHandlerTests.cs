using TopLab.Application.Common.Results;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Queries.GetTestById;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;

namespace TopLab.Application.Tests.Features.TestCatalogAndReferenceRanges;

public class GetTestByIdQueryHandlerTests
{
    [Fact]
    public async Task GetTestById_Found_ReturnsDetailIncludingIsActiveAndTestCode()
    {
        var db = new FakeApplicationDbContext();
        db.TestGroups.Add(TestGroup.Create(TestGroupId.Create(7), "Kidney"));
        db.Tests.Add(Test.Create(
            TestId.Create(1),
            "CBC",
            "CBC Report",
            "CBC Receipt",
            "CBC01",
            60,
            100m,
            ResultKind.Simple,
            isCultureType: false,
            TestGroupId.Create(7),
            barcode: "12345",
            isSentOut: true,
            sentOutCostPrice: 5m,
            labToLabPrice: 20m));

        var handler = new GetTestByIdQueryHandler(db);
        var result = await handler.Handle(new GetTestByIdQuery(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var dto = result.Value!;
        Assert.Equal("CBC01", dto.TestCode);
        Assert.True(dto.IsActive);
        Assert.Equal("Kidney", dto.TestGroupName);
        Assert.Equal(7, dto.TestGroupId);
        Assert.Equal("12345", dto.Barcode);
        Assert.True(dto.IsSentOut);
        Assert.Equal(5m, dto.SentOutCostPrice);
        Assert.Equal(20m, dto.LabToLabPrice);
        Assert.Equal((int)ResultKind.Simple, dto.ResultKind);
        Assert.Equal("CBC Receipt", dto.ReceiptName);
    }

    [Fact]
    public async Task GetTestById_InactiveTest_StillReturned()
    {
        var db = new FakeApplicationDbContext();
        db.Tests.Add(Test.Create(TestId.Create(2), "Urea", "Urea Report", "Urea Receipt", "URE", 45, 70m, isActive: false));

        var handler = new GetTestByIdQueryHandler(db);
        var result = await handler.Handle(new GetTestByIdQuery(2), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value!.IsActive);
    }

    [Fact]
    public async Task GetTestById_Missing_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        var handler = new GetTestByIdQueryHandler(db);

        var result = await handler.Handle(new GetTestByIdQuery(999), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }
}