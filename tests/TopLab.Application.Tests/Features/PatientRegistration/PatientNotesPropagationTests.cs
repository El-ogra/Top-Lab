using TopLab.Application.Features.ResultDelivery.Queries.GetUndeliveredResults;
using TopLab.Application.Features.ResultsEntry.Queries.GetResultEntry;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.PatientRegistration;

/// <summary>
/// STAGE-01 (baseline 1.2.2.4.1): the patient's notes reach the result-entry
/// and the result-delivery patient data. The rendering of those notes in the
/// two windows belongs to STAGE-06 and STAGE-10.
/// </summary>
public class PatientNotesPropagationTests
{
    private static readonly DateTime Day = new(2026, 3, 10, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task ResultEntry_CarriesThePatientNotes()
    {
        var db = new FakeApplicationDbContext();
        var patient = Patient.Create(
            PatientId.Create(1), "Ahmed", Sex.Male, 30, AgeUnit.Year, Day, notes: "حساسية من البنسلين");
        db.Patients.Add(patient);
        db.Tests.Add(Test.Create(TestId.Create(10), "CBC", "CBC", "CBC", "T10", 30, 100m));
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(100), PatientId.Create(1), TestId.Create(10), 100m));

        var handler = new GetResultEntryQueryHandler(db);
        var result = await handler.Handle(new GetResultEntryQuery(100), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("حساسية من البنسلين", result.Value!.PatientNotes);
    }

    [Fact]
    public async Task ResultEntry_WithoutNotes_CarriesNull()
    {
        var db = new FakeApplicationDbContext();
        var patient = Patient.Create(PatientId.Create(1), "Ahmed", Sex.Male, 30, AgeUnit.Year, Day);
        db.Patients.Add(patient);
        db.Tests.Add(Test.Create(TestId.Create(10), "CBC", "CBC", "CBC", "T10", 30, 100m));
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(100), PatientId.Create(1), TestId.Create(10), 100m));

        var handler = new GetResultEntryQueryHandler(db);
        var result = await handler.Handle(new GetResultEntryQuery(100), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value!.PatientNotes);
    }

    [Fact]
    public async Task UndeliveredPatients_CarryThePatientNotes()
    {
        var db = new FakeApplicationDbContext();
        var patient = Patient.Create(
            PatientId.Create(1), "Ahmed", Sex.Male, 30, AgeUnit.Year, Day, notes: "يُفضّل الاتصال قبل التسليم");
        db.Patients.Add(patient);
        db.Tests.Add(Test.Create(TestId.Create(10), "CBC", "CBC", "CBC", "T10", 30, 100m));
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(100), PatientId.Create(1), TestId.Create(10), 100m));

        var handler = new GetUndeliveredResultsQueryHandler(db);
        var result = await handler.Handle(
            new GetUndeliveredResultsQuery(DateOnly.FromDateTime(Day), DateOnly.FromDateTime(Day)),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var row = Assert.Single(result.Value!);
        Assert.Equal("يُفضّل الاتصال قبل التسليم", row.PatientNotes);
    }

    [Fact]
    public async Task UndeliveredPatients_WithoutNotes_CarryNull()
    {
        var db = new FakeApplicationDbContext();
        var patient = Patient.Create(PatientId.Create(1), "Ahmed", Sex.Male, 30, AgeUnit.Year, Day);
        db.Patients.Add(patient);
        db.Tests.Add(Test.Create(TestId.Create(10), "CBC", "CBC", "CBC", "T10", 30, 100m));
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(100), PatientId.Create(1), TestId.Create(10), 100m));

        var handler = new GetUndeliveredResultsQueryHandler(db);
        var result = await handler.Handle(
            new GetUndeliveredResultsQuery(DateOnly.FromDateTime(Day), DateOnly.FromDateTime(Day)),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var row = Assert.Single(result.Value!);
        Assert.Null(row.PatientNotes);
    }
}
