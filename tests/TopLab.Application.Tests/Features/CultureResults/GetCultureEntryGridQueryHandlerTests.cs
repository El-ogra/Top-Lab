using TopLab.Application.Features.CultureResults.Queries.GetCultureEntryGrid;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.CultureResults;

/// <summary>
/// REF-093: pregnancy-flag visibility is governed solely by
/// CultureAntibioticDisplay.IsDisplayable — a pre-existing saved result
/// must not force a hidden antibiotic back into the grid.
/// REF-094: same rule for the children flag (including age-unit awareness).
/// </summary>
public sealed class GetCultureEntryGridQueryHandlerTests
{
    [Fact]
    public async Task Grid_PregnancyOnlyAntibioticWithSavedResult_ExcludedForNonPregnantAdult()
    {
        var db = Seed(pregnant: false);
        db.CultureAntibioticResults.Add(CultureAntibioticResult.Create(
            CultureAntibioticResultId.Create(10),
            PatientTestId.Create(10),
            AntibioticId.Create(2),
            SensitivityCategory.ResistantFor));

        var result = await new GetCultureEntryGridQueryHandler(db).Handle(new GetCultureEntryGridQuery(10), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { 1 }, result.Value!.Rows.Select(x => x.AntibioticId));
    }

    [Fact]
    public async Task Grid_PregnancyOnlyAntibioticWithSavedResult_IncludedWhenPregnancyIndicated()
    {
        var db = Seed(pregnant: true);
        db.CultureAntibioticResults.Add(CultureAntibioticResult.Create(
            CultureAntibioticResultId.Create(10),
            PatientTestId.Create(10),
            AntibioticId.Create(2),
            SensitivityCategory.ResistantFor));

        var result = await new GetCultureEntryGridQueryHandler(db).Handle(new GetCultureEntryGridQuery(10), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { 1, 2 }, result.Value!.Rows.Select(x => x.AntibioticId));
    }

    [Fact]
    public async Task Grid_UnflaggedAntibioticWithSavedResult_IsIncluded()
    {
        var db = Seed(pregnant: false);
        db.CultureAntibioticResults.Add(CultureAntibioticResult.Create(
            CultureAntibioticResultId.Create(11),
            PatientTestId.Create(10),
            AntibioticId.Create(1),
            SensitivityCategory.HighlyFor));

        var result = await new GetCultureEntryGridQueryHandler(db).Handle(new GetCultureEntryGridQuery(10), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Contains(result.Value!.Rows, row => row.AntibioticId == 1 && row.CultureAntibioticResultId == 11);
    }

    [Fact]
    public async Task Grid_ExcludedAntibioticSavedResult_DoesNotAppearInProjection()
    {
        var db = Seed(pregnant: false);
        db.CultureAntibioticResults.Add(CultureAntibioticResult.Create(
            CultureAntibioticResultId.Create(10),
            PatientTestId.Create(10),
            AntibioticId.Create(2),
            SensitivityCategory.ResistantFor));

        var result = await new GetCultureEntryGridQueryHandler(db).Handle(new GetCultureEntryGridQuery(10), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.DoesNotContain(result.Value!.Rows, row => row.AntibioticId == 2);
        Assert.DoesNotContain(result.Value!.Rows, row => row.CultureAntibioticResultId == 10);
        var single = Assert.Single(result.Value!.Rows);
        Assert.Equal(1, single.AntibioticId);
        Assert.Null(single.SensitivityCategory);
    }

    private static FakeApplicationDbContext Seed(bool pregnant)
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(Patient.Create(PatientId.Create(1), "Patient", Sex.Female, 30, AgeUnit.Year, DateTime.UtcNow));
        db.Tests.Add(Test.Create(TestId.Create(1), "Culture", "Culture report", "Culture", "CULT", 1, 100m, ResultKind.Culture, isCultureType: true));
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(10), PatientId.Create(1), TestId.Create(1), 100m));
        db.Antibiotics.Add(Antibiotic.Create(AntibioticId.Create(1), "Normal"));
        db.Antibiotics.Add(Antibiotic.Create(AntibioticId.Create(2), "Pregnancy", isPregnancyFlagged: true));
        db.CultureAntibioticAttachments.Add(new CultureAntibioticAttachment(TestId.Create(1), AntibioticId.Create(1)));
        db.CultureAntibioticAttachments.Add(new CultureAntibioticAttachment(TestId.Create(1), AntibioticId.Create(2)));
        if (pregnant)
        {
            db.MedicalConditionTypes.Add(MedicalConditionType.Create(MedicalConditionTypeId.Create(7), "حمل", MedicalConditionCategory.Pregnancy));
            db.PatientMedicalConditions.Add(PatientMedicalCondition.Create(PatientId.Create(1), MedicalConditionTypeId.Create(7)));
        }

