using TopLab.Application.Features.PatientRegistration.Queries.SearchPatients;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using Xunit;

namespace TopLab.Application.Tests.Features.PatientRegistration.Queries;

public class SearchPatientsQueryHandlerTests
{
    private static Patient MakePatient(int id, string name, string? labId = null, string? nationalId = null)
    {
        var p = Patient.Create(PatientId.Create(id), name, Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow, nationalId: nationalId);
        if (labId != null)
        {
            p.AssignLabId(LabId.Create(labId));
        }
        return p;
    }

    [Fact]
    public async Task Search_NoTerm_ReturnsAllNonDeleted()
    {
        var db = new FakeApplicationDbContext();
        var a = MakePatient(1, "Ahmed");
        var b = MakePatient(2, "Sara");
        var deleted = MakePatient(3, "DeletedOne");
        deleted.SoftDelete();
        db.Patients.Add(a);
        db.Patients.Add(b);
        db.Patients.Add(deleted);

        var handler = new SearchPatientsQueryHandler(db);
        var result = await handler.Handle(new SearchPatientsQuery(null, 1, 50), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
    }

    [Fact]
    public async Task Search_ByName_Substring_Matches()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1, "Ahmed Mohamed"));
        db.Patients.Add(MakePatient(2, "Sara Ali"));
        db.Patients.Add(MakePatient(3, "Mona Ahmed"));

        var handler = new SearchPatientsQueryHandler(db);
        var result = await handler.Handle(new SearchPatientsQuery("Ahmed", 1, 50), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
    }

    [Fact]
    public async Task Search_ByNationalId_Matches()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1, "Ahmed", nationalId: "29501010101010"));
        db.Patients.Add(MakePatient(2, "Sara", nationalId: "29502020202020"));

        var handler = new SearchPatientsQueryHandler(db);
        var result = await handler.Handle(new SearchPatientsQuery("2950101", 1, 50), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
        Assert.Equal(1, result.Value![0].PatientId);
    }

    [Fact]
    public async Task Search_ByLabId_Matches()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1, "Ahmed", labId: "LAB-1"));
        db.Patients.Add(MakePatient(2, "Sara", labId: "LAB-2"));

        var handler = new SearchPatientsQueryHandler(db);
        var result = await handler.Handle(new SearchPatientsQuery("LAB-1", 1, 50), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
        Assert.Equal("LAB-1", result.Value![0].LabId);
    }

    [Fact]
    public async Task Search_ByAnyPhoneNumber_Br03Invariant()
    {
        var db = new FakeApplicationDbContext();
        var patient = MakePatient(1, "Ahmed");
        patient.SetPhoneNumbers(new[]
        {
            new PatientNumberInput("01012345678", 0),
            new PatientNumberInput("01098765432", 1)
        });
        db.Patients.Add(patient);
        db.PatientPhoneNumbers.Add(PatientPhoneNumber.Create(PatientPhoneNumberId.Create(1), PatientId.Create(1), "01012345678", 0));
        db.PatientPhoneNumbers.Add(PatientPhoneNumber.Create(PatientPhoneNumberId.Create(2), PatientId.Create(1), "01098765432", 1));
        db.Patients.Add(MakePatient(2, "Other"));

        var handler = new SearchPatientsQueryHandler(db);

        var result = await handler.Handle(new SearchPatientsQuery("01012345678", 1, 50), CancellationToken.None);
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);

        var result2 = await handler.Handle(new SearchPatientsQuery("01098765432", 1, 50), CancellationToken.None);
        Assert.True(result2.IsSuccess);
        Assert.Single(result2.Value!);
    }

    [Fact]
    public async Task Search_ExcludesSoftDeleted()
    {
        var db = new FakeApplicationDbContext();
        var p = MakePatient(1, "Ahmed");
        p.SoftDelete();
        db.Patients.Add(p);

        var handler = new SearchPatientsQueryHandler(db);
        var result = await handler.Handle(new SearchPatientsQuery(null, 1, 50), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }

    [Fact]
    public async Task Search_Pagination_AppliesSkipTake()
    {
        var db = new FakeApplicationDbContext();
        for (int i = 0; i < 10; i++)
        {
            db.Patients.Add(MakePatient(i + 1, $"P{i:D2}"));
        }

        var handler = new SearchPatientsQueryHandler(db);

        var page1 = await handler.Handle(new SearchPatientsQuery(null, 1, 3), CancellationToken.None);
        Assert.Equal(3, page1.Value!.Count);

        var page2 = await handler.Handle(new SearchPatientsQuery(null, 2, 3), CancellationToken.None);
        Assert.Equal(3, page2.Value!.Count);

        var page4 = await handler.Handle(new SearchPatientsQuery(null, 4, 3), CancellationToken.None);
        Assert.Single(page4.Value!);
    }

    [Fact]
    public async Task Search_EmptyDatabase_ReturnsEmpty()
    {
        var db = new FakeApplicationDbContext();
        var handler = new SearchPatientsQueryHandler(db);

        var result = await handler.Handle(new SearchPatientsQuery("anything", 1, 50), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }
}