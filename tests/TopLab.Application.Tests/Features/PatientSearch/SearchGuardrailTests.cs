using TopLab.Application.Features.PatientSearch.Queries.SearchPatientsGlobal;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Settings;
using Xunit;

namespace TopLab.Application.Tests.Features.PatientSearch;

/// <summary>
/// S1 — characterisation (guardrail) tests for <c>SearchPatientsGlobalQuery</c>.
///
/// These pin the safety properties S2 depends on BEFORE any filter is added:
/// the soft-delete exclusion, the ordering, the paging chain and the display cap.
/// A regression in any of them must be distinguishable from one introduced by S2,
/// which is why this file touches no production code (P-01 §3, AS-7).
///
/// Read-only intent: every test here passes against the pinned commit. When S2 adds
/// the six filter parameters these tests must stay green unchanged.
/// </summary>
public class SearchGuardrailTests
{
    private static Patient MakePatient(int id, string name, DateTime registrationUtc,
        string? labId = null, string? nationalId = null, Sex sex = Sex.Male,
        int ageValue = 30, AgeUnit ageUnit = AgeUnit.Year)
    {
        var patient = Patient.Create(
            PatientId.Create(id), name, sex, ageValue, ageUnit, registrationUtc,
            nationalId: nationalId);

        if (labId is not null)
        {
            patient.AssignLabId(LabId.Create(labId));
        }

        return patient;
    }

    private static void AddPhone(FakeApplicationDbContext db, int phoneId, int patientId, string number, byte sortOrder)
    {
        db.PatientPhoneNumbers.Add(PatientPhoneNumber.Create(
            PatientPhoneNumberId.Create(phoneId), PatientId.Create(patientId), number, sortOrder));
    }

    private static SystemSettings WithNameAssist(bool enabled)
    {
        var settings = SystemSettings.CreateDefault();
        settings.SetGeneralFlags(
            saveTreatingDoctorOnlyFromEntityWindow: false,
            enablePatientNameSearchAssist: enabled,
            disableAutoTitleInsertion: false,
            printFileExternalBarcode: false,
            printDateTimeOnTubeBarcode: false,
            printLabIdInsteadOfPatientId: false,
            autoReviewAndComplete: false,
            printAccountInsteadOfDateOnReport: false);
        return settings;
    }

    // ---------------------------------------------------------------------
    // Guardrail 1 — the query shape itself (SD-7, C-1, VG-01)
    // ---------------------------------------------------------------------

    [Fact]
    public void SearchGuardrail_Query_HasNoFilterParameter_BeforeSlice2()
    {
        var members = typeof(SearchPatientsGlobalQuery)
            .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Select(p => p.Name)
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(new[] { "Page", "PageSize", "Text" }, members);
    }

