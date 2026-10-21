using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using Xunit;

namespace TopLab.Domain.Tests.Patients;

/// <summary>
/// STAGE-01 (baseline 1.2.2.3): the medical-history catalogue's rename rule.
/// </summary>
public class MedicalConditionTypeTests
{
    [Fact]
    public void Create_Valid_StoresNameAndCategory()
    {
        var type = MedicalConditionType.Create(
            MedicalConditionTypeId.Create(2), "هل يؤخذ علاج للسكر", MedicalConditionCategory.Medication);

        Assert.Equal("هل يؤخذ علاج للسكر", type.Name);
        Assert.Equal(MedicalConditionCategory.Medication, type.Category);
    }

    [Fact]
    public void Create_BlankName_Throws()
    {
        Assert.Throws<ArgumentException>(() => MedicalConditionType.Create(
            MedicalConditionTypeId.Create(2), "  ", MedicalConditionCategory.Medication));
    }

    [Fact]
    public void Rename_TrimsAndStoresTheNewName()
    {
        var type = MedicalConditionType.Create(
            MedicalConditionTypeId.Create(2), "هل يؤخذ علاج للسكر", MedicalConditionCategory.Medication);

        type.Rename("  علاج السكر  ");

        Assert.Equal("علاج السكر", type.Name);
    }

    [Fact]
    public void Rename_BlankName_Throws()
    {
        var type = MedicalConditionType.Create(
            MedicalConditionTypeId.Create(2), "هل يؤخذ علاج للسكر", MedicalConditionCategory.Medication);

        Assert.Throws<ArgumentException>(() => type.Rename("   "));
    }

    [Fact]
    public void Rename_DoesNotChangeTheCategory()
    {
        var type = MedicalConditionType.Create(
            MedicalConditionTypeId.Create(1), "حمل", MedicalConditionCategory.Pregnancy);

        type.Rename("حمل (تجريبي)");

        Assert.Equal(MedicalConditionCategory.Pregnancy, type.Category);
    }
}
