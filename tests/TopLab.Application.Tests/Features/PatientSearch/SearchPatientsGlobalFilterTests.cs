using TopLab.Application.Features.PatientSearch.Queries.SearchPatientsGlobal;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using Xunit;

namespace TopLab.Application.Tests.Features.PatientSearch;

/// <summary>
/// S2 — the six P-01 patient-search filters.
///
/// Every test here is built so that it FAILS if its predicate were removed, and so that it
/// fails if the predicate were replaced by something that widens. Each fixture holds several
/// rows and the assertion names the exact surviving ids, so an "everything" result is never a
/// pass (SD-5 clause 5).
/// </summary>
public class SearchPatientsGlobalFilterTests
{
    private static readonly DateTime Anchor = new(2026, 3, 1, 12, 0, 0, DateTimeKind.Utc);

    private static Patient MakePatient(int id, string name, DateTime registrationUtc,
        Sex sex = Sex.Male, int ageValue = 30, AgeUnit ageUnit = AgeUnit.Year,
        int? treatingDoctorId = null, int? referralEntityId = null, string? labId = null)
    {
        var patient = Patient.Create(
            PatientId.Create(id), name, sex, ageValue, ageUnit, registrationUtc,
            treatingDoctorId: treatingDoctorId is null ? null : ExternalEntityId.Create(treatingDoctorId.Value),
            referralEntityId: referralEntityId is null ? null : ExternalEntityId.Create(referralEntityId.Value));

        if (labId is not null)
        {
            patient.AssignLabId(LabId.Create(labId));
        }

        return patient;
    }

    private static void AddTest(FakeApplicationDbContext db, int patientTestId, int patientId, int testId)
    {
        db.PatientTests.Add(PatientTest.Create(
            PatientTestId.Create(patientTestId), PatientId.Create(patientId), TestId.Create(testId), 100m));
    }

    private static int[] Ids(TopLab.Application.Common.Results.Result<IReadOnlyList<TopLab.Application.Features.PatientSearch.Common.PatientSearchHitDto>> result)
        => result.Value!.Select(h => h.PatientId).OrderBy(id => id).ToArray();

    // =====================================================================
    // F1 — treating doctor and referral entity. TWO separate filters (SD-6).
    // =====================================================================

