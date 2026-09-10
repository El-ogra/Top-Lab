using TopLab.Application.Features.PatientSearch.Queries.SearchPatientsGlobal;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Settings;
using Xunit;

namespace TopLab.Application.Tests.Features.PatientSearch;

public class SearchPatientsGlobalQueryHandlerTests
{
    private static Patient MakePatient(int id, string name, DateTime registrationUtc,
        string? labId = null, string? nationalId = null)
    {
        var p = Patient.Create(PatientId.Create(id), name, Sex.Male, 30, AgeUnit.Year, registrationUtc, nationalId: nationalId);
        if (labId != null)
        {
            p.AssignLabId(LabId.Create(labId));
        }
        return p;
    }

    private static void AddPhone(FakeApplicationDbContext db, int phoneId, int patientId, string number, byte sortOrder)
    {
        db.PatientPhoneNumbers.Add(PatientPhoneNumber.Create(
            PatientPhoneNumberId.Create(phoneId), PatientId.Create(patientId), number, sortOrder));
    }

    private static SystemSettings WithNameAssist(bool enabled)
    {
        var settings = SystemSettings.CreateDefault();
        settings.SetGeneralFlags(
            saveTreatingDoctorOnlyFromEntityWindow: false,
            enablePatientNameSearchAssist: enabled,
            disableAutoTitleInsertion: false,
            printFileExternalBarcode: false,
            printDateTimeOnTubeBarcode: false,
            printLabIdInsteadOfPatientId: false,
            autoReviewAndComplete: false,
            printAccountInsteadOfDateOnReport: false);
        return settings;
    }

    [Fact]
    public async Task EmptyText_ReturnsMostRecentRegistrations_ExcludesSoftDeleted()
    {
        var db = new FakeApplicationDbContext();
        var now = DateTime.UtcNow;
        var old = MakePatient(1, "Old", now.AddDays(-3));
        var middle = MakePatient(2, "Middle", now.AddDays(-2));
        var newest = MakePatient(3, "New", now.AddDays(-1));
        var deleted = MakePatient(4, "Deleted", now);
        deleted.SoftDelete();
        db.Patients.Add(old);
        db.Patients.Add(middle);
        db.Patients.Add(newest);
        db.Patients.Add(deleted);

        var handler = new SearchPatientsGlobalQueryHandler(db);
        var result = await handler.Handle(new SearchPatientsGlobalQuery(null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value!.Count);
        Assert.Equal(3, result.Value[0].PatientId);
        Assert.Equal(2, result.Value[1].PatientId);
        Assert.Equal(1, result.Value[2].PatientId);
    }

    [Fact]
    public async Task Search_ByName_CaseInsensitiveSubstring_WhenAssistEnabled()
    {
        var db = new FakeApplicationDbContext();
        db.SystemSettings.Add(WithNameAssist(true));
        db.Patients.Add(MakePatient(1, "Ahmed Mohamed", DateTime.UtcNow));
        db.Patients.Add(MakePatient(2, "Sara Ali", DateTime.UtcNow));

        var handler = new SearchPatientsGlobalQueryHandler(db);
        var result = await handler.Handle(new SearchPatientsGlobalQuery("AHMED"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var hit = Assert.Single(result.Value!);
        Assert.Equal(1, hit.PatientId);
        Assert.Equal("Ahmed Mohamed", hit.FullName);
    }

    [Fact]
    public async Task Search_ByNameIgnored_WhenAssistDisabled_MissingSettingsDefaultsOff()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1, "Ahmed Mohamed", DateTime.UtcNow));

        var handler = new SearchPatientsGlobalQueryHandler(db);
        var result = await handler.Handle(new SearchPatientsGlobalQuery("Ahmed"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }

    [Fact]
    public async Task Search_ByNameIgnored_WhenAssistExplicitlyOff_ButLabIdStillMatches()
    {
        var db = new FakeApplicationDbContext();
        db.SystemSettings.Add(WithNameAssist(false));
        db.Patients.Add(MakePatient(1, "Ahmed Mohamed", DateTime.UtcNow, labId: "LAB-1"));

        var handler = new SearchPatientsGlobalQueryHandler(db);

        var byName = await handler.Handle(new SearchPatientsGlobalQuery("Ahmed"), CancellationToken.None);
        Assert.True(byName.IsSuccess);
        Assert.Empty(byName.Value!);

        var byLabId = await handler.Handle(new SearchPatientsGlobalQuery("LAB-1"), CancellationToken.None);
        Assert.True(byLabId.IsSuccess);
        Assert.Single(byLabId.Value!);
    }

    [Fact]
    public async Task Search_ByLabId_ExactTrimmedMatch_NotSubstring()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1, "Ahmed", DateTime.UtcNow, labId: "LAB-1"));

        var handler = new SearchPatientsGlobalQueryHandler(db);

        var exact = await handler.Handle(new SearchPatientsGlobalQuery("  LAB-1  "), CancellationToken.None);
        Assert.True(exact.IsSuccess);
        Assert.Single(exact.Value!);
        Assert.Equal("LAB-1", exact.Value![0].LabId);

