using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;
using TopLab.Infrastructure.Persistence;
using TopLab.Infrastructure.Tests.Common;
using Xunit;

namespace TopLab.Infrastructure.Tests.Persistence.Configurations;

public class F5ConfigurationTests
{
    private static Microsoft.EntityFrameworkCore.Metadata.IEntityType GetEntityType<T>() where T : class
    {
        using var ctx = new ApplicationDbContext(InMemoryContextFactory.Create());
        var et = ctx.Model.FindEntityType(typeof(T));
        Assert.NotNull(et);
        return et!;
    }

    [Fact]
    public void CultureResults_HavePinnedKeysAndForeignKeyMatrix()
    {
        var header = GetEntityType<CultureResult>();
        Assert.Equal(new[] { nameof(CultureResult.PatientTestId) }, header.FindPrimaryKey()!.Properties.Select(x => x.Name));
        var headerFk = Assert.Single(header.GetForeignKeys());
        Assert.Equal(DeleteBehavior.Cascade, headerFk.DeleteBehavior);
        Assert.Equal(typeof(PatientTest), headerFk.PrincipalEntityType.ClrType);

        var sensitivity = GetEntityType<CultureAntibioticResult>();
        Assert.Equal(ValueGenerated.OnAdd, sensitivity.FindPrimaryKey()!.Properties.Single().ValueGenerated);
        Assert.Contains(sensitivity.GetForeignKeys(), x => x.PrincipalEntityType.ClrType == typeof(CultureResult) && x.DeleteBehavior == DeleteBehavior.Cascade);
        Assert.Contains(sensitivity.GetForeignKeys(), x => x.PrincipalEntityType.ClrType == typeof(Antibiotic) && x.DeleteBehavior == DeleteBehavior.Restrict);
        Assert.Contains(sensitivity.GetIndexes(), x => x.Properties.Any(p => p.Name == nameof(CultureAntibioticResult.PatientTestId)));
    }

    [Fact]
    public void MedicalConditionType_SeedsPregnancyCatalogRow()
    {
        using var ctx = new ApplicationDbContext(InMemoryContextFactory.Create());
        ctx.Database.EnsureCreated();
        Assert.Contains(ctx.Set<MedicalConditionType>(), x => x.Name == "حمل" && x.Category == TopLab.Domain.Common.Enums.MedicalConditionCategory.Pregnancy);
    }

    [Fact]
    public void Patient_HasExpectedColumns()
    {
        var et = GetEntityType<Patient>();
        Assert.NotNull(et.FindProperty(nameof(Patient.FullName)));
        Assert.NotNull(et.FindProperty(nameof(Patient.LabId)));
        Assert.NotNull(et.FindProperty(nameof(Patient.RegistrationDateUtc)));
        var idx = et.GetIndexes().FirstOrDefault(i => i.Properties.Any(p => p.Name == nameof(Patient.LabId)));
        Assert.NotNull(idx);
    }

    [Fact]
    public void Patient_IsDeleted_IsBitNotNull()
    {
        var et = GetEntityType<Patient>();
        var prop = et.FindProperty(nameof(Patient.IsDeleted));
        Assert.NotNull(prop);
        Assert.False(prop!.IsNullable);
    }

    [Fact]
    public void Patient_HasIndexOnIsDeleted()
    {
        var et = GetEntityType<Patient>();
        var idx = et.GetIndexes().FirstOrDefault(i => i.Properties.Any(p => p.Name == nameof(Patient.IsDeleted)));
        Assert.NotNull(idx);
    }

    [Fact]
    public void PatientPhoneNumber_HasIndexOnPhoneNumber()
    {
        var et = GetEntityType<PatientPhoneNumber>();
        var idx = et.GetIndexes().FirstOrDefault(i => i.Properties.Any(p => p.Name == nameof(PatientPhoneNumber.PhoneNumber)));
        Assert.NotNull(idx);
    }

    [Fact]
    public void Test_HasDecimalPrecision()
    {
        var et = GetEntityType<TopLab.Domain.Tests.Test>();
        var prop = et.FindProperty(nameof(TopLab.Domain.Tests.Test.PatientPrice));
        Assert.NotNull(prop);
        Assert.Equal(18, prop.GetPrecision());
        Assert.Equal(2, prop.GetScale());
    }

