using TopLab.Application.Features.ReportProduction.Queries.GetCombinableTests;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.ReportProduction;

public class GetCombinableTestsQueryHandlerTests
{
    [Fact]
    public async Task ReturnsReviewedRowsOnlyWithCatalogMapping()
    {
        var db = Seed();
        db.PatientTests.Add(Reviewed(11, testId: 2, value: "خلية بيضاء"));
        db.PatientTests.Add(EnteredOnly(12, testId: 3));

        var result = await new GetCombinableTestsQueryHandler(db)
            .Handle(new GetCombinableTestsQuery(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var dto = Assert.Single(result.Value!);
        Assert.Equal(11, dto.PatientTestId);
        Assert.Equal(2, dto.TestId);
        Assert.Equal("Glucose", dto.TestName);
        Assert.Equal("GLU", dto.TestCode);
        Assert.Equal(0, dto.ResultKind);
    }

    [Fact]
    public async Task MapsCultureKind()
    {
        var db = Seed();
        db.Tests.Add(Test.Create(TestId.Create(4), "CultureA", "Culture report", "Culture", "CULT4", 1, 100m, ResultKind.Culture, isCultureType: true));
        db.PatientTests.Add(Reviewed(21, testId: 4, value: null));

        var result = await new GetCombinableTestsQueryHandler(db)
            .Handle(new GetCombinableTestsQuery(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value![0].ResultKind);
    }

    [Fact]
    public async Task EmptyCatalogTestStillReturnsEligibleRow()
    {
        var db = Seed();
        db.PatientTests.Add(Reviewed(31, testId: 999, value: "x"));

        var result = await new GetCombinableTestsQueryHandler(db)
            .Handle(new GetCombinableTestsQuery(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var dto = Assert.Single(result.Value!);
        Assert.Equal(string.Empty, dto.TestName);
    }

    [Fact]
    public async Task MissingPatient_ReturnsNotFound()
    {
        var db = Seed();
        var result = await new GetCombinableTestsQueryHandler(db)
            .Handle(new GetCombinableTestsQuery(42), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("المريض غير موجود.", result.Error!.Message);
    }

    [Fact]
    public async Task DeletedPatient_ReturnsNotFound()
    {
        var db = Seed();
        db.Patients[0].SoftDelete();

        var result = await new GetCombinableTestsQueryHandler(db)
            .Handle(new GetCombinableTestsQuery(1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("المريض غير موجود.", result.Error!.Message);
    }

    [Fact]
    public async Task NoReviewedRows_ReturnsEmptyList()
    {
        var db = Seed();
        db.PatientTests.Add(EnteredOnly(12, testId: 2));

        var result = await new GetCombinableTestsQueryHandler(db)
            .Handle(new GetCombinableTestsQuery(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }

    internal static FakeApplicationDbContext Seed()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(Patient.Create(PatientId.Create(1), "Ali", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow));
        db.Tests.Add(Test.Create(TestId.Create(2), "Glucose", "Glucose report", "Glucose", "GLU", 1, 100m, ResultKind.Simple));
        db.Tests.Add(Test.Create(TestId.Create(3), "Profile", "Profile report", "Profile", "PRO", 1, 100m, ResultKind.SpecializedProfile));
        return db;
    }

    internal static PatientTest Reviewed(int ptId, int testId, string? value)
    {
        var pt = PatientTest.Create(PatientTestId.Create(ptId), PatientId.Create(1), TestId.Create(testId), 100m);
        pt.EnterResult(value, ResultFlag.Normal, 1, DateTime.UtcNow);
        pt.MarkReviewed(2, DateTime.UtcNow);
        return pt;
    }

    internal static PatientTest EnteredOnly(int ptId, int testId)
    {
        var pt = PatientTest.Create(PatientTestId.Create(ptId), PatientId.Create(1), TestId.Create(testId), 100m);
        pt.EnterResult("x", null, 1, DateTime.UtcNow);
        return pt;
    }
}