        var substring = await handler.Handle(new SearchPatientsGlobalQuery("LAB"), CancellationToken.None);
        Assert.True(substring.IsSuccess);
        Assert.Empty(substring.Value!);
    }

    [Fact]
    public async Task Search_ByNationalId_ExactTrimmedMatch_NotSubstring()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1, "Ahmed", DateTime.UtcNow, nationalId: "29501010101010"));

        var handler = new SearchPatientsGlobalQueryHandler(db);

        var exact = await handler.Handle(new SearchPatientsGlobalQuery("29501010101010"), CancellationToken.None);
        Assert.True(exact.IsSuccess);
        Assert.Single(exact.Value!);

        var substring = await handler.Handle(new SearchPatientsGlobalQuery("2950101"), CancellationToken.None);
        Assert.True(substring.IsSuccess);
        Assert.Empty(substring.Value!);
    }

    [Fact]
    public async Task Search_ByAnyPhoneNumber_Substring_AnyStoredNumber()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1, "Ahmed", DateTime.UtcNow));
        AddPhone(db, 1, 1, "01012345678", 0);
        AddPhone(db, 2, 1, "01098765432", 1);

        var handler = new SearchPatientsGlobalQueryHandler(db);

        var first = await handler.Handle(new SearchPatientsGlobalQuery("12345678"), CancellationToken.None);
        Assert.True(first.IsSuccess);
        Assert.Single(first.Value!);

        var second = await handler.Handle(new SearchPatientsGlobalQuery("98765432"), CancellationToken.None);
        Assert.True(second.IsSuccess);
        Assert.Single(second.Value!);
    }

    [Fact]
    public async Task Search_Hit_CarriesPhoneList()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1, "Ahmed", DateTime.UtcNow));
        AddPhone(db, 1, 1, "01012345678", 1);
        AddPhone(db, 2, 1, "01098765432", 0);

        var handler = new SearchPatientsGlobalQueryHandler(db);
        var result = await handler.Handle(new SearchPatientsGlobalQuery("01012345678"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var hit = Assert.Single(result.Value!);
        Assert.Equal(new[] { "01098765432", "01012345678" }, hit.PhoneNumbers);
    }

    [Fact]
    public async Task Search_ExcludesSoftDeleted()
    {
        var db = new FakeApplicationDbContext();
        db.SystemSettings.Add(WithNameAssist(true));
        var deleted = MakePatient(1, "Ahmed Mohamed", DateTime.UtcNow);
        deleted.SoftDelete();
        db.Patients.Add(deleted);
        var live = MakePatient(2, "Ahmed Mohamed", DateTime.UtcNow.AddDays(-1));
        db.Patients.Add(live);

        var handler = new SearchPatientsGlobalQueryHandler(db);
        var result = await handler.Handle(new SearchPatientsGlobalQuery("Ahmed"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var hit = Assert.Single(result.Value!);
        Assert.Equal(2, hit.PatientId);
    }

    [Fact]
    public async Task Search_Pagination_SkipTake()
    {
        var db = new FakeApplicationDbContext();
        for (var i = 1; i <= 10; i++)
        {
            db.Patients.Add(MakePatient(i, $"P{i:D2}", DateTime.UtcNow.AddMinutes(-i)));
        }

        var handler = new SearchPatientsGlobalQueryHandler(db);

        var page1 = await handler.Handle(new SearchPatientsGlobalQuery(null, 1, 3), CancellationToken.None);
        Assert.True(page1.IsSuccess);
        Assert.Equal(3, page1.Value!.Count);
        Assert.Equal(new[] { 1, 2, 3 }, page1.Value.Select(h => h.PatientId).ToArray());

        var page4 = await handler.Handle(new SearchPatientsGlobalQuery(null, 4, 3), CancellationToken.None);
        Assert.True(page4.IsSuccess);
        var hit = Assert.Single(page4.Value!);
        Assert.Equal(10, hit.PatientId);
    }

    [Fact]
    public async Task Search_Hit_CarriesAggregateStatus_AndTestCount_MixedStages()
    {
        var db = new FakeApplicationDbContext();
        db.SystemSettings.Add(WithNameAssist(true));
        // registered yesterday so the S1 masking (registration today) never applies
        var patient = MakePatient(1, "Mixed", DateTime.UtcNow.AddDays(-1));
        db.Patients.Add(patient);
        var now = DateTime.UtcNow;

        // one delivered (stage 6); one not entered (stage 1) -> min = 1 -> S2
        var delivered = PatientTest.Create(PatientTestId.Create(1), PatientId.Create(1), TestId.Create(10), 100m);
        delivered.MarkEntered(5, now);
        delivered.MarkReviewed(5, now);
        delivered.MarkPrinted(5, now);
        delivered.MarkDelivered(5, now);
        db.PatientTests.Add(delivered);

        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(2), PatientId.Create(1), TestId.Create(11), 50m));

        var handler = new SearchPatientsGlobalQueryHandler(db);
        var result = await handler.Handle(new SearchPatientsGlobalQuery("Mixed"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var hit = Assert.Single(result.Value!);
        Assert.Equal(2, hit.TestCount);
        Assert.Equal(2, hit.AggregateStatus);
        Assert.Equal("Individual", hit.AccountType);
    }
}