    [Fact]
    public void ReferenceRange_HasDecimalPrecision_18_4()
    {
        var et = GetEntityType<TopLab.Domain.Tests.ReferenceRange>();
        var prop = et.FindProperty(nameof(TopLab.Domain.Tests.ReferenceRange.MinValue));
        Assert.Equal(18, prop!.GetPrecision());
        Assert.Equal(4, prop.GetScale());
    }

    [Fact]
    public void SystemSettings_ShouldHaveSingletonSeed()
    {
        using var ctx = new ApplicationDbContext(InMemoryContextFactory.Create());
        ctx.Database.EnsureCreated();
        var count = ctx.Set<TopLab.Domain.Settings.SystemSettings>().Count();
        Assert.Equal(1, count);
    }

    [Fact]
    public void Permission_SeedCount_Is13()
    {
        using var ctx = new ApplicationDbContext(InMemoryContextFactory.Create());
        ctx.Database.EnsureCreated();
        var perms = ctx.Set<TopLab.Domain.Users.Permission>().ToList();
        Assert.Equal(13, perms.Count);
    }

    [Fact]
    public void EnvelopePrintItemPosition_SeedCount_Is4()
    {
        using var ctx = new ApplicationDbContext(InMemoryContextFactory.Create());
        ctx.Database.EnsureCreated();
        Assert.Equal(4, ctx.Set<TopLab.Domain.Settings.EnvelopePrintItemPosition>().Count());
    }

    [Fact]
    public void PatientTest_HasCompositeIndex()
    {
        var et = GetEntityType<TopLab.Domain.Results.PatientTest>();
        var idx = et.GetIndexes().FirstOrDefault(i => i.Properties.Count == 3);
        Assert.NotNull(idx);
    }

    [Fact]
    public void PatientTest_HasCompositeIndex_OnPatientIdAndIsSampleDrawn()
    {
        var et = GetEntityType<TopLab.Domain.Results.PatientTest>();
        var idx = et.GetIndexes().FirstOrDefault(i =>
            i.Properties.Count == 2
            && i.Properties.Any(p => p.Name == nameof(TopLab.Domain.Results.PatientTest.PatientId))
            && i.Properties.Any(p => p.Name == nameof(TopLab.Domain.Results.PatientTest.IsSampleDrawn)));
        Assert.NotNull(idx);
    }

    [Fact]
    public void Test_HasTestCodeColumn_UniqueIndex()
    {
        var et = GetEntityType<TopLab.Domain.Tests.Test>();
        var prop = et.FindProperty(nameof(TopLab.Domain.Tests.Test.TestCode));
        Assert.NotNull(prop);
        Assert.False(prop.IsNullable);
        Assert.Equal(50, prop.GetMaxLength());
        var idx = et.GetIndexes().FirstOrDefault(i => i.Properties.Any(p => p.Name == nameof(TopLab.Domain.Tests.Test.TestCode)));
        Assert.NotNull(idx);
        Assert.True(idx!.IsUnique);
    }

    [Fact]
    public void Test_HasIsActiveColumn_DefaultTrue()
    {
        var et = GetEntityType<TopLab.Domain.Tests.Test>();
        var prop = et.FindProperty(nameof(TopLab.Domain.Tests.Test.IsActive));
        Assert.NotNull(prop);
        Assert.False(prop.IsNullable);
        Assert.Equal(true, prop.GetDefaultValue());
    }

    [Fact]
    public void TestGroup_HasIsActiveColumn_DefaultTrue()
    {
        var et = GetEntityType<TopLab.Domain.Tests.TestGroup>();
        var prop = et.FindProperty(nameof(TopLab.Domain.Tests.TestGroup.IsActive));
        Assert.NotNull(prop);
        Assert.False(prop.IsNullable);
        Assert.Equal(true, prop.GetDefaultValue());
    }

