using TopLab.Application.Common.Results;
using TopLab.Application.Features.SampleCollection.Commands.MarkSampleDrawn;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using Xunit;

namespace TopLab.Application.Tests.Features.SampleCollection;

public class MarkSampleDrawnCommandHandlerTests
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
    public async Task HappyPath_SetsDrawnFlagAndTimestamp_SavesOnce()
    {
        var db = new FakeApplicationDbContext();
        var clock = new FakeDateTimeProvider { UtcNow = new DateTime(2026, 9, 7, 10, 0, 0, DateTimeKind.Utc) };
        db.Patients.Add(MakePatient(1));
        var pt = MakePatientTest(100, 1);
        db.PatientTests.Add(pt);

        var handler = new MarkSampleDrawnCommandHandler(db, clock);
        var result = await handler.Handle(
            new MarkSampleDrawnCommand(100, clock.UtcNow), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(pt.IsSampleDrawn);
        Assert.Equal(clock.UtcNow, pt.SampleDrawnAtUtc);
        Assert.Equal(1, db.SaveChangesCallCount);
    }

    [Fact]
    public async Task UnknownTest_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        var handler = new MarkSampleDrawnCommandHandler(db, new FakeDateTimeProvider());

        var result = await handler.Handle(
            new MarkSampleDrawnCommand(99, DateTime.UtcNow), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }

    [Fact]
    public async Task TestOnSoftDeletedPatient_ReturnsConflict()
    {
        var db = new FakeApplicationDbContext();
        var deleted = MakePatient(1);
        deleted.SoftDelete();
        db.Patients.Add(deleted);
        db.PatientTests.Add(MakePatientTest(100, 1));

        var handler = new MarkSampleDrawnCommandHandler(db, new FakeDateTimeProvider());
        var result = await handler.Handle(
            new MarkSampleDrawnCommand(100, DateTime.UtcNow), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
    }

    [Fact]
    public async Task OutsideDrawnTest_ReturnsConflict_WithSpecificMessage_FR_M21_001()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1));
        db.PatientTests.Add(MakePatientTest(100, 1, outside: true));

        var handler = new MarkSampleDrawnCommandHandler(db, new FakeDateTimeProvider());
        var result = await handler.Handle(
            new MarkSampleDrawnCommand(100, DateTime.UtcNow), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Equal("تم تسجيل العينة كمسحوبة خارج المعمل؛ لا يمكن تعديلها من شاشة السحب", result.Error!.Message);
    }

    [Fact]
    public async Task DoubleDraw_IsIdempotentSuccess_WithoutExtraSave()
    {
        var db = new FakeApplicationDbContext();
        var clock = new FakeDateTimeProvider();
        db.Patients.Add(MakePatient(1));
        db.PatientTests.Add(MakePatientTest(100, 1, drawn: true));

        var handler = new MarkSampleDrawnCommandHandler(db, clock);
        var result = await handler.Handle(
            new MarkSampleDrawnCommand(100, clock.UtcNow), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, db.SaveChangesCallCount);
    }
}
