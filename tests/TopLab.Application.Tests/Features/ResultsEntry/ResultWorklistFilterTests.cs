using TopLab.Application.Features.ResultsEntry.Common;
using TopLab.Application.Features.ResultsEntry.Queries.GetResultWorklist;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.ResultsEntry;

/// <summary>
/// S4 — P-01 F6 (results-not-printed) and F7 (results-not-verified) worklist filters.
///
/// F6 is real backend work: <c>IsPrinted</c> had no query parameter and the handler only
/// projected it. The negative case is the important one — <c>IsPrinted = false</c> must select
/// ONLY unprinted rows and must never behave as "no filter" (C-4).
///
/// F7 required no backend change at all: the property and its dispatch already existed (C-3),
/// so these tests exercise the EXISTING parameter and prove the filter narrows.
/// </summary>
public class ResultWorklistFilterTests
{
    private static Patient MakePatient(int id, string name, DateTime registrationUtc)
        => Patient.Create(PatientId.Create(id), name, Sex.Male, 30, AgeUnit.Year, registrationUtc);

    private static void AddCatalogTest(FakeApplicationDbContext db, int id, string name)
        => db.Tests.Add(Test.Create(TestId.Create(id), name, name, name, $"T{id}", 30, 100m, ResultKind.Simple, false, null));

    private static PatientTest MakeRow(int ptId, int patientId, int testId)
        => PatientTest.Create(PatientTestId.Create(ptId), PatientId.Create(patientId), TestId.Create(testId), 100m);

    /// <summary>
    /// One patient registered today with three rows:
    /// row 1 unprinted and unreviewed, row 2 reviewed AND printed, row 3 reviewed but unprinted.
    /// Any filter that returns the wrong set is visible in the returned PatientTestIds.
    ///
    /// The Domain lifecycle requires a result to be entered and reviewed before it can be
    /// printed (<c>MarkPrinted</c> throws "Result not reviewed." otherwise), so row 2 goes
    /// through EnterResult -> MarkReviewed -> MarkPrinted.
    /// </summary>
    private static FakeApplicationDbContext MixedPrintState()
    {
        var db = new FakeApplicationDbContext();
        var now = DateTime.UtcNow;
        db.Patients.Add(MakePatient(1, "Mixed", now));
        AddCatalogTest(db, 10, "CBC");
        AddCatalogTest(db, 11, "Glucose");

        // Row 101: neither entered nor reviewed nor printed.
        var unprinted = MakeRow(101, 1, 10);

        // Row 102: the only PRINTED row.
        var printed = MakeRow(102, 1, 11);
        printed.EnterResult("5.5", null, 5, now);
        printed.MarkReviewed(5, now);
        printed.MarkPrinted(5, now);

        // Row 103: reviewed but NOT printed — the row that proves reviewed != printed.
        var reviewedUnprinted = MakeRow(103, 1, 10);
        reviewedUnprinted.EnterResult("7.1", null, 5, now);
        reviewedUnprinted.MarkReviewed(5, now);

        db.PatientTests.Add(unprinted);
        db.PatientTests.Add(printed);
        db.PatientTests.Add(reviewedUnprinted);
        return db;
    }

    private static int[] RowIds(
        TopLab.Application.Common.Results.Result<IReadOnlyList<ResultWorklistItemDto>> result)
        => result.Value!.Select(i => i.PatientTestId).OrderBy(id => id).ToArray();

    // =====================================================================
    // F6 — IsPrinted. The negative case is the important one.
    // =====================================================================

    [Fact]
    public async Task ResultWorklist_FilterIsPrintedFalse_ReturnsOnlyUnprinted()
    {
        var db = MixedPrintState();
        var handler = new GetResultWorklistQueryHandler(db);

        var result = await handler.Handle(
            new GetResultWorklistQuery(IsPrinted: false), CancellationToken.None);

        Assert.True(result.IsSuccess);

        // Rows 101 and 103 are unprinted; row 102 is printed and MUST be excluded.
        // If false were treated as "no filter", row 102 would appear and this fails.
        Assert.Equal(new[] { 101, 103 }, RowIds(result));
        Assert.DoesNotContain(102, RowIds(result));

        // And the DTO flag agrees with the filter.
        Assert.All(result.Value!, item => Assert.False(item.IsPrinted));
    }

    [Fact]
    public async Task ResultWorklist_FilterIsPrintedTrue_ReturnsOnlyPrinted()
    {
        var db = MixedPrintState();
        var handler = new GetResultWorklistQueryHandler(db);

        var result = await handler.Handle(
            new GetResultWorklistQuery(IsPrinted: true), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { 102 }, RowIds(result));
        Assert.All(result.Value!, item => Assert.True(item.IsPrinted));
    }

    [Fact]
    public async Task ResultWorklist_IsPrintedNull_AppliesNoFilter()
    {
        var db = MixedPrintState();
        var handler = new GetResultWorklistQueryHandler(db);

        var result = await handler.Handle(
            new GetResultWorklistQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);

        // All three rows, printed and unprinted together.
        Assert.Equal(new[] { 101, 102, 103 }, RowIds(result));

        // Explicit null must behave exactly like the parameter being absent.
        var explicitNull = await handler.Handle(
            new GetResultWorklistQuery(IsPrinted: null), CancellationToken.None);
        Assert.Equal(RowIds(result), RowIds(explicitNull));
    }