    [Fact]
    public void ExternalEntity_HasExpectedMapping()
    {
        var et = GetEntityType<TopLab.Domain.ExternalEntities.ExternalEntity>();

        var name = et.FindProperty(nameof(TopLab.Domain.ExternalEntities.ExternalEntity.Name));
        Assert.NotNull(name);
        Assert.False(name!.IsNullable);
        Assert.Equal(200, name.GetMaxLength());

        var city = et.FindProperty(nameof(TopLab.Domain.ExternalEntities.ExternalEntity.City));
        Assert.NotNull(city);
        Assert.True(city!.IsNullable);
        Assert.Equal(100, city.GetMaxLength());

        var code = et.FindProperty(nameof(TopLab.Domain.ExternalEntities.ExternalEntity.GeneratedIdCode));
        Assert.NotNull(code);
        Assert.True(code!.IsNullable);
        Assert.Equal(50, code.GetMaxLength());

        var priceListId = et.FindProperty(nameof(TopLab.Domain.ExternalEntities.ExternalEntity.PriceListId));
        Assert.NotNull(priceListId);
        Assert.True(priceListId!.IsNullable);

        var percent = et.FindProperty(nameof(TopLab.Domain.ExternalEntities.ExternalEntity.DiscountOrCommissionPercent));
        Assert.NotNull(percent);
        Assert.True(percent!.IsNullable);
        Assert.Equal(5, percent.GetPrecision());
        Assert.Equal(2, percent.GetScale());

        var typeIndex = et.GetIndexes().FirstOrDefault(i =>
            i.Properties.Any(p => p.Name == nameof(TopLab.Domain.ExternalEntities.ExternalEntity.EntityType)));
        Assert.NotNull(typeIndex);
    }

    [Fact]
    public void ExternalEntity_HasNoUniqueIndexOnGeneratedIdCode()
    {
        var et = GetEntityType<TopLab.Domain.ExternalEntities.ExternalEntity>();

        Assert.DoesNotContain(et.GetIndexes(), i =>
            i.IsUnique && i.Properties.Any(p => p.Name == nameof(TopLab.Domain.ExternalEntities.ExternalEntity.GeneratedIdCode)));
    }

    [Fact]
    public void PriceList_HasExpectedMapping()
    {
        var et = GetEntityType<PriceList>();

        var id = et.FindProperty("Id");
        Assert.NotNull(id);
        Assert.True(id!.ValueGenerated == ValueGenerated.OnAdd);
        Assert.Equal("PriceListId", id.GetColumnName());

        var name = et.FindProperty(nameof(PriceList.Name));
        Assert.NotNull(name);
        Assert.False(name!.IsNullable);
        Assert.Equal(150, name.GetMaxLength());

        Assert.Contains(et.GetNavigations(), n => n.Name == "Items");
    }

    [Fact]
    public void PriceListItem_HasCompositeKeyAndDecimalPrecision()
    {
        var et = GetEntityType<PriceListItem>();

        var key = et.FindPrimaryKey();
        Assert.NotNull(key);
        Assert.Equal(new[] { nameof(PriceListItem.PriceListId), nameof(PriceListItem.TestId) }, key!.Properties.Select(p => p.Name).ToArray());

        var price = et.FindProperty(nameof(PriceListItem.Price));
        Assert.NotNull(price);
        Assert.False(price!.IsNullable);
        Assert.Equal(18, price.GetPrecision());
        Assert.Equal(2, price.GetScale());
    }

    [Fact]
    public void CustomGroup_HasExpectedMapping()
    {
        var et = GetEntityType<TopLab.Domain.Tests.CustomGroup>();

        var id = et.FindProperty("Id");
        Assert.NotNull(id);
        Assert.True(id!.ValueGenerated == ValueGenerated.OnAdd);
        Assert.Equal("CustomGroupId", id.GetColumnName());

        var name = et.FindProperty(nameof(TopLab.Domain.Tests.CustomGroup.Name));
        Assert.NotNull(name);
        Assert.False(name!.IsNullable);
        Assert.Equal(150, name.GetMaxLength());

        Assert.Contains(et.GetNavigations(), n => n.Name == "Items");
    }