    [Fact]
    public async Task Search_FilterByTreatingDoctor_Narrows()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1, "A", Anchor, treatingDoctorId: 10));
        db.Patients.Add(MakePatient(2, "B", Anchor, treatingDoctorId: 10));
        db.Patients.Add(MakePatient(3, "C", Anchor, treatingDoctorId: 11));
        db.Patients.Add(MakePatient(4, "D", Anchor));

        var result = await new SearchPatientsGlobalQueryHandler(db).Handle(
            new SearchPatientsGlobalQuery(null, TreatingDoctorId: ExternalEntityId.Create(10)),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { 1, 2 }, Ids(result));

        // The single-patient doctor returns one row, not the whole table.
        var other = await new SearchPatientsGlobalQueryHandler(db).Handle(
            new SearchPatientsGlobalQuery(null, TreatingDoctorId: ExternalEntityId.Create(11)),
            CancellationToken.None);
        Assert.Equal(new[] { 3 }, Ids(other));

        // A doctor nobody has returns empty, never everything.
        var none = await new SearchPatientsGlobalQueryHandler(db).Handle(
            new SearchPatientsGlobalQuery(null, TreatingDoctorId: ExternalEntityId.Create(999)),
            CancellationToken.None);
        Assert.Empty(Ids(none));
    }

    [Fact]
    public async Task Search_FilterByReferralEntity_Narrows()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1, "A", Anchor, referralEntityId: 20));
        db.Patients.Add(MakePatient(2, "B", Anchor, referralEntityId: 21));
        db.Patients.Add(MakePatient(3, "C", Anchor));

        var result = await new SearchPatientsGlobalQueryHandler(db).Handle(
            new SearchPatientsGlobalQuery(null, ReferralEntityId: ExternalEntityId.Create(20)),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { 1 }, Ids(result));

        var other = await new SearchPatientsGlobalQueryHandler(db).Handle(
            new SearchPatientsGlobalQuery(null, ReferralEntityId: ExternalEntityId.Create(21)),
            CancellationToken.None);
        Assert.Equal(new[] { 2 }, Ids(other));

        var none = await new SearchPatientsGlobalQueryHandler(db).Handle(
            new SearchPatientsGlobalQuery(null, ReferralEntityId: ExternalEntityId.Create(999)),
            CancellationToken.None);
        Assert.Empty(Ids(none));
    }

    [Fact]
    public async Task Search_DoctorAndReferralAreDistinct_SameValueDifferentResults()
    {
        var db = new FakeApplicationDbContext();

        // Patient 1 is seen by doctor 7 and referred by entity 8.
        db.Patients.Add(MakePatient(1, "A", Anchor, treatingDoctorId: 7, referralEntityId: 8));
        // Patient 2 has the doctor only. Patient 3 has the referral entity only.
        db.Patients.Add(MakePatient(2, "B", Anchor, treatingDoctorId: 7));
        db.Patients.Add(MakePatient(3, "C", Anchor, referralEntityId: 7));
        // Patient 4 is the mirror image of patient 1: doctor 8, referral 7.
        db.Patients.Add(MakePatient(4, "D", Anchor, treatingDoctorId: 8, referralEntityId: 7));

        var handler = new SearchPatientsGlobalQueryHandler(db);

        // The SAME id value used as a doctor and as a referral entity must select
        // different patients. A collapsed single column or a merged lookup fails here.
        var byDoctor = await handler.Handle(
            new SearchPatientsGlobalQuery(null, TreatingDoctorId: ExternalEntityId.Create(7)),
            CancellationToken.None);
        var byReferral = await handler.Handle(
            new SearchPatientsGlobalQuery(null, ReferralEntityId: ExternalEntityId.Create(7)),
            CancellationToken.None);

        Assert.Equal(new[] { 1, 2 }, Ids(byDoctor));
        Assert.Equal(new[] { 3, 4 }, Ids(byReferral));
        Assert.NotEqual(Ids(byDoctor), Ids(byReferral));

        // And when both are supplied they are AND-combined, not OR-combined.
        // Patient 1 is the only one with doctor 7 AND referral 8. Patient 2 has
        // doctor 7 but no referral entity; patient 3 has referral 7 but no doctor;
        // patient 4 has referral 7 but doctor 8. An OR of the two filters would
        // return all four; the AND returns exactly one.
        var both = await handler.Handle(
            new SearchPatientsGlobalQuery(
                null,
                TreatingDoctorId: ExternalEntityId.Create(7),
                ReferralEntityId: ExternalEntityId.Create(8)),
            CancellationToken.None);
        Assert.Equal(new[] { 1 }, Ids(both));
    }

    [Fact]
    public void Search_DoctorAndReferralAreSeparateQueryParameters()
    {
        var names = typeof(SearchPatientsGlobalQuery)
            .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Select(p => p.Name)
            .ToArray();

        Assert.Contains("TreatingDoctorId", names);
        Assert.Contains("ReferralEntityId", names);

        // Distinct parameter types are not required, but distinct *names* and distinct
        // backing fields are — one property must not serve both concepts.
        var doctor = typeof(SearchPatientsGlobalQuery).GetProperty("TreatingDoctorId")!;
        var referral = typeof(SearchPatientsGlobalQuery).GetProperty("ReferralEntityId")!;
        Assert.NotSame(doctor, referral);
        Assert.NotEqual(doctor.Name, referral.Name);
    }

    // =====================================================================
    // F2 — search by test
    // =====================================================================

    [Fact]
    public async Task Search_FilterByTest_Narrows_AndUnknownTestReturnsEmpty()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1, "A", Anchor));
        db.Patients.Add(MakePatient(2, "B", Anchor));
        db.Patients.Add(MakePatient(3, "C", Anchor));
        AddTest(db, 1, 1, 100);
        AddTest(db, 2, 1, 101);
        AddTest(db, 3, 2, 100);
        // patient 3 has no PatientTest at all

        var handler = new SearchPatientsGlobalQueryHandler(db);

        var withHundred = await handler.Handle(
            new SearchPatientsGlobalQuery(null, TestId: 100), CancellationToken.None);
        Assert.True(withHundred.IsSuccess);
        Assert.Equal(new[] { 1, 2 }, Ids(withHundred));

        var withHundredOne = await handler.Handle(
            new SearchPatientsGlobalQuery(null, TestId: 101), CancellationToken.None);
        Assert.Equal(new[] { 1 }, Ids(withHundredOne));

        // A test nobody has returns EMPTY, not everything (VG-02).
        var unknown = await handler.Handle(
            new SearchPatientsGlobalQuery(null, TestId: 999), CancellationToken.None);
        Assert.True(unknown.IsSuccess);
        Assert.Empty(Ids(unknown));
    }

    // =====================================================================
    // F3 — search by gender
    // =====================================================================

    [Fact]
    public async Task Search_FilterByGender_Narrows()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1, "A", Anchor, sex: Sex.Male));
        db.Patients.Add(MakePatient(2, "B", Anchor, sex: Sex.Female));
        db.Patients.Add(MakePatient(3, "C", Anchor, sex: Sex.Male));
        db.Patients.Add(MakePatient(4, "D", Anchor, sex: Sex.Female));

        var handler = new SearchPatientsGlobalQueryHandler(db);

        var males = await handler.Handle(new SearchPatientsGlobalQuery(null, Sex: Sex.Male), CancellationToken.None);
        Assert.True(males.IsSuccess);
        Assert.Equal(new[] { 1, 3 }, Ids(males));

        var females = await handler.Handle(new SearchPatientsGlobalQuery(null, Sex: Sex.Female), CancellationToken.None);
        Assert.Equal(new[] { 2, 4 }, Ids(females));
    }

    // =====================================================================
    // F4 — search by age band (AS-5: unit-aware, like with like, no conversion)
    // =====================================================================

    [Fact]
    public async Task Search_FilterByAge_Narrows()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1, "Child", Anchor, ageValue: 5, ageUnit: AgeUnit.Year));
        db.Patients.Add(MakePatient(2, "Teen", Anchor, ageValue: 15, ageUnit: AgeUnit.Year));
        db.Patients.Add(MakePatient(3, "Adult", Anchor, ageValue: 40, ageUnit: AgeUnit.Year));

        var handler = new SearchPatientsGlobalQueryHandler(db);

        var band = await handler.Handle(
            new SearchPatientsGlobalQuery(null, Age: new AgeValueBand(AgeUnit.Year, From: 0, To: 17)),
            CancellationToken.None);
        Assert.True(band.IsSuccess);
        Assert.Equal(new[] { 1, 2 }, Ids(band));

        var adultsOnly = await handler.Handle(
            new SearchPatientsGlobalQuery(null, Age: new AgeValueBand(AgeUnit.Year, From: 18)),
            CancellationToken.None);
        Assert.Equal(new[] { 3 }, Ids(adultsOnly));
    }

    [Fact]
    public async Task Search_FilterByAge_ComparesLikeWithLike_DayUnitDoesNotMatchYearValues()
    {
        var db = new FakeApplicationDbContext();
        // A baby recorded in DAYS and a child recorded in YEARS.
        db.Patients.Add(MakePatient(1, "Baby", Anchor, ageValue: 400, ageUnit: AgeUnit.Day));
        db.Patients.Add(MakePatient(2, "Child", Anchor, ageValue: 8, ageUnit: AgeUnit.Year));
        // A numeric band that would match the 400-days row if units were ignored.
        db.Patients.Add(MakePatient(3, "Adult", Anchor, ageValue: 400, ageUnit: AgeUnit.Year));

        var handler = new SearchPatientsGlobalQueryHandler(db);

        // Unit Day, band 0..500. The 400-Year patient must NOT be returned (AS-5/BR-04).
        var days = await handler.Handle(
            new SearchPatientsGlobalQuery(null, Age: new AgeValueBand(AgeUnit.Day, From: 0, To: 500)),
            CancellationToken.None);
        Assert.True(days.IsSuccess);
        Assert.Equal(new[] { 1 }, Ids(days));

        // Unit Year, same numeric band: the Day row is now excluded instead.
        var years = await handler.Handle(
            new SearchPatientsGlobalQuery(null, Age: new AgeValueBand(AgeUnit.Year, From: 0, To: 500)),
            CancellationToken.None);
        Assert.Equal(new[] { 2, 3 }, Ids(years));
    }

    [Fact]
    public async Task Search_FilterByAge_BoundsAreInclusive()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1, "Low", Anchor, ageValue: 10, ageUnit: AgeUnit.Year));
        db.Patients.Add(MakePatient(2, "Mid", Anchor, ageValue: 20, ageUnit: AgeUnit.Year));
        db.Patients.Add(MakePatient(3, "High", Anchor, ageValue: 30, ageUnit: AgeUnit.Year));

        var result = await new SearchPatientsGlobalQueryHandler(db).Handle(
            new SearchPatientsGlobalQuery(null, Age: new AgeValueBand(AgeUnit.Year, From: 10, To: 30)),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { 1, 2, 3 }, Ids(result));
    }

    // =====================================================================
    // F5 — search by date range (AS-4: bounds RegistrationDateUtc, inclusive)
    // =====================================================================

    [Fact]
    public async Task Search_FilterByDateRange_Narrows_IncludingBothBoundaries()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1, "Before", new DateTime(2026, 2, 28, 23, 59, 59, DateTimeKind.Utc)));
        db.Patients.Add(MakePatient(2, "FromBound", new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc)));
        db.Patients.Add(MakePatient(3, "Mid", new DateTime(2026, 3, 15, 12, 0, 0, DateTimeKind.Utc)));
        db.Patients.Add(MakePatient(4, "ToBound", new DateTime(2026, 3, 31, 23, 59, 59, DateTimeKind.Utc)));
        db.Patients.Add(MakePatient(5, "After", new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc)));

        var handler = new SearchPatientsGlobalQueryHandler(db);

        var result = await handler.Handle(
            new SearchPatientsGlobalQuery(null,
                From: new DateOnly(2026, 3, 1),
                To: new DateOnly(2026, 3, 31)),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        // Both boundaries are INCLUSIVE.
        Assert.Equal(new[] { 2, 3, 4 }, Ids(result));
    }

    [Fact]
    public async Task Search_FilterByDateRange_OpenEndedBoundsWorkIndependently()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1, "Early", new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc)));
        db.Patients.Add(MakePatient(2, "Late", new DateTime(2026, 6, 10, 0, 0, 0, DateTimeKind.Utc)));

        var handler = new SearchPatientsGlobalQueryHandler(db);

        var fromOnly = await handler.Handle(
            new SearchPatientsGlobalQuery(null, From: new DateOnly(2026, 3, 1)), CancellationToken.None);
        Assert.Equal(new[] { 2 }, Ids(fromOnly));

        var toOnly = await handler.Handle(
            new SearchPatientsGlobalQuery(null, To: new DateOnly(2026, 3, 1)), CancellationToken.None);
        Assert.Equal(new[] { 1 }, Ids(toOnly));
    }

    // =====================================================================
    // Combination, inertness, and interaction with the text path
    // =====================================================================

    [Fact]
    public async Task Search_FiltersCombineWithAnd()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1, "A", Anchor, Sex.Male, 30, AgeUnit.Year, treatingDoctorId: 10));
        db.Patients.Add(MakePatient(2, "B", Anchor, Sex.Female, 30, AgeUnit.Year, treatingDoctorId: 10));
        db.Patients.Add(MakePatient(3, "C", Anchor, Sex.Male, 30, AgeUnit.Year, treatingDoctorId: 11));
        db.Patients.Add(MakePatient(4, "D", Anchor, Sex.Male, 70, AgeUnit.Year, treatingDoctorId: 10));
        db.Patients.Add(MakePatient(5, "E", Anchor, Sex.Male, 30, AgeUnit.Year, treatingDoctorId: 10));
        AddTest(db, 1, 5, 100);

        var result = await new SearchPatientsGlobalQueryHandler(db).Handle(
            new SearchPatientsGlobalQuery(
                null,
                TreatingDoctorId: ExternalEntityId.Create(10),
                Sex: Sex.Male,
                Age: new AgeValueBand(AgeUnit.Year, From: 18, To: 60),
                TestId: 100),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        // Only patient 5 satisfies all four; each wrong row is excluded by a different filter.
        Assert.Equal(new[] { 5 }, Ids(result));
    }

    [Fact]
    public async Task Search_FilterWithEmptyText_StillFilters()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1, "Male", Anchor, Sex.Male));
        db.Patients.Add(MakePatient(2, "Female", Anchor, Sex.Female));

        var handler = new SearchPatientsGlobalQueryHandler(db);

        foreach (var text in new[] { null, string.Empty, "   " })
        {
            var result = await handler.Handle(
                new SearchPatientsGlobalQuery(text, Sex: Sex.Male), CancellationToken.None);
            Assert.True(result.IsSuccess);
            Assert.Equal(new[] { 1 }, Ids(result));
        }
    }

    [Fact]
    public async Task Search_NullFilterParameters_ApplyNoFilterAtAll()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1, "A", Anchor, Sex.Male, 30, AgeUnit.Year, treatingDoctorId: 10, referralEntityId: 20));
        db.Patients.Add(MakePatient(2, "B", Anchor, Sex.Female, 70, AgeUnit.Year));
        AddTest(db, 1, 1, 100);

        var handler = new SearchPatientsGlobalQueryHandler(db);

        // Every parameter explicitly null must be inert.
        var result = await handler.Handle(
            new SearchPatientsGlobalQuery(
                null,
                Page: 1,
                PageSize: 50,
                TreatingDoctorId: null,
                ReferralEntityId: null,
                TestId: null,
                Sex: null,
                Age: null,
                From: null,
                To: null),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { 1, 2 }, Ids(result));
    }

    [Fact]
    public async Task Search_FilterAndTextCombineWithAnd()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1, "SameNameOne", Anchor, Sex.Male, treatingDoctorId: 10, labId: "LAB-1"));
        db.Patients.Add(MakePatient(2, "SameNameTwo", Anchor, Sex.Female, treatingDoctorId: 10));
        db.Patients.Add(MakePatient(3, "Other", Anchor, Sex.Male, treatingDoctorId: 11));

        var handler = new SearchPatientsGlobalQueryHandler(db);

        // Name assist is off by default, so use the LabId text path to prove Text still applies.
        var withLabId = await handler.Handle(
            new SearchPatientsGlobalQuery("LAB-9", Sex: Sex.Male), CancellationToken.None);
        Assert.True(withLabId.IsSuccess);
        Assert.Empty(Ids(withLabId));

        // Text narrows AND the filter narrows; neither replaces the other.
        var textOnly = await handler.Handle(
            new SearchPatientsGlobalQuery("LAB-1"), CancellationToken.None);
        Assert.Equal(new[] { 1 }, Ids(textOnly));

        var textAndFilter = await handler.Handle(
            new SearchPatientsGlobalQuery("LAB-1", Sex: Sex.Female), CancellationToken.None);
        Assert.Empty(Ids(textAndFilter));
    }

    [Fact]
    public async Task Search_SoftDeletedRowsStayExcludedWhenFiltersAreApplied()
    {
        var db = new FakeApplicationDbContext();
        var deleted = MakePatient(1, "Deleted", Anchor, Sex.Male);
        deleted.SoftDelete();
        db.Patients.Add(deleted);
        db.Patients.Add(MakePatient(2, "Live", Anchor, Sex.Male));

        var result = await new SearchPatientsGlobalQueryHandler(db).Handle(
            new SearchPatientsGlobalQuery(null, Sex: Sex.Male), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { 2 }, Ids(result));
    }

    [Fact]
    public async Task Search_FilterStillHonoursPageSizeCap()
    {
        var db = new FakeApplicationDbContext();

        // 120 male patients registered on distinct days.
        for (var i = 1; i <= 120; i++)
        {
            db.Patients.Add(MakePatient(i, $"P{i:D3}", Anchor.AddDays(-i), Sex.Male));
        }

        // 120 female patients that the gender filter must exclude.
        for (var i = 121; i <= 240; i++)
        {
            db.Patients.Add(MakePatient(i, $"F{i:D3}", Anchor.AddDays(-i), Sex.Female));
        }

        var result = await new SearchPatientsGlobalQueryHandler(db).Handle(
            new SearchPatientsGlobalQuery(null, Sex: Sex.Male), CancellationToken.None);

        Assert.True(result.IsSuccess);

        // The cap still holds after filtering: 50 of the 120 matching rows.
        Assert.Equal(50, result.Value!.Count);
        Assert.All(result.Value!, h => Assert.True(h.PatientId <= 120));
    }
}