        return db;
    }

    [Fact]
    public async Task Grid_ChildrenOnlyAntibioticWithSavedResult_ExcludedForAdult_Years()
    {
        var db = SeedChild(age: 30, unit: AgeUnit.Year);
        db.CultureAntibioticResults.Add(CultureAntibioticResult.Create(
            CultureAntibioticResultId.Create(20),
            PatientTestId.Create(10),
            AntibioticId.Create(2),
            SensitivityCategory.ResistantFor));

        var result = await new GetCultureEntryGridQueryHandler(db).Handle(new GetCultureEntryGridQuery(10), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { 1 }, result.Value!.Rows.Select(x => x.AntibioticId));
    }

    [Fact]
    public async Task Grid_ChildrenOnlyAntibioticWithSavedResult_ExcludedForAdult_Months()
    {
        // 144 months = 12 years: NOT under twelve, so the children-only
        // antibiotic stays hidden even with a saved result (unit-awareness).
        var db = SeedChild(age: 144, unit: AgeUnit.Month);
        db.CultureAntibioticResults.Add(CultureAntibioticResult.Create(
            CultureAntibioticResultId.Create(20),
            PatientTestId.Create(10),
            AntibioticId.Create(2),
            SensitivityCategory.ResistantFor));

        var result = await new GetCultureEntryGridQueryHandler(db).Handle(new GetCultureEntryGridQuery(10), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { 1 }, result.Value!.Rows.Select(x => x.AntibioticId));
    }

    [Fact]
    public async Task Grid_ChildrenOnlyAntibioticWithSavedResult_IncludedForChildUnder12()
    {
        var db = SeedChild(age: 11, unit: AgeUnit.Year);
        db.CultureAntibioticResults.Add(CultureAntibioticResult.Create(
            CultureAntibioticResultId.Create(20),
            PatientTestId.Create(10),
            AntibioticId.Create(2),
            SensitivityCategory.ResistantFor));

        var result = await new GetCultureEntryGridQueryHandler(db).Handle(new GetCultureEntryGridQuery(10), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { 1, 2, 3 }, result.Value!.Rows.Select(x => x.AntibioticId));
    }

    [Fact]
    public async Task Grid_UnflaggedAntibioticWithSavedResult_IsIncludedForAdult()
    {
        var db = SeedChild(age: 30, unit: AgeUnit.Year);
        db.CultureAntibioticResults.Add(CultureAntibioticResult.Create(
            CultureAntibioticResultId.Create(21),
            PatientTestId.Create(10),
            AntibioticId.Create(1),
            SensitivityCategory.HighlyFor));

        var result = await new GetCultureEntryGridQueryHandler(db).Handle(new GetCultureEntryGridQuery(10), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Contains(result.Value!.Rows, row => row.AntibioticId == 1 && row.CultureAntibioticResultId == 21);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task Grid_BothFlagsAntibiotic_IncludedWhenEitherContextHolds(bool pregnant, bool isChild)
    {
        var db = SeedChild(age: isChild ? 11 : 30, unit: AgeUnit.Year, pregnant: pregnant);
        db.CultureAntibioticResults.Add(CultureAntibioticResult.Create(
            CultureAntibioticResultId.Create(22),
            PatientTestId.Create(10),
            AntibioticId.Create(3),
            SensitivityCategory.ModerateFor));

        var result = await new GetCultureEntryGridQueryHandler(db).Handle(new GetCultureEntryGridQuery(10), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Contains(result.Value!.Rows, row => row.AntibioticId == 3 && row.CultureAntibioticResultId == 22);
    }

    private static FakeApplicationDbContext SeedChild(int age, AgeUnit unit, bool pregnant = false)
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(Patient.Create(PatientId.Create(1), "Patient", Sex.Female, age, unit, DateTime.UtcNow));
        db.Tests.Add(Test.Create(TestId.Create(1), "Culture", "Culture report", "Culture", "CULT", 1, 100m, ResultKind.Culture, isCultureType: true));
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(10), PatientId.Create(1), TestId.Create(1), 100m));
        db.Antibiotics.Add(Antibiotic.Create(AntibioticId.Create(1), "Normal"));
        db.Antibiotics.Add(Antibiotic.Create(AntibioticId.Create(2), "Child", isChildrenFlagged: true));
        db.Antibiotics.Add(Antibiotic.Create(AntibioticId.Create(3), "Both", isPregnancyFlagged: true, isChildrenFlagged: true));
        db.CultureAntibioticAttachments.Add(new CultureAntibioticAttachment(TestId.Create(1), AntibioticId.Create(1)));
        db.CultureAntibioticAttachments.Add(new CultureAntibioticAttachment(TestId.Create(1), AntibioticId.Create(2)));
        db.CultureAntibioticAttachments.Add(new CultureAntibioticAttachment(TestId.Create(1), AntibioticId.Create(3)));
        if (pregnant)
        {
            db.MedicalConditionTypes.Add(MedicalConditionType.Create(MedicalConditionTypeId.Create(7), "حمل", MedicalConditionCategory.Pregnancy));
            db.PatientMedicalConditions.Add(PatientMedicalCondition.Create(PatientId.Create(1), MedicalConditionTypeId.Create(7)));
        }

        return db;
    }
}
