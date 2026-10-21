using Microsoft.EntityFrameworkCore;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Patients;
using TopLab.Domain.Settings;
using TopLab.Infrastructure.Persistence;
using TopLab.Infrastructure.Tests.Common;
using Xunit;

namespace TopLab.Infrastructure.Tests.Persistence.Configurations;

/// <summary>
/// STAGE-01: the medical-history catalogue's seed (twelve rows: the existing
/// pregnancy row and the eleven rows the migration adds), the new patient
/// columns and the print-result password columns.
/// </summary>
public class StageOneMedicalHistoryConfigurationTests
{
    private static ApplicationDbContext CreateContext()
    {
        var ctx = new ApplicationDbContext(InMemoryContextFactory.Create());
        ctx.Database.EnsureCreated();
        return ctx;
    }

    [Fact]
    public void MedicalConditionType_SeedsTwelveRows()
    {
        using var ctx = CreateContext();

        Assert.Equal(12, ctx.Set<MedicalConditionType>().Count());
    }

    [Fact]
    public void MedicalConditionType_SeededRows_HaveTheBaselineIdsCategoriesAndLabels()
    {
        using var ctx = CreateContext();

        var rows = ctx.Set<MedicalConditionType>()
            .OrderBy(m => m.Id.Value)
            .ToList();

        var expected = new (int Id, string Name, MedicalConditionCategory Category)[]
        {
            (1, "حمل", MedicalConditionCategory.Pregnancy),
            (2, "هل يؤخذ علاج للسكر", MedicalConditionCategory.Medication),
            (3, "هل يؤخذ علاج لسيولة الدم", MedicalConditionCategory.Medication),
            (4, "هل يؤخذ علاج للغدد", MedicalConditionCategory.Medication),
            (5, "هل يؤخذ علاج للفيروسات", MedicalConditionCategory.Medication),
            (6, "هل يؤخذ مضاد حيوي", MedicalConditionCategory.Medication),
            (7, "هل يؤخذ علاج للكبد", MedicalConditionCategory.Medication),
            (8, "مريض بفقر الدم", MedicalConditionCategory.Condition),
            (9, "مريض بالتهاب المفاصل", MedicalConditionCategory.Condition),
            (10, "مريض بالضغط الدم", MedicalConditionCategory.Condition),
            (11, "مريض بالفشل كلوي", MedicalConditionCategory.Condition),
            (12, "مريض بالذئبة الحمراء", MedicalConditionCategory.Condition)
        };

        Assert.Equal(expected.Length, rows.Count);
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.Equal(expected[i].Id, rows[i].Id.Value);
            Assert.Equal(expected[i].Name, rows[i].Name);
            Assert.Equal(expected[i].Category, rows[i].Category);
        }
    }

    [Fact]
    public void MedicalConditionType_ThePregnancyRowIsUnchanged()
    {
        using var ctx = CreateContext();

        var pregnancy = Assert.Single(ctx.Set<MedicalConditionType>().Where(m => m.Id.Value == 1));
        Assert.Equal("حمل", pregnancy.Name);
        Assert.Equal(MedicalConditionCategory.Pregnancy, pregnancy.Category);
    }

    [Fact]
    public void MedicalConditionType_NoRowOtherThanThePregnancyOneIsPregnancy()
    {
        using var ctx = CreateContext();

        Assert.Equal(
            1,
            ctx.Set<MedicalConditionType>().Count(m => m.Category == MedicalConditionCategory.Pregnancy));
    }

    [Fact]
    public void Patient_CardNumber_IsNullableMax30AndIndexed()
    {
        using var ctx = new ApplicationDbContext(InMemoryContextFactory.Create());
        var et = ctx.Model.FindEntityType(typeof(Patient));
        Assert.NotNull(et);

        var prop = et!.FindProperty(nameof(Patient.CardNumber));
        Assert.NotNull(prop);
        Assert.True(prop!.IsNullable);
        Assert.Equal(30, prop.GetMaxLength());

        var idx = et.GetIndexes().FirstOrDefault(i => i.Properties.Any(p => p.Name == nameof(Patient.CardNumber)));
        Assert.NotNull(idx);
        Assert.False(idx!.IsUnique);
    }

    [Fact]
    public void Patient_RecentUltrasoundImaging_IsBitNotNullWithFalseDefault()
    {
        using var ctx = new ApplicationDbContext(InMemoryContextFactory.Create());
        var et = ctx.Model.FindEntityType(typeof(Patient));
        Assert.NotNull(et);

        var prop = et!.FindProperty(nameof(Patient.RecentUltrasoundImaging));
        Assert.NotNull(prop);
        Assert.False(prop!.IsNullable);
        Assert.Equal(false, prop.GetDefaultValue());
    }

    [Fact]
    public void SystemSettings_PrintResultPasswordColumns_AreMapped()
    {
        using var ctx = new ApplicationDbContext(InMemoryContextFactory.Create());
        var et = ctx.Model.FindEntityType(typeof(SystemSettings));
        Assert.NotNull(et);

        var enabled = et!.FindProperty(nameof(SystemSettings.PrintResultPasswordEnabled));
        Assert.NotNull(enabled);
        Assert.False(enabled!.IsNullable);
        Assert.Equal(false, enabled.GetDefaultValue());

        var hash = et.FindProperty(nameof(SystemSettings.PrintResultPasswordHash));
        Assert.NotNull(hash);
        Assert.True(hash!.IsNullable);
        Assert.Equal(300, hash.GetMaxLength());
    }

    [Fact]
    public void SystemSettings_TheSeededRowHasThePasswordRequirementOff()
    {
        using var ctx = CreateContext();

        var settings = Assert.Single(ctx.Set<SystemSettings>());
        Assert.False(settings.PrintResultPasswordEnabled);
        Assert.Null(settings.PrintResultPasswordHash);
    }
}