    [Fact]
    public void CustomGroupItem_HasCompositeKeyAndDecimalPrecision()
    {
        var et = GetEntityType<TopLab.Domain.Tests.CustomGroupItem>();

        var key = et.FindPrimaryKey();
        Assert.NotNull(key);
        Assert.Equal(new[] { nameof(TopLab.Domain.Tests.CustomGroupItem.CustomGroupId), nameof(TopLab.Domain.Tests.CustomGroupItem.TestId) }, key!.Properties.Select(p => p.Name).ToArray());

        var price = et.FindProperty(nameof(TopLab.Domain.Tests.CustomGroupItem.Price));
        Assert.NotNull(price);
        Assert.False(price!.IsNullable);
        Assert.Equal(18, price.GetPrecision());
        Assert.Equal(2, price.GetScale());
    }

[Fact]
    public void TestComment_HasExpectedMapping()
    {
        var et = GetEntityType<TopLab.Domain.Tests.TestComment>();

        var id = et.FindProperty("Id");
        Assert.NotNull(id);
        Assert.True(id!.ValueGenerated == ValueGenerated.OnAdd);
        Assert.Equal("TestCommentId", id.GetColumnName());

        var text = et.FindProperty(nameof(TopLab.Domain.Tests.TestComment.CommentText));
        Assert.NotNull(text);
        Assert.False(text!.IsNullable);
        Assert.Equal(1000, text.GetMaxLength());

        var testIdIdx = et.GetIndexes().FirstOrDefault(i =>
            i.Properties.Any(p => p.Name == nameof(TopLab.Domain.Tests.TestComment.TestId)));
        Assert.NotNull(testIdIdx);
    }

    [Fact]
    public void Antibiotic_HasExpectedMapping()
    {
        var et = GetEntityType<TopLab.Domain.Tests.Antibiotic>();

        var id = et.FindProperty("Id");
        Assert.NotNull(id);
        Assert.True(id!.ValueGenerated == ValueGenerated.OnAdd);
        Assert.Equal("AntibioticId", id.GetColumnName());

        var name = et.FindProperty(nameof(TopLab.Domain.Tests.Antibiotic.Name));
        Assert.NotNull(name);
        Assert.False(name!.IsNullable);
        Assert.Equal(150, name.GetMaxLength());

        var pregnancy = et.FindProperty(nameof(TopLab.Domain.Tests.Antibiotic.IsPregnancyFlagged));
        Assert.NotNull(pregnancy);
        Assert.False(pregnancy!.IsNullable);

        var children = et.FindProperty(nameof(TopLab.Domain.Tests.Antibiotic.IsChildrenFlagged));
        Assert.NotNull(children);
        Assert.False(children!.IsNullable);
    }

    [Fact]
    public void CultureAntibioticAttachment_HasCompositeKey()
    {
        var et = GetEntityType<TopLab.Domain.Tests.CultureAntibioticAttachment>();

        var key = et.FindPrimaryKey();
        Assert.NotNull(key);
        Assert.Equal(new[]
        {
            nameof(TopLab.Domain.Tests.CultureAntibioticAttachment.TestId),
            nameof(TopLab.Domain.Tests.CultureAntibioticAttachment.AntibioticId)
        }, key!.Properties.Select(p => p.Name).ToArray());

Assert.Empty(et.GetForeignKeys());
    }

    [Fact]
    public void PaymentOperation_HasExpectedMapping()
    {
        var et = GetEntityType<PaymentOperation>();

        var amount = et.FindProperty(nameof(PaymentOperation.Amount));
        Assert.NotNull(amount);
        Assert.False(amount!.IsNullable);
        Assert.Equal(18, amount.GetPrecision());
        Assert.Equal(2, amount.GetScale());

        var discount = et.FindProperty(nameof(PaymentOperation.DiscountAmount));
        Assert.NotNull(discount);
        Assert.True(discount!.IsNullable);
        Assert.Equal(18, discount.GetPrecision());
        Assert.Equal(2, discount.GetScale());

        var operationType = et.FindProperty(nameof(PaymentOperation.OperationType));
        Assert.NotNull(operationType);
        Assert.False(operationType!.IsNullable);
        // HasColumnType("tinyint") is a relational annotation the InMemory provider
        // does not surface; the int conversion below is its InMemory-observable proxy.
        // The tinyint annotation itself is pinned by the zero-drift gate (snapshot).
        Assert.Equal(typeof(int), operationType.GetProviderClrType());

        var fk = Assert.Single(et.GetForeignKeys());
        Assert.Equal(DeleteBehavior.Cascade, fk.DeleteBehavior);
        Assert.Equal(typeof(Patient), fk.PrincipalEntityType.ClrType);

        var idx = et.GetIndexes().FirstOrDefault(i =>
            i.Properties.Any(p => p.Name == nameof(PaymentOperation.PatientId)));
        Assert.NotNull(idx);
    }