    [Fact]
    public void SearchGuardrail_Query_PageSizeDefaultIs50_AndNoHundredLiteralExists()
    {
        var ctor = typeof(SearchPatientsGlobalQuery).GetConstructors().Single();
        var parameters = ctor.GetParameters();

        // The positional defaults are the guardrail. PageSize == 50, Page == 1.
        var pageSizeParam = parameters.Single(p => p.Name == "PageSize");
        var pageParam = parameters.Single(p => p.Name == "Page");

        Assert.True(pageSizeParam.HasDefaultValue);
        Assert.Equal(50, pageSizeParam.DefaultValue);
        Assert.True(pageParam.HasDefaultValue);
        Assert.Equal(1, pageParam.DefaultValue);
        Assert.False(parameters.Single(p => p.Name == "Text").HasDefaultValue);

        // SD-7 / C-1: no member default is 100, and no member named for a cap exists.
        Assert.DoesNotContain(parameters, p => p.DefaultValue is int value && value == 100);

        var propertyNames = typeof(SearchPatientsGlobalQuery)
            .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Select(p => p.Name)
            .ToArray();
        Assert.DoesNotContain(propertyNames, n => n.Contains("Cap", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void SearchGuardrail_Query_PositionalShapeIsTextPagePageSize_InThatOrder()
    {
        var parameters = typeof(SearchPatientsGlobalQuery)
            .GetConstructors().Single()
            .GetParameters()
            .Select(p => p.Name)
            .ToArray();

        Assert.Equal(new[] { "Text", "Page", "PageSize" }, parameters);
    }

    // ---------------------------------------------------------------------
    // Guardrail 2 — the display cap is enforced (SD-5 clause 4, SD-7)
    // ---------------------------------------------------------------------

    [Fact]
    public async Task SearchGuardrail_Cap_IsEnforced_DefaultPageSizeReturnsAtMost50Rows()
    {
        var db = new FakeApplicationDbContext();
        for (var i = 1; i <= 120; i++)
        {
            db.Patients.Add(MakePatient(i, $"P{i:D3}", new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMinutes(-i)));
        }

        var handler = new SearchPatientsGlobalQueryHandler(db);
        var result = await handler.Handle(new SearchPatientsGlobalQuery(null), CancellationToken.None);

        Assert.True(result.IsSuccess);

        // The cap: no more than the default PageSize of 50 rows come back,
        // even though 120 live patients exist. No 100 literal is involved.
        Assert.Equal(50, result.Value!.Count);
        Assert.True(result.Value.Count < 120, "the cap must actually bound the result set");
    }

    [Fact]
    public async Task SearchGuardrail_Cap_IsEnforced_WhenPageSizeIsSuppliedItIsHonouredExactly()
    {
        var db = new FakeApplicationDbContext();
        for (var i = 1; i <= 40; i++)
        {
            db.Patients.Add(MakePatient(i, $"P{i:D3}", new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMinutes(-i)));
        }

        var handler = new SearchPatientsGlobalQueryHandler(db);

        var five = await handler.Handle(new SearchPatientsGlobalQuery(null, 1, 5), CancellationToken.None);
        Assert.True(five.IsSuccess);
        Assert.Equal(5, five.Value!.Count);

        var seven = await handler.Handle(new SearchPatientsGlobalQuery(null, 1, 7), CancellationToken.None);
        Assert.True(seven.IsSuccess);
        Assert.Equal(7, seven.Value!.Count);

        // PageSize is the cap; it is never silently replaced by a larger value.
        Assert.True(seven.Value!.Count < 40);
    }

    // ---------------------------------------------------------------------
    // Guardrail 3 — paging narrows, never materialises the table
    // ---------------------------------------------------------------------

    [Fact]
    public async Task SearchGuardrail_Paging_SkipTakeNarrows_AndNeverReturnsTheWholeTable()
    {
        var db = new FakeApplicationDbContext();
        for (var i = 1; i <= 25; i++)
        {
            db.Patients.Add(MakePatient(i, $"P{i:D2}", new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMinutes(-i)));
        }

        var handler = new SearchPatientsGlobalQueryHandler(db);

        var page1 = await handler.Handle(new SearchPatientsGlobalQuery(null, 1, 10), CancellationToken.None);
        Assert.True(page1.IsSuccess);
        Assert.Equal(10, page1.Value!.Count);

        var page3 = await handler.Handle(new SearchPatientsGlobalQuery(null, 3, 10), CancellationToken.None);
        Assert.True(page3.IsSuccess);
        Assert.Equal(5, page3.Value!.Count);

        // Nothing outside the requested page comes back — Skip/Take really applies.
        var page3Ids = page3.Value!.Select(h => h.PatientId).ToArray();
        var page1Ids = page1.Value!.Select(h => h.PatientId).ToArray();
        Assert.Empty(page3Ids.Intersect(page1Ids));
    }

    [Fact]
    public async Task SearchGuardrail_Paging_PageBeyondEnd_ReturnsEmptyRatherThanWrapping()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1, "P01", new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)));
        db.Patients.Add(MakePatient(2, "P02", new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMinutes(-1)));

        var handler = new SearchPatientsGlobalQueryHandler(db);
        var result = await handler.Handle(new SearchPatientsGlobalQuery(null, 99, 10), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }

    [Fact]
    public async Task SearchGuardrail_Paging_FirstPageUsesNoOffset()
    {
        var db = new FakeApplicationDbContext();
        for (var i = 1; i <= 12; i++)
        {
            db.Patients.Add(MakePatient(i, $"P{i:D2}", new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMinutes(-i)));
        }

        var handler = new SearchPatientsGlobalQueryHandler(db);

        var defaultPage = await handler.Handle(new SearchPatientsGlobalQuery(null), CancellationToken.None);
        var explicitFirstPage = await handler.Handle(new SearchPatientsGlobalQuery(null, 1, 50), CancellationToken.None);

        Assert.True(defaultPage.IsSuccess);
        Assert.True(explicitFirstPage.IsSuccess);
        Assert.Equal(
            defaultPage.Value!.Select(h => h.PatientId).ToArray(),
            explicitFirstPage.Value!.Select(h => h.PatientId).ToArray());
    }

    // ---------------------------------------------------------------------
    // Guardrail 4 — ordering is RegistrationDateUtc descending and stable
    // ---------------------------------------------------------------------

    [Fact]
    public async Task SearchGuardrail_Ordering_IsRegistrationDateUtcDescending()
    {
        var db = new FakeApplicationDbContext();
        var anchor = new DateTime(2026, 3, 1, 12, 0, 0, DateTimeKind.Utc);
        db.Patients.Add(MakePatient(1, "Oldest", anchor.AddDays(-10)));
        db.Patients.Add(MakePatient(2, "Newest", anchor));
        db.Patients.Add(MakePatient(3, "Middle", anchor.AddDays(-5)));

        var handler = new SearchPatientsGlobalQueryHandler(db);
        var result = await handler.Handle(new SearchPatientsGlobalQuery(null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { 2, 3, 1 }, result.Value!.Select(h => h.PatientId).ToArray());
    }

    [Fact]
    public async Task SearchGuardrail_Ordering_IsStableAcrossPages_NoRowIsLostOrDuplicated()
    {
        var db = new FakeApplicationDbContext();
        var anchor = new DateTime(2026, 3, 1, 12, 0, 0, DateTimeKind.Utc);
        for (var i = 1; i <= 20; i++)
        {
            // Newest registration last, so the descending order is ids 20..1.
            db.Patients.Add(MakePatient(i, $"P{i:D2}", anchor.AddMinutes(i)));
        }

        var handler = new SearchPatientsGlobalQueryHandler(db);

        var page1 = await handler.Handle(new SearchPatientsGlobalQuery(null, 1, 7), CancellationToken.None);
        var page2 = await handler.Handle(new SearchPatientsGlobalQuery(null, 2, 7), CancellationToken.None);
        var page3 = await handler.Handle(new SearchPatientsGlobalQuery(null, 3, 7), CancellationToken.None);

        Assert.True(page1.IsSuccess);
        Assert.True(page2.IsSuccess);
        Assert.True(page3.IsSuccess);

        var paged = page1.Value!.Concat(page2.Value!).Concat(page3.Value!)
            .Select(h => h.PatientId).ToArray();

        Assert.Equal(20, paged.Length);
        Assert.Equal(20, paged.Distinct().Count());
        // Concatenating the pages reproduces the full descending order.
        Assert.Equal(paged.OrderByDescending(id => id), paged);
    }

    // ---------------------------------------------------------------------
    // Guardrail 5 — the soft-delete exclusion is applied before anything else
    // ---------------------------------------------------------------------

    [Fact]
    public async Task SearchGuardrail_SoftDelete_ExcludedWhenNoTextIsSupplied()
    {
        var db = new FakeApplicationDbContext();
        var deleted = MakePatient(1, "Deleted", new DateTime(2026, 3, 1, 12, 0, 0, DateTimeKind.Utc));
        deleted.SoftDelete();
        var live = MakePatient(2, "Live", new DateTime(2026, 3, 1, 12, 0, 0, DateTimeKind.Utc).AddMinutes(-1));
        db.Patients.Add(deleted);
        db.Patients.Add(live);

        var handler = new SearchPatientsGlobalQueryHandler(db);
        var result = await handler.Handle(new SearchPatientsGlobalQuery(null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var hit = Assert.Single(result.Value!);
        Assert.Equal(2, hit.PatientId);
    }

    [Fact]
    public async Task SearchGuardrail_SoftDelete_CountsTowardTheCap_ButIsNeverReturned()
    {
        var db = new FakeApplicationDbContext();
        var anchor = new DateTime(2026, 3, 1, 12, 0, 0, DateTimeKind.Utc);

        // The 60 newest rows are soft-deleted. A cap applied BEFORE the soft-delete
        // filter would hand back an empty page; applied after, it returns 50 live rows.
        for (var i = 1; i <= 60; i++)
        {
            var patient = MakePatient(i, $"Deleted{i:D3}", anchor.AddMinutes(-i));
            patient.SoftDelete();
            db.Patients.Add(patient);
        }

        // 100 live patients follow, so 50 live rows are genuinely available.
        for (var i = 61; i <= 160; i++)
        {
            db.Patients.Add(MakePatient(i, $"Live{i:D3}", anchor.AddMinutes(-i)));
        }

        var handler = new SearchPatientsGlobalQueryHandler(db);
        var result = await handler.Handle(new SearchPatientsGlobalQuery(null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(50, result.Value!.Count);
        Assert.DoesNotContain(result.Value!, h => h.PatientId <= 60);
        Assert.Equal(100, db.Patients.Count(p => !p.IsDeleted));
    }

    // ---------------------------------------------------------------------
    // Guardrail 6 — the existing text paths still work (C-8, SD-8)
    // ---------------------------------------------------------------------

    [Fact]
    public async Task SearchGuardrail_Text_NamePath_IsGatedByNameSearchAssist()
    {
        var dbEnabled = new FakeApplicationDbContext();
        dbEnabled.SystemSettings.Add(WithNameAssist(true));
        dbEnabled.Patients.Add(MakePatient(1, "Ahmed Mohamed", DateTime.UtcNow));
        dbEnabled.Patients.Add(MakePatient(2, "Sara Ali", DateTime.UtcNow.AddMinutes(-1)));

        var enabledResult = await new SearchPatientsGlobalQueryHandler(dbEnabled)
            .Handle(new SearchPatientsGlobalQuery("AHMED"), CancellationToken.None);

        Assert.True(enabledResult.IsSuccess);
        var enabledHit = Assert.Single(enabledResult.Value!);
        Assert.Equal(1, enabledHit.PatientId);

        var dbDisabled = new FakeApplicationDbContext();
        dbDisabled.SystemSettings.Add(WithNameAssist(false));
        dbDisabled.Patients.Add(MakePatient(1, "Ahmed Mohamed", DateTime.UtcNow));

        var disabledResult = await new SearchPatientsGlobalQueryHandler(dbDisabled)
            .Handle(new SearchPatientsGlobalQuery("Ahmed"), CancellationToken.None);

        Assert.True(disabledResult.IsSuccess);

        // SD-8: the assist flag's meaning and default are pinned here.
        Assert.Empty(disabledResult.Value!);
    }

    [Fact]
    public async Task SearchGuardrail_Text_NameAssistDefaultsOff_WhenNoSettingsRowExists()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1, "Ahmed Mohamed", DateTime.UtcNow));

        var result = await new SearchPatientsGlobalQueryHandler(db)
            .Handle(new SearchPatientsGlobalQuery("Ahmed"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }

    [Fact]
    public async Task SearchGuardrail_Text_LabIdPath_IsExactTrimmedMatchNotSubstring()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1, "Ahmed", DateTime.UtcNow, labId: "LAB-1"));
        db.Patients.Add(MakePatient(2, "Sara", DateTime.UtcNow.AddMinutes(-1)));

        var handler = new SearchPatientsGlobalQueryHandler(db);

        var exact = await handler.Handle(new SearchPatientsGlobalQuery("  LAB-1  "), CancellationToken.None);
        Assert.True(exact.IsSuccess);
        Assert.Equal(1, Assert.Single(exact.Value!).PatientId);

        var substring = await handler.Handle(new SearchPatientsGlobalQuery("LAB"), CancellationToken.None);
        Assert.True(substring.IsSuccess);

        // Narrowing, not broadening: a partial term must not return the row either.
        Assert.Empty(substring.Value!);
    }

    [Fact]
    public async Task SearchGuardrail_Text_NationalIdPath_IsExactTrimmedMatchNotSubstring()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1, "Ahmed", DateTime.UtcNow, nationalId: "29501010101010"));
        db.Patients.Add(MakePatient(2, "Sara", DateTime.UtcNow.AddMinutes(-1)));

        var handler = new SearchPatientsGlobalQueryHandler(db);

        var exact = await handler.Handle(new SearchPatientsGlobalQuery("29501010101010"), CancellationToken.None);
        Assert.True(exact.IsSuccess);
        Assert.Equal(1, Assert.Single(exact.Value!).PatientId);

        var substring = await handler.Handle(new SearchPatientsGlobalQuery("2950101"), CancellationToken.None);
        Assert.True(substring.IsSuccess);
        Assert.Empty(substring.Value!);
    }

    [Fact]
    public async Task SearchGuardrail_Text_PhonePath_NarrowsToTheOwningPatientOnly()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1, "Ahmed", DateTime.UtcNow));
        db.Patients.Add(MakePatient(2, "Sara", DateTime.UtcNow.AddMinutes(-1)));
        db.Patients.Add(MakePatient(3, "Nadia", DateTime.UtcNow.AddMinutes(-2)));
        AddPhone(db, 1, 1, "01012345678", 0);
        AddPhone(db, 2, 2, "01099999999", 0);

        var handler = new SearchPatientsGlobalQueryHandler(db);
        var result = await handler.Handle(new SearchPatientsGlobalQuery("12345678"), CancellationToken.None);

        Assert.True(result.IsSuccess);

        // C-8: the phone pre-query resolves ids and the patient Where narrows to them.
        var hit = Assert.Single(result.Value!);
        Assert.Equal(1, hit.PatientId);
    }

    [Fact]
    public async Task SearchGuardrail_Text_PhonePath_MatchesAnyStoredNumberOfThePatient()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1, "Ahmed", DateTime.UtcNow));
        AddPhone(db, 1, 1, "01012345678", 0);
        AddPhone(db, 2, 1, "01098765432", 1);

        var handler = new SearchPatientsGlobalQueryHandler(db);

        var byFirst = await handler.Handle(new SearchPatientsGlobalQuery("12345678"), CancellationToken.None);
        var bySecond = await handler.Handle(new SearchPatientsGlobalQuery("98765432"), CancellationToken.None);

        Assert.True(byFirst.IsSuccess);
        Assert.True(bySecond.IsSuccess);
        Assert.Equal(1, Assert.Single(byFirst.Value!).PatientId);
        Assert.Equal(1, Assert.Single(bySecond.Value!).PatientId);
    }

    [Fact]
    public async Task SearchGuardrail_Text_EmptyOrWhitespaceText_ReturnsEverythingUnfiltered()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1, "One", DateTime.UtcNow));
        db.Patients.Add(MakePatient(2, "Two", DateTime.UtcNow.AddMinutes(-1)));

        var handler = new SearchPatientsGlobalQueryHandler(db);

        var nullText = await handler.Handle(new SearchPatientsGlobalQuery(null), CancellationToken.None);
        var emptyText = await handler.Handle(new SearchPatientsGlobalQuery(string.Empty), CancellationToken.None);
        var blankText = await handler.Handle(new SearchPatientsGlobalQuery("   "), CancellationToken.None);

        Assert.True(nullText.IsSuccess);
        Assert.True(emptyText.IsSuccess);
        Assert.True(blankText.IsSuccess);
        Assert.Equal(2, nullText.Value!.Count);
        Assert.Equal(2, emptyText.Value!.Count);
        Assert.Equal(2, blankText.Value!.Count);
    }
}