using TopLab.Application.Features.SampleCollection.Queries.GetPatientTestsForDraw;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.SampleCollection;

public class GetPatientTestsForDrawQueryHandlerTests
{
    private static Patient MakePatient(int id, string name)
    {
        return Patient.Create(PatientId.Create(id), name, Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);
    }

    private static void AddTest(FakeApplicationDbContext db, int id, string name)
    {
        db.Tests.Add(Test.Create(TestId.Create(id), name, name, name, $"T{id}", 30, 100m));
    }

    private static PatientTest MakePatientTest(int ptId, int patientId, int testId, bool outside = false, bool drawn = false)
    {
        var pt = PatientTest.Create(
            PatientTestId.Create(ptId), PatientId.Create(patientId), TestId.Create(testId), 100m,
            isTakenOutsideLab: outside);
        if (drawn)
        {
            pt.MarkSampleDrawn(DateTime.UtcNow);
        }
        return pt;
    }

    [Fact]
    public async Task Returns_DrawnAndNotDrawnGroups_WithResolvedTestNames()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1, "Ahmed"));
        AddTest(db, 10, "CBC");
        AddTest(db, 11, "Glucose");
        db.PatientTests.Add(MakePatientTest(100, 1, 10, drawn: true));
        db.PatientTests.Add(MakePatientTest(101, 1, 11));

        var handler = new GetPatientTestsForDrawQueryHandler(db);
        var result = await handler.Handle(new GetPatientTestsForDrawQuery(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value!.PatientId);
        Assert.Equal("Ahmed", result.Value.FullName);
        Assert.Single(result.Value.Drawn);
        Assert.Equal("CBC", result.Value.Drawn[0].TestName);
        Assert.True(result.Value.Drawn[0].IsSampleDrawn);
        Assert.Single(result.Value.NotDrawn);
        Assert.Equal("Glucose", result.Value.NotDrawn[0].TestName);
        Assert.False(result.Value.NotDrawn[0].IsSampleDrawn);
    }

    [Fact]
    public async Task OutsideDrawnRow_StaysInNotDrawn_M21RefusesToDrawIt()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1, "Sara"));
        AddTest(db, 10, "CBC");
        db.PatientTests.Add(MakePatientTest(100, 1, 10, outside: true));

        var handler = new GetPatientTestsForDrawQueryHandler(db);
        var result = await handler.Handle(new GetPatientTestsForDrawQuery(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Drawn);
        Assert.Single(result.Value.NotDrawn);
        Assert.True(result.Value.NotDrawn[0].IsTakenOutsideLab);
    }

    [Fact]
    public async Task UnknownPatient_ReturnsEmptyBoard_EchoingPatientId()
    {
        var db = new FakeApplicationDbContext();

        var handler = new GetPatientTestsForDrawQueryHandler(db);
        var result = await handler.Handle(new GetPatientTestsForDrawQuery(99), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(99, result.Value!.PatientId);
        Assert.Empty(result.Value.Drawn);
        Assert.Empty(result.Value.NotDrawn);
    }
}