    [Fact]
    public void PaymentOperation_IsEffectivelyZero_IsNotMapped()
    {
        var et = GetEntityType<PaymentOperation>();

        Assert.Null(et.FindProperty(nameof(PaymentOperation.IsEffectivelyZero)));
    }

    [Fact]
    public void PatientTestReferenceRangeSnapshot_HasExpectedMapping()
    {
        var et = GetEntityType<TopLab.Domain.Results.PatientTestReferenceRangeSnapshot>();

        var key = et.FindPrimaryKey();
        Assert.NotNull(key);
        Assert.Equal(
            new[] { nameof(TopLab.Domain.Results.PatientTestReferenceRangeSnapshot.PatientTestId) },
            key!.Properties.Select(p => p.Name).ToArray());

        var min = et.FindProperty(nameof(TopLab.Domain.Results.PatientTestReferenceRangeSnapshot.MinValue));
        Assert.NotNull(min);
        Assert.False(min!.IsNullable);
        Assert.Equal(18, min.GetPrecision());
        Assert.Equal(4, min.GetScale());

        var max = et.FindProperty(nameof(TopLab.Domain.Results.PatientTestReferenceRangeSnapshot.MaxValue));
        Assert.NotNull(max);
        Assert.False(max!.IsNullable);
        Assert.Equal(18, max.GetPrecision());
        Assert.Equal(4, max.GetScale());

        var low = et.FindProperty(nameof(TopLab.Domain.Results.PatientTestReferenceRangeSnapshot.LowComment));
        Assert.NotNull(low);
        Assert.True(low!.IsNullable);
        Assert.Equal(500, low.GetMaxLength());

        var high = et.FindProperty(nameof(TopLab.Domain.Results.PatientTestReferenceRangeSnapshot.HighComment));
        Assert.NotNull(high);
        Assert.True(high!.IsNullable);
        Assert.Equal(500, high.GetMaxLength());

        var sex = et.FindProperty(nameof(TopLab.Domain.Results.PatientTestReferenceRangeSnapshot.Sex));
        Assert.NotNull(sex);
        Assert.True(sex!.IsNullable);
        // HasColumnType("tinyint") is a relational annotation the InMemory provider
        // does not surface (GetProviderClrType() is null for the nullable enum);
        // the tinyint annotation itself is pinned by the zero-drift gate (snapshot).
        Assert.Equal(typeof(TopLab.Domain.Common.Enums.Sex?), sex.ClrType);

        var ageUnit = et.FindProperty(nameof(TopLab.Domain.Results.PatientTestReferenceRangeSnapshot.AgeUnit));
        Assert.NotNull(ageUnit);
        Assert.False(ageUnit!.IsNullable);
        Assert.Equal(typeof(TopLab.Domain.Common.Enums.AgeUnit), ageUnit.ClrType);

        var captured = et.FindProperty(nameof(TopLab.Domain.Results.PatientTestReferenceRangeSnapshot.CapturedAtUtc));
        Assert.NotNull(captured);
        Assert.False(captured!.IsNullable);
        Assert.Equal(typeof(DateTimeOffset), captured.ClrType);

        var fk = Assert.Single(et.GetForeignKeys());
        Assert.Equal(DeleteBehavior.Cascade, fk.DeleteBehavior);
        Assert.Equal(typeof(TopLab.Domain.Results.PatientTest), fk.PrincipalEntityType.ClrType);
    }

