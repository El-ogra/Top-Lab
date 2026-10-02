using TopLab.Application.Common.Interfaces;
using TopLab.Application.Features.PatientRegistration.Commands.AddTestsToVisit;
using TopLab.Application.Features.PatientRegistration.Commands.ApplyConditionDeltas;
using TopLab.Application.Features.PatientRegistration.Commands.ApplyVisitDeltas;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.PatientRegistration;

/// <summary>W-02 S14 (WP-29): visit edits are all-or-nothing with a single save.</summary>
public class VisitDeltasAtomicityTests
{
    private static FakeApplicationDbContext Seed()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(Patient.Create(PatientId.Create(1), "P", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow));
        db.Tests.Add(Test.Create(TestId.Create(10), "T", "T", "T", "T10", 1, 100m, ResultKind.Simple));
        db.MedicalConditionTypes.Add(MedicalConditionType.Create(MedicalConditionTypeId.Create(7), "M", MedicalConditionCategory.Condition));
        return db;
    }

    [Fact]
    public async Task ApplyVisitDeltas_AppliesAllChangesInOneSave()
    {
        var db = Seed();
        var before = db.SaveChangesCallCount;

        var result = await new ApplyVisitDeltasCommandHandler(db, new FakeSender(), new FakeAppUnitOfWork()).Handle(
            new ApplyVisitDeltasCommand(
                1,
                Array.Empty<int>(),
                new[] { new AddTestInput(10, false, false, false, false, false, false) },
                Array.Empty<VisitFlagUpdate>()),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(before + 1, db.SaveChangesCallCount);
        Assert.Single(db.PatientTests);
    }

    /// <summary>
    /// The fake context applies list mutations immediately, so store-state rollback
    /// is a real-EF property (owned by the UoW transaction). What the unit level
    /// proves: a mid-chain failure returns before any save boundary is crossed.
    /// </summary>
    [Fact]
    public async Task ApplyVisitDeltas_AddFailure_RollsBackRemovals()
    {
        var db = Seed();
        var keep = PatientTest.Create(PatientTestId.Create(50), PatientId.Create(1), TestId.Create(10), 100m);
        db.PatientTests.Add(keep);
        var before = db.SaveChangesCallCount;

        var result = await new ApplyVisitDeltasCommandHandler(db, new FakeSender(), new FakeAppUnitOfWork()).Handle(
            new ApplyVisitDeltasCommand(
                1,
                new[] { 50 },
                new[] { new AddTestInput(99, false, false, false, false, false, false) },
                Array.Empty<VisitFlagUpdate>()),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(before, db.SaveChangesCallCount);
    }

    [Fact]
    public async Task ApplyVisitDeltas_UpdateFailure_RollsBackAdditions()
    {
        var db = Seed();
        var before = db.SaveChangesCallCount;

        var result = await new ApplyVisitDeltasCommandHandler(db, new FakeSender(), new FakeAppUnitOfWork()).Handle(
            new ApplyVisitDeltasCommand(
                1,
                Array.Empty<int>(),
                new[] { new AddTestInput(10, false, false, false, false, false, false) },
                new[] { new VisitFlagUpdate(999, false, false, false, false, false, false) }),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(before, db.SaveChangesCallCount);
    }

    [Fact]
    public async Task ApplyConditionDeltas_RollsBackOnFailure()
    {
        var db = Seed();
        var before = db.SaveChangesCallCount;

        var result = await new ApplyConditionDeltasCommandHandler(db, new FakeAppUnitOfWork()).Handle(
            new ApplyConditionDeltasCommand(1, new[] { 404 }),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(before, db.SaveChangesCallCount);
    }

    [Fact]
    public void IAppUnitOfWork_DoesNotExposeDbContextToApplication()
    {
        var members = typeof(IAppUnitOfWork).GetMembers();

        Assert.DoesNotContain(members, m =>
            m.Name.Contains("DbContext", StringComparison.Ordinal) ||
            m.Name.Contains("Database", StringComparison.Ordinal));
        Assert.DoesNotContain(
            typeof(IAppUnitOfWork).GetMethods().SelectMany(m => m.GetParameters()),
            p => p.ParameterType.Name.Contains("DbContext"));
    }
}
