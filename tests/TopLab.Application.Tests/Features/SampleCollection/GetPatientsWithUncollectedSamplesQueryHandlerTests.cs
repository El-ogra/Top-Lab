using TopLab.Application.Features.SampleCollection.Queries.GetPatientsWithUncollectedSamples;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using Xunit;

namespace TopLab.Application.Tests.Features.SampleCollection;

public class GetPatientsWithUncollectedSamplesQueryHandlerTests
{
    private static Patient MakePatient(int id, string name, DateTime registrationUtc)
    {
        return Patient.Create(PatientId.Create(id), name, Sex.Male, 30, AgeUnit.Year, registrationUtc);
    }

    private static PatientTest MakeTest(int ptId, int patientId, bool outside = false, bool drawn = false)
    {
        var pt = PatientTest.Create(
            PatientTestId.Create(ptId), PatientId.Create(patientId), TestId.Create(10), 100m,
            isTakenOutsideLab: outside);
        if (drawn)
        {
            pt.MarkSampleDrawn(DateTime.UtcNow);
        }
        return pt;
    }

    [Fact]
    public async Task Returns_PatientsWithUndrawnInLabTests_OrderedByRegistrationAsc()
    {
        var db = new FakeApplicationDbContext();
        var now = DateTime.UtcNow;
        var early = MakePatient(1, "Early", now.AddHours(-3));
        var late = MakePatient(2, "Late", now.AddHours(-1));
        db.Patients.Add(early);
        db.Patients.Add(late);
        db.PatientTests.Add(MakeTest(11, 2));
        db.PatientTests.Add(MakeTest(12, 1));
        db.PatientTests.Add(MakeTest(13, 1));

        var handler = new GetPatientsWithUncollectedSamplesQueryHandler(db);
        var result = await handler.Handle(
            new GetPatientsWithUncollectedSamplesQuery(null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
        Assert.Equal(1, result.Value[0].PatientId);
        Assert.Equal(2, result.Value[0].UndrawnCount);
        Assert.Equal(2, result.Value[1].PatientId);
        Assert.Equal(1, result.Value[1].UndrawnCount);
    }

    [Fact]
    public async Task Excludes_PatientWithOnlyOutsideDrawnTests_FR_M21_001()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1, "Outside", DateTime.UtcNow));
        db.PatientTests.Add(MakeTest(11, 1, outside: true));

        var handler = new GetPatientsWithUncollectedSamplesQueryHandler(db);
        var result = await handler.Handle(
            new GetPatientsWithUncollectedSamplesQuery(null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }

    [Fact]
    public async Task Excludes_PatientWithOnlyDrawnTests()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1, "Drawn", DateTime.UtcNow));
        db.PatientTests.Add(MakeTest(11, 1, drawn: true));

        var handler = new GetPatientsWithUncollectedSamplesQueryHandler(db);
        var result = await handler.Handle(
            new GetPatientsWithUncollectedSamplesQuery(null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }

    [Fact]
    public async Task Excludes_SoftDeletedPatients()
    {
        var db = new FakeApplicationDbContext();
        var deleted = MakePatient(1, "Deleted", DateTime.UtcNow);
        deleted.SoftDelete();
        db.Patients.Add(deleted);
        db.PatientTests.Add(MakeTest(11, 1));

        var handler = new GetPatientsWithUncollectedSamplesQueryHandler(db);
        var result = await handler.Handle(
            new GetPatientsWithUncollectedSamplesQuery(null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }

    [Fact]
    public async Task DayFilter_DefaultsToToday_AndExplicitDayWorks()
    {
        var db = new FakeApplicationDbContext();
        var now = DateTime.UtcNow;
        db.Patients.Add(MakePatient(1, "Today", now));
        db.Patients.Add(MakePatient(2, "Yesterday", now.AddDays(-1)));
        db.PatientTests.Add(MakeTest(11, 1));
        db.PatientTests.Add(MakeTest(12, 2));

        var handler = new GetPatientsWithUncollectedSamplesQueryHandler(db);

        var today = await handler.Handle(
            new GetPatientsWithUncollectedSamplesQuery(null), CancellationToken.None);
        Assert.True(today.IsSuccess);
        Assert.Single(today.Value!);
        Assert.Equal(1, today.Value![0].PatientId);

        var yesterday = await handler.Handle(
            new GetPatientsWithUncollectedSamplesQuery(DateOnly.FromDateTime(now.AddDays(-1))), CancellationToken.None);
        Assert.True(yesterday.IsSuccess);
        Assert.Single(yesterday.Value!);
        Assert.Equal(2, yesterday.Value![0].PatientId);
    }
}