    [Fact]
    public async Task ResultWorklist_FilterIsPrintedFalse_IsNotTheSameAsNoFilter()
    {
        var db = MixedPrintState();
        var handler = new GetResultWorklistQueryHandler(db);

        var unprintedOnly = await handler.Handle(
            new GetResultWorklistQuery(IsPrinted: false), CancellationToken.None);
        var noFilter = await handler.Handle(
            new GetResultWorklistQuery(IsPrinted: null), CancellationToken.None);

        Assert.True(unprintedOnly.IsSuccess);
        Assert.True(noFilter.IsSuccess);

        // The central F6 assertion: the negative filter NARROWS the set.
        Assert.True(RowIds(unprintedOnly).Length < RowIds(noFilter).Length);
        Assert.NotEqual(RowIds(noFilter), RowIds(unprintedOnly));
    }

    [Fact]
    public async Task ResultWorklist_FilterIsPrintedFalse_ExcludesEveryPrintedRow()
    {
        var db = new FakeApplicationDbContext();
        var now = DateTime.UtcNow;
        db.Patients.Add(MakePatient(1, "All", now));
        AddCatalogTest(db, 10, "CBC");

        for (var i = 1; i <= 6; i++)
        {
            var row = MakeRow(200 + i, 1, 10);
            if (i % 2 == 0)
            {
                row.EnterResult("5.5", null, 5, now);
                row.MarkReviewed(5, now);
                row.MarkPrinted(5, now);
            }

            db.PatientTests.Add(row);
        }

        var handler = new GetResultWorklistQueryHandler(db);

        var unprinted = await handler.Handle(
            new GetResultWorklistQuery(IsPrinted: false), CancellationToken.None);

        Assert.True(unprinted.IsSuccess);

        // Exactly the 3 odd rows. A "no filter" implementation would return all 6.
        Assert.Equal(3, unprinted.Value!.Count);
        Assert.All(unprinted.Value!, item => Assert.False(item.IsPrinted));

        var printed = await handler.Handle(
            new GetResultWorklistQuery(IsPrinted: true), CancellationToken.None);
        Assert.Equal(3, printed.Value!.Count);
        Assert.All(printed.Value!, item => Assert.True(item.IsPrinted));
    }

    [Fact]
    public void ResultWorklist_IsPrinted_IsATriStateParameter()
    {
        // A non-nullable bool could not express "no filter" — this pins the type that makes
        // false distinguishable from absent (C-4).
        var property = typeof(GetResultWorklistQuery).GetProperty("IsPrinted")!;
        Assert.Equal(typeof(bool?), property.PropertyType);

        var parameter = typeof(GetResultWorklistQuery).GetConstructors().Single()
            .GetParameters().Single(p => p.Name == "IsPrinted");
        Assert.True(parameter.HasDefaultValue);
        Assert.Null(parameter.DefaultValue);
    }

    // =====================================================================
    // F7 — IsReviewed, through the EXISTING parameter (C-3: no backend rebuild)
    // =====================================================================

    [Fact]
    public async Task ResultWorklist_IsReviewedFilter_Narrows()
    {
        var db = new FakeApplicationDbContext();
        var now = DateTime.UtcNow;
        db.Patients.Add(MakePatient(1, "Rev", now));
        AddCatalogTest(db, 10, "CBC");

        var unreviewed = MakeRow(101, 1, 10);
        var reviewed = MakeRow(102, 1, 10);
        reviewed.EnterResult("6.2", null, 5, now);
        reviewed.MarkReviewed(5, now);
        db.PatientTests.Add(unreviewed);
        db.PatientTests.Add(reviewed);

        var handler = new GetResultWorklistQueryHandler(db);

        var onlyUnreviewed = await handler.Handle(
            new GetResultWorklistQuery(IsReviewed: false), CancellationToken.None);
        Assert.True(onlyUnreviewed.IsSuccess);
        Assert.Equal(new[] { 101 }, RowIds(onlyUnreviewed));

        var onlyReviewed = await handler.Handle(
            new GetResultWorklistQuery(IsReviewed: true), CancellationToken.None);
        Assert.Equal(new[] { 102 }, RowIds(onlyReviewed));

        var noFilter = await handler.Handle(
            new GetResultWorklistQuery(IsReviewed: null), CancellationToken.None);
        Assert.Equal(new[] { 101, 102 }, RowIds(noFilter));
    }

    [Fact]
    public async Task ResultWorklist_PrintAndReviewFiltersCombineWithAnd()
    {
        var db = MixedPrintState();
        var handler = new GetResultWorklistQueryHandler(db);

        // Row 103 is reviewed AND unprinted — the only row satisfying both.
        var result = await handler.Handle(
            new GetResultWorklistQuery(IsReviewed: true, IsPrinted: false), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { 103 }, RowIds(result));
    }

    [Fact]
    public void ResultWorklist_HasResultFilter_IsUnaffected()
    {
        // The three checkboxes must stay distinguishable: HasResult, IsReviewed, IsPrinted
        // are three separate parameters on the record.
        var names = typeof(GetResultWorklistQuery)
            .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Select(p => p.Name)
            .ToArray();

        Assert.Contains("HasResult", names);
        Assert.Contains("IsReviewed", names);
        Assert.Contains("IsPrinted", names);
    }
}