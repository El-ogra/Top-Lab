using TopLab.Application.Common.Results;
using TopLab.Application.Features.ResultDelivery.Queries.GetDeliveryGrid;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.ResultDelivery;

public class GetDeliveryGridQueryHandlerTests
{
    private static Patient MakePatient(int id = 1)
    {
        return Patient.Create(PatientId.Create(id), $"P{id}", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);
    }

    [Fact]
    public async Task Grid_ReturnsAllColumns_WithFrozenPrice()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient());
        // Catalog price (500) differs from the frozen order-time price (100):
        // the grid must carry the frozen value.
        db.Tests.Add(Test.Create(TestId.Create(10), "CBC", "CBC", "CBC-R", "T10", 30, 500m));
        var pt = PatientTest.Create(PatientTestId.Create(100), PatientId.Create(1), TestId.Create(10), 100m);
        pt.EnterResult("5.5", ResultFlag.High, 1, DateTime.UtcNow);
        pt.MarkReviewed(1, DateTime.UtcNow);
        pt.MarkPrinted(1, DateTime.UtcNow);
        db.PatientTests.Add(pt);

        var handler = new GetDeliveryGridQueryHandler(db);
        var result = await handler.Handle(new GetDeliveryGridQuery(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var row = Assert.Single(result.Value!);
        Assert.Equal(100, row.PatientTestId);
        Assert.Equal("CBC", row.TestName);
        Assert.Equal("T10", row.TestCode);
        Assert.Equal("5.5", row.ResultValue);
        Assert.Equal((int)ResultFlag.High, row.Flag);
        Assert.True(row.IsEntered);
        Assert.True(row.IsReviewed);
        Assert.True(row.IsPrinted);
        Assert.False(row.IsDelivered);
        Assert.Equal(100m, row.Price);
    }

    [Fact]
    public async Task Grid_ShowsAllLines_WithPerLineFlags()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient());
        db.Tests.Add(Test.Create(TestId.Create(10), "CBC", "CBC", "CBC-R", "T10", 30, 100m));
        db.Tests.Add(Test.Create(TestId.Create(11), "Glu", "Glu", "Glu-R", "T11", 30, 50m));
        var entered = PatientTest.Create(PatientTestId.Create(100), PatientId.Create(1), TestId.Create(10), 100m);
        entered.EnterResult("5", ResultFlag.Normal, 1, DateTime.UtcNow);
        var fresh = PatientTest.Create(PatientTestId.Create(101), PatientId.Create(1), TestId.Create(11), 50m);
        db.PatientTests.Add(entered);
        db.PatientTests.Add(fresh);

        var handler = new GetDeliveryGridQueryHandler(db);
        var result = await handler.Handle(new GetDeliveryGridQuery(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
        Assert.Contains(result.Value, r => r.PatientTestId == 100 && r.IsEntered && !r.IsReviewed);
        Assert.Contains(result.Value, r => r.PatientTestId == 101 && !r.IsEntered && r.Price == 50m);
    }

    [Fact]
    public async Task UnknownPatient_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();

        var handler = new GetDeliveryGridQueryHandler(db);
        var result = await handler.Handle(new GetDeliveryGridQuery(99), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("المريض غير موجود.", result.Error!.Message);
    }

    [Fact]
    public async Task SoftDeletedPatient_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        var patient = MakePatient();
        patient.SoftDelete();
        db.Patients.Add(patient);

        var handler = new GetDeliveryGridQueryHandler(db);
        var result = await handler.Handle(new GetDeliveryGridQuery(1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("المريض غير موجود.", result.Error!.Message);
    }

    [Fact]
    public void Validator_InvalidPatientId_Rejects()
    {
        var validator = new GetDeliveryGridQueryValidator();
        var result = validator.Validate(new GetDeliveryGridQuery(0));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "معرّف المريض غير صالح.");
    }
}