    [Fact]
    public void Analyte_HasUniqueName_AndCurrentRange()
    {
        var et = GetEntityType<Analyte>();

        var id = et.FindProperty("Id");
        Assert.NotNull(id);
        Assert.True(id!.ValueGenerated == ValueGenerated.OnAdd);
        Assert.Equal("AnalyteId", id.GetColumnName());

        var name = et.FindProperty(nameof(Analyte.Name));
        Assert.NotNull(name);
        Assert.False(name!.IsNullable);
        Assert.Equal(150, name.GetMaxLength());

        var idx = et.GetIndexes().FirstOrDefault(i => i.Properties.Any(p => p.Name == nameof(Analyte.Name)));
        Assert.NotNull(idx);
        Assert.True(idx!.IsUnique);

        var rangeNav = et.GetNavigations().FirstOrDefault(n => n.Name == nameof(Analyte.CurrentRange));
        Assert.NotNull(rangeNav);
        Assert.Equal(typeof(AnalyteReferenceRange), rangeNav!.ClrType);
    }

    [Fact]
    public void AnalyteReferenceRange_HasUniqueAnalyteId_OneAggregatePerAnalyte()
    {
        var et = GetEntityType<AnalyteReferenceRange>();

        var id = et.FindProperty("Id");
        Assert.NotNull(id);
        Assert.True(id!.ValueGenerated == ValueGenerated.OnAdd);
        Assert.Equal("AnalyteReferenceRangeId", id.GetColumnName());

        var idx = et.GetIndexes().FirstOrDefault(i => i.Properties.Any(p => p.Name == nameof(AnalyteReferenceRange.AnalyteId)));
        Assert.NotNull(idx);
        Assert.True(idx!.IsUnique);
    }

    [Fact]
    public void AnalyteReferenceRangeBand_HasBandShape_AndDecimalPrecision()
    {
        var et = GetEntityType<AnalyteReferenceRangeBand>();

        var min = et.FindProperty(nameof(AnalyteReferenceRangeBand.MinValue));
        Assert.NotNull(min);
        Assert.False(min!.IsNullable);
        Assert.Equal(18, min.GetPrecision());
        Assert.Equal(4, min.GetScale());

        var max = et.FindProperty(nameof(AnalyteReferenceRangeBand.MaxValue));
        Assert.NotNull(max);
        Assert.False(max!.IsNullable);
        Assert.Equal(18, max.GetPrecision());
        Assert.Equal(4, max.GetScale());

        var low = et.FindProperty(nameof(AnalyteReferenceRangeBand.LowComment));
        Assert.NotNull(low);
        Assert.True(low!.IsNullable);
        Assert.Equal(500, low.GetMaxLength());

        var rangeIdx = et.GetIndexes().FirstOrDefault(i =>
            i.Properties.Any(p => p.Name == nameof(AnalyteReferenceRangeBand.AnalyteReferenceRangeId)));
        Assert.NotNull(rangeIdx);
    }

    [Fact]
    public void Profile_HasUniqueTestPairing_AndImmutableFixedPrice()
    {
        var et = GetEntityType<Profile>();

        var id = et.FindProperty("Id");
        Assert.NotNull(id);
        Assert.True(id!.ValueGenerated == ValueGenerated.OnAdd);
        Assert.Equal("ProfileId", id.GetColumnName());

        var price = et.FindProperty(nameof(Profile.FixedPrice));
        Assert.NotNull(price);
        Assert.False(price!.IsNullable);
        Assert.Equal(18, price.GetPrecision());
        Assert.Equal(2, price.GetScale());

        var testIdx = et.GetIndexes().FirstOrDefault(i =>
            i.Properties.Any(p => p.Name == nameof(Profile.TestId)));
        Assert.NotNull(testIdx);
        Assert.True(testIdx!.IsUnique);

        var active = et.FindProperty(nameof(Profile.IsActive));
        Assert.NotNull(active);
        Assert.False(active!.IsNullable);
        Assert.Equal(true, active.GetDefaultValue());
    }

