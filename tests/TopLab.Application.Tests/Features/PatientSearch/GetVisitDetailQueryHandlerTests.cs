using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientSearch.Queries.GetVisitDetail;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.PatientSearch;

public class GetVisitDetailQueryHandlerTests
{
    private static Patient MakePatient(int id, string name, DateTime when)
    {
        return Patient.Create(
            PatientId.Create(id), name, Sex.Male, 30, AgeUnit.Year, when,
            pickupDateUtc: when.AddHours(4));
    }

    private static void AddPhone(FakeApplicationDbContext db, int phoneId, int patientId, string number, byte sortOrder)
    {
        db.PatientPhoneNumbers.Add(PatientPhoneNumber.Create(
            PatientPhoneNumberId.Create(phoneId), PatientId.Create(patientId), number, sortOrder));
    }

    [Fact]
    public async Task Returns_FullDetail_WithResolvedNamesAndOrderings()
    {
        var db = new FakeApplicationDbContext();
        var now = DateTime.UtcNow;
        var p = MakePatient(1, "Detail", now);
        db.Patients.Add(p);

        db.MedicalConditionTypes.Add(MedicalConditionType.Create(MedicalConditionTypeId.Create(7), "Diabetes", MedicalConditionCategory.Condition));
        db.MedicalConditionTypes.Add(MedicalConditionType.Create(MedicalConditionTypeId.Create(9), "Hypertension", MedicalConditionCategory.Condition));
        // joins added in reverse id order to prove ordering by MedicalConditionTypeId
        db.PatientMedicalConditions.Add(PatientMedicalCondition.Create(PatientId.Create(1), MedicalConditionTypeId.Create(9)));
        db.PatientMedicalConditions.Add(PatientMedicalCondition.Create(PatientId.Create(1), MedicalConditionTypeId.Create(7)));

        AddPhone(db, 1, 1, "01098765432", 1);
        AddPhone(db, 2, 1, "01012345678", 0);

        db.Tests.Add(Test.Create(TestId.Create(10), "CBC", "CBC", "CBC", "T-CBC", 60, 100m));

        var delivered = PatientTest.Create(PatientTestId.Create(103), PatientId.Create(1), TestId.Create(10), 150m);
        delivered.MarkEntered(5, now);
        delivered.MarkReviewed(5, now);
        delivered.MarkPrinted(5, now);
        delivered.MarkDelivered(5, now);
        db.PatientTests.Add(delivered);

        // a flagged, delivered unpublished line to assert ResultValue / ResultFlag projection
        var flagged = PatientTest.Create(PatientTestId.Create(101), PatientId.Create(1), TestId.Create(10), 60m);
        flagged.EnterResult("5.4", ResultFlag.High, 5, now);
        db.PatientTests.Add(flagged);

        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(102), PatientId.Create(1), TestId.Create(10), 80m));

        var handler = new GetVisitDetailQueryHandler(db);
        var result = await handler.Handle(new GetVisitDetailQuery(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var detail = result.Value!;
        Assert.Equal(1, detail.PatientId);
        Assert.Equal("Detail", detail.FullName);
        Assert.Equal("Male", detail.Sex);
        Assert.Equal("Year", detail.AgeUnit);
        Assert.Equal(30, detail.AgeValue);
        Assert.Equal("Individual", detail.AccountType);
        Assert.Equal(now.AddHours(4), detail.PickupDateUtc);

        Assert.Equal(new[] { "01012345678", "01098765432" }, detail.PhoneNumbers);
        Assert.Equal(new[] { "Diabetes", "Hypertension" }, detail.MedicalConditionNames);

        Assert.Equal(3, detail.Tests.Count);
        Assert.Equal(new[] { 101, 102, 103 }, detail.Tests.Select(t => t.PatientTestId).ToArray());
        Assert.Equal("CBC", detail.Tests[0].TestName);
        Assert.Equal("T-CBC", detail.Tests[0].TestCode);
        Assert.Equal(60m, detail.Tests[0].PriceAtOrderTime);
        Assert.Equal("5.4", detail.Tests[0].ResultValue);
        Assert.Equal((int)ResultFlag.High, detail.Tests[0].ResultFlag);
        Assert.False(detail.Tests[0].IsReviewed);
        Assert.Null(detail.Tests[1].ResultValue);
        Assert.True(detail.Tests[2].IsDelivered);
        Assert.Equal(150m, detail.Tests[2].PriceAtOrderTime);

        // charged = 150 + 60 + 80 = 290; no payments
        Assert.Equal(290m, detail.Balance);
    }

    [Fact]
    public async Task MissingOrDeleted_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        var deleted = MakePatient(1, "Gone", DateTime.UtcNow);
        deleted.SoftDelete();
        db.Patients.Add(deleted);

        var handler = new GetVisitDetailQueryHandler(db);

        var missing = await handler.Handle(new GetVisitDetailQuery(999), CancellationToken.None);
        Assert.False(missing.IsSuccess);
        Assert.Equal(ErrorType.NotFound, missing.Error!.Type);
        Assert.Equal("المريض غير موجود.", missing.Error!.Message);

        var deletedResult = await handler.Handle(new GetVisitDetailQuery(1), CancellationToken.None);
        Assert.False(deletedResult.IsSuccess);
        Assert.Equal(ErrorType.NotFound, deletedResult.Error!.Type);
    }
}