using TopLab.Application.Features.ResultsEntry.Commands.EnterResult;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Settings;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.ResultsEntry;

public class EnterResultAnalyteBandTests
{
    private static Patient MakePatient(int id = 1, Sex sex = Sex.Male, int age = 30, AgeUnit unit = AgeUnit.Year)
    {
        return Patient.Create(PatientId.Create(id), $"P{id}", sex, age, unit, DateTime.UtcNow);
    }

    private static Test MakeSimpleTest(int id)
    {
        return Test.Create(TestId.Create(id), $"T{id}", $"T{id}", $"T{id}", $"T{id}", 30, 100m, ResultKind.Simple);
    }

    private static (Analyte Analyte, AnalyteReferenceRange Range, AnalyteReferenceRangeBand Band) SeedAnalyteWithBand(
        FakeApplicationDbContext db,
        int analyteId,
        int bandId,
        int? lowBound = null)
    {
        var analyte = Analyte.Create(AnalyteId.Create(analyteId), "Sodium", "Sodium");
        var range = AnalyteReferenceRange.Create(AnalyteReferenceRangeId.Create(analyteId + 10000), analyte.Id);
        var band = AnalyteReferenceRangeBand.Create(
            AnalyteReferenceRangeBandId.Create(bandId),
            range.Id,
            AgeUnit.Year,
            0,
            100,
            lowBound ?? 4m,
            10m,
            null);
        db.Analytes.Add(analyte);
        db.AnalyteReferenceRanges.Add(range);
        db.AnalyteReferenceRangeBands.Add(band);
        return (analyte, range, band);
    }

    private static (FakeApplicationDbContext Db, FakeCurrentUserService User, FakeDateTimeProvider Clock) Build()
    {
        var db = new FakeApplicationDbContext();
        var user = new FakeCurrentUserService { UserId = 7 };
        var clock = new FakeDateTimeProvider { UtcNow = new DateTime(2026, 9, 8, 12, 0, 0, DateTimeKind.Utc) };
        db.SystemSettings.Add(SystemSettings.CreateDefault());
        return (db, user, clock);
    }

    [Fact]
    public async Task Flag_Computed_FromAnalyteBands_NotLegacyRanges()
    {
        var (db, user, clock) = Build();
        db.Patients.Add(MakePatient(1, Sex.Male, 30, AgeUnit.Year));
        var (_, _, _) = SeedAnalyteWithBand(db, 50, 60);
        var test = MakeSimpleTest(10);
        test.MapToAnalyte(AnalyteId.Create(50));
        db.Tests.Add(test);
        // Legacy rows differ; they must be ignored for mapped tests.
        db.ReferenceRanges.Add(ReferenceRange.Create(ReferenceRangeId.Create(1), TestId.Create(10), AgeUnit.Year, 0, 100, 0m, 100m));
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(101), PatientId.Create(1), TestId.Create(10), 100m));

        var handler = new EnterResultCommandHandler(db, user, clock);
        var result = await handler.Handle(new EnterResultCommand(101, "12"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(ResultFlag.High, db.PatientTests[0].ResultFlag);
        var snapshot = Assert.Single(db.PatientTestReferenceRangeSnapshots);
        Assert.Equal(4m, snapshot.MinValue);
        Assert.Equal(10m, snapshot.MaxValue);
    }

    [Fact]
    public async Task MappedTest_NoBands_NoLegacyFallback()
    {
        var (db, user, clock) = Build();
        db.Patients.Add(MakePatient(1, Sex.Male, 30, AgeUnit.Year));
        var analyte = Analyte.Create(AnalyteId.Create(50), "Sodium", "Sodium");
        db.Analytes.Add(analyte);
        var test = MakeSimpleTest(10);
        test.MapToAnalyte(analyte.Id);
        db.Tests.Add(test);
        // Legacy rows exist, but a mapped test must never fall back to them.
        db.ReferenceRanges.Add(ReferenceRange.Create(ReferenceRangeId.Create(1), TestId.Create(10), AgeUnit.Year, 0, 100, 0m, 100m));
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(101), PatientId.Create(1), TestId.Create(10), 100m));

        var handler = new EnterResultCommandHandler(db, user, clock);
        var result = await handler.Handle(new EnterResultCommand(101, "12"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(db.PatientTests[0].ResultFlag);
        Assert.Empty(db.PatientTestReferenceRangeSnapshots);
    }

    [Fact]
    public async Task UnmappedTest_StillUsesLegacyRanges()
    {
        var (db, user, clock) = Build();
        db.Patients.Add(MakePatient(1, Sex.Male, 30, AgeUnit.Year));
        db.Tests.Add(MakeSimpleTest(10));
        db.ReferenceRanges.Add(ReferenceRange.Create(ReferenceRangeId.Create(1), TestId.Create(10), AgeUnit.Year, 0, 100, 4m, 10m));
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(101), PatientId.Create(1), TestId.Create(10), 100m));

        var handler = new EnterResultCommandHandler(db, user, clock);
        var result = await handler.Handle(new EnterResultCommand(101, "12"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(ResultFlag.High, db.PatientTests[0].ResultFlag);
        Assert.Single(db.PatientTestReferenceRangeSnapshots);
    }

    [Fact]
    public async Task StaleMappedSnapshot_Removed_WhenBandsNoLongerMatch()
    {
        var (db, user, clock) = Build();
        db.Patients.Add(MakePatient(1, Sex.Male, 5, AgeUnit.Year));
        var (_, _, _) = SeedAnalyteWithBand(db, 50, 60);
        var test = MakeSimpleTest(10);
        test.MapToAnalyte(AnalyteId.Create(50));
        db.Tests.Add(test);
        var pt = PatientTest.Create(PatientTestId.Create(101), PatientId.Create(1), TestId.Create(10), 100m);
        db.PatientTests.Add(pt);
        var stale = new ReferenceRangeSnapshot(10, null, AgeUnit.Year, 0, 100, 1m, 2m, null, null, DateTimeOffset.UtcNow);
        db.PatientTestReferenceRangeSnapshots.Add(PatientTestReferenceRangeSnapshot.FromSnapshot(PatientTestId.Create(101), stale));

        var handler = new EnterResultCommandHandler(db, user, clock);
        var result = await handler.Handle(new EnterResultCommand(101, "5"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var snapshot = Assert.Single(db.PatientTestReferenceRangeSnapshots);
        Assert.Equal(4m, snapshot.MinValue);
    }
}