    [Fact]
    public void ProfileAnalyte_HasUniqueProfileAnalytePair()
    {
        var et = GetEntityType<ProfileAnalyte>();

        var idx = et.GetIndexes().FirstOrDefault(i =>
            i.Properties.Any(p => p.Name == nameof(ProfileAnalyte.ProfileId))
            && i.Properties.Any(p => p.Name == nameof(ProfileAnalyte.AnalyteId)));
        Assert.NotNull(idx);
        Assert.True(idx!.IsUnique);
        Assert.Equal(2, idx!.Properties.Count);
    }

    [Fact]
    public void Test_HasNullableAnalyteIdMapping()
    {
        var et = GetEntityType<Test>();

        var analyteId = et.FindProperty(nameof(Test.AnalyteId));
        Assert.NotNull(analyteId);
        Assert.True(analyteId!.IsNullable);

        var idx = et.GetIndexes().FirstOrDefault(i =>
            i.Properties.Any(p => p.Name == nameof(Test.AnalyteId)));
        Assert.NotNull(idx);
    }

    [Fact]
    public void ProfileResultItem_HasRequiredAnalyteId_NotLegacyName()
    {
        var et = GetEntityType<ProfileResultItem>();

        Assert.Null(et.FindProperty("AnalyteName"));

        var analyteId = et.FindProperty(nameof(ProfileResultItem.AnalyteId));
        Assert.NotNull(analyteId);
        Assert.False(analyteId!.IsNullable);
        Assert.Equal(typeof(AnalyteId), analyteId.ClrType);

        var fk = et.GetForeignKeys().FirstOrDefault(f =>
            f.Properties.Any(p => p.Name == nameof(ProfileResultItem.AnalyteId)));
        Assert.NotNull(fk);
        Assert.Equal(typeof(Analyte), fk!.PrincipalEntityType.ClrType);
        Assert.Equal(DeleteBehavior.Restrict, fk!.DeleteBehavior);

        var printCount = et.FindProperty(nameof(ProfileResultItem.PrintCount));
        Assert.NotNull(printCount);
    }

    [Fact]
    public void ProfileResultItemReferenceRangeSnapshot_HasExpectedMapping()
    {
        var et = GetEntityType<ProfileResultItemReferenceRangeSnapshot>();

        var key = et.FindPrimaryKey();
        Assert.NotNull(key);
        Assert.Equal(
            new[] { nameof(ProfileResultItemReferenceRangeSnapshot.ProfileResultItemId) },
            key!.Properties.Select(p => p.Name).ToArray());

        var analyteId = et.FindProperty(nameof(ProfileResultItemReferenceRangeSnapshot.AnalyteId));
        Assert.NotNull(analyteId);
        Assert.False(analyteId!.IsNullable);

        // Historical identity — no FK to the live analyte (mirrors M-04 snapshot).
        Assert.DoesNotContain(et.GetForeignKeys(), f =>
            f.PrincipalEntityType.ClrType == typeof(Analyte));

        var min = et.FindProperty(nameof(ProfileResultItemReferenceRangeSnapshot.MinValue));
        Assert.NotNull(min);
        Assert.Equal(18, min!.GetPrecision());
        Assert.Equal(4, min.GetScale());

        var captured = et.FindProperty(nameof(ProfileResultItemReferenceRangeSnapshot.CapturedAtUtc));
        Assert.NotNull(captured);
        Assert.Equal(typeof(DateTimeOffset), captured!.ClrType);
    }

    [Fact]
    public void ProfileResultAmendment_HasExpectedMapping_AndCascade()
    {
        var et = GetEntityType<ProfileResultAmendment>();

        var id = et.FindProperty("Id");
        Assert.NotNull(id);
        Assert.True(id!.ValueGenerated == ValueGenerated.OnAdd);
        Assert.Equal("ProfileResultAmendmentId", id.GetColumnName());

        var fk = Assert.Single(et.GetForeignKeys());
        Assert.Equal(DeleteBehavior.Cascade, fk.DeleteBehavior);
        Assert.Equal(typeof(ProfileResultItem), fk.PrincipalEntityType.ClrType);

        var idx = et.GetIndexes().FirstOrDefault(i =>
            i.Properties.Any(p => p.Name == nameof(ProfileResultAmendment.ProfileResultItemId)));
        Assert.NotNull(idx);
    }
}
