using TopLab.Application.Common.Results;
using TopLab.Application.Features.ResultsEntry.Commands.MarkResultPrinted;
using TopLab.Application.Features.ResultsEntry.Common;
using TopLab.Application.Tests.Common.Fakes;
using Xunit;

namespace TopLab.Application.Tests.Features.ResultsEntry;

/// <summary>
/// Hand-rolled <see cref="IResultPrintCoordinator"/> for S5 view-model and SD-16 wiring tests.
/// No mocking library (house rule).
/// </summary>
public sealed class FakeResultPrintCoordinator : IResultPrintCoordinator
{
    public List<(int PatientTestId, ResultPrintKind Kind)> Calls { get; } = new();

    public bool Printed { get; set; } = true;

    public string? ErrorMessage { get; set; }

    /// <summary>W-02 S6: fails only this patient test id, so one row in a batch can be made to fail.</summary>
    public int? FailForPatientTestId { get; set; }

    public Task<ResultPrintOutcome> PrintAsync(
        int patientTestId, ResultPrintKind kind, CancellationToken ct = default)
    {
        Calls.Add((patientTestId, kind));
        var ok = Printed && FailForPatientTestId != patientTestId;
        return Task.FromResult(new ResultPrintOutcome(
            patientTestId, kind, ok, ok ? null : ErrorMessage ?? "تعذر طباعة التقرير."));
    }
}

/// <summary>
/// W-02 S5 / SD-16 (C-21): the command is <b>wired</b> to the honest coordinator, not deleted,
/// and it never marks. Also proves the binding Arabic failure text.
/// </summary>
public class MarkResultPrintedWiringTests
{
    [Fact]
    public async Task MarkResultPrinted_WiresThroughCoordinator_WithSimpleResult()
    {
        var db = SeedReviewed();
        var coordinator = new FakeResultPrintCoordinator { Printed = true };
        var handler = new MarkResultPrintedCommandHandler(db, new FakeCurrentUserService(), coordinator);

        var result = await handler.Handle(new MarkResultPrintedCommand(101), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var call = Assert.Single(coordinator.Calls);
        Assert.Equal(101, call.PatientTestId);
        Assert.Equal(ResultPrintKind.SimpleResult, call.Kind);
    }

    [Fact]
    public async Task MarkResultPrinted_WhenPrintFails_ReturnsArabicErrorAndNoSuccess()
    {
        var db = SeedReviewed();
        var coordinator = new FakeResultPrintCoordinator
        {
            Printed = false,
            ErrorMessage = "تعذر طباعة التقرير."
        };
        var handler = new MarkResultPrintedCommandHandler(db, new FakeCurrentUserService(), coordinator);

        var result = await handler.Handle(new MarkResultPrintedCommand(101), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("تعذر طباعة التقرير.", result.Error!.Message);
    }

    /// <summary>SD-1: the wired handler must not write IsPrinted/PrintCount.</summary>
    [Fact]
    public async Task MarkResultPrinted_Wired_NeverMarksTheRow()
    {
        var db = SeedReviewed();
        var handler = new MarkResultPrintedCommandHandler(
            db, new FakeCurrentUserService(), new FakeResultPrintCoordinator());

        await handler.Handle(new MarkResultPrintedCommand(101), CancellationToken.None);

        var row = db.PatientTests.Single();
        Assert.False(row.IsPrinted);
        Assert.Equal(0, row.PrintCount);
    }

    [Fact]
    public async Task MarkResultPrinted_Unreviewed_StillRejectedByExistingGuard()
    {
        var db = SeedUnreviewed();
        var coordinator = new FakeResultPrintCoordinator();
        var handler = new MarkResultPrintedCommandHandler(db, new FakeCurrentUserService(), coordinator);

        var result = await handler.Handle(new MarkResultPrintedCommand(101), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Empty(coordinator.Calls);
    }

    private static FakeApplicationDbContext SeedUnreviewed()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(TopLab.Domain.Patients.Patient.Create(
            TopLab.Domain.Common.Ids.PatientId.Create(1), "Patient",
            TopLab.Domain.Common.Enums.Sex.Male, 30,
            TopLab.Domain.Common.Enums.AgeUnit.Year, System.DateTime.UtcNow));
        db.PatientTests.Add(UnreviewedRow());
        return db;
    }

    private static FakeApplicationDbContext SeedReviewed()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(TopLab.Domain.Patients.Patient.Create(
            TopLab.Domain.Common.Ids.PatientId.Create(1), "Patient",
            TopLab.Domain.Common.Enums.Sex.Male, 30,
            TopLab.Domain.Common.Enums.AgeUnit.Year, System.DateTime.UtcNow));
        var row = EnteredRow();
        row.MarkReviewed(5, System.DateTime.UtcNow);
        db.PatientTests.Add(row);
        return db;
    }

    private static TopLab.Domain.Results.PatientTest UnreviewedRow() => EnteredRow();

    /// <summary>A row with a result entered — reviewing requires it (PatientTest.MarkReviewed).</summary>
    private static TopLab.Domain.Results.PatientTest EnteredRow()
    {
        var pt = TopLab.Domain.Results.PatientTest.Create(
            TopLab.Domain.Common.Ids.PatientTestId.Create(101),
            TopLab.Domain.Common.Ids.PatientId.Create(1),
            TopLab.Domain.Common.Ids.TestId.Create(1),
            100m);
        pt.EnterResult("5", TopLab.Domain.Common.Enums.ResultFlag.Normal, 1, System.DateTime.UtcNow);
        return pt;
    }
}
