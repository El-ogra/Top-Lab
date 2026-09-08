using TopLab.Application.Common.Results;
using TopLab.Application.Features.SampleCollection.Commands.MarkAllSamplesDrawnForPatient;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using Xunit;

namespace TopLab.Application.Tests.Features.SampleCollection;

public class MarkAllSamplesDrawnForPatientCommandHandlerTests
{
    private static Patient MakePatient(int id, string name = "Ahmed")
    {
        return Patient.Create(PatientId.Create(id), name, Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);
    }

    private static PatientTest MakePatientTest(int ptId, int patientId, bool outside = false, bool drawn = false)
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
    public async Task HappyPath_MarksOnlyEligibleRows_SavesExactlyOnce()
    {
        var db = new FakeApplicationDbContext();
        var clock = new FakeDateTimeProvider { UtcNow = new DateTime(2026, 9, 7, 10, 0, 0, DateTimeKind.Utc) };
        db.Patients.Add(MakePatient(1));
        var first = MakePatientTest(100, 1);
        var second = MakePatientTest(101, 1);
        var outside = MakePatientTest(102, 1, outside: true);
        var already = MakePatientTest(103, 1, drawn: true);
        db.PatientTests.Add(first);
        db.PatientTests.Add(second);
        db.PatientTests.Add(outside);
        db.PatientTests.Add(already);

        var handler = new MarkAllSamplesDrawnForPatientCommandHandler(db, clock);
        var result = await handler.Handle(new MarkAllSamplesDrawnForPatientCommand(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value);
        Assert.True(first.IsSampleDrawn);
        Assert.True(second.IsSampleDrawn);
        Assert.Equal(clock.UtcNow, first.SampleDrawnAtUtc);
        Assert.False(outside.IsSampleDrawn);
        Assert.Equal(1, db.SaveChangesCallCount);
    }

    [Fact]
    public async Task NoPending_ReturnsZero_WithoutSaving()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1));
        db.PatientTests.Add(MakePatientTest(100, 1, drawn: true));

        var handler = new MarkAllSamplesDrawnForPatientCommandHandler(db, new FakeDateTimeProvider());
        var result = await handler.Handle(new MarkAllSamplesDrawnForPatientCommand(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value);
        Assert.Equal(0, db.SaveChangesCallCount);
    }

    [Fact]
    public async Task UnknownPatient_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        var handler = new MarkAllSamplesDrawnForPatientCommandHandler(db, new FakeDateTimeProvider());

        var result = await handler.Handle(new MarkAllSamplesDrawnForPatientCommand(99), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }

    [Fact]
    public async Task SoftDeletedPatient_ReturnsConflict()
    {
        var db = new FakeApplicationDbContext();
        var deleted = MakePatient(1);
        deleted.SoftDelete();
        db.Patients.Add(deleted);
        db.PatientTests.Add(MakePatientTest(100, 1));

        var handler = new MarkAllSamplesDrawnForPatientCommandHandler(db, new FakeDateTimeProvider());
        var result = await handler.Handle(new MarkAllSamplesDrawnForPatientCommand(1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
    }
}
