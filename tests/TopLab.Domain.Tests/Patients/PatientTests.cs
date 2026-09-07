using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Domain.Tests.Patients;

public class PatientTests
{
    [Fact]
    public void Create_Valid_Succeeds()
    {
        var p = Patient.Create(PatientId.Create(1), "Ahmed Mohamed", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);
        Assert.Equal("Ahmed Mohamed", p.FullName);
        Assert.Equal(30, p.AgeValue);
    }

    [Fact]
    public void Create_MissingFullName_Throws()
    {
        Assert.Throws<ArgumentException>(() => Patient.Create(PatientId.Create(1), "", Sex.Male, 20, AgeUnit.Year, DateTime.UtcNow));
        Assert.Throws<ArgumentException>(() => Patient.Create(PatientId.Create(1), "  ", Sex.Male, 20, AgeUnit.Year, DateTime.UtcNow));
    }

    [Fact]
    public void Create_NegativeAge_Throws()
    {
        Assert.Throws<ArgumentException>(() => Patient.Create(PatientId.Create(1), "Ali", Sex.Male, -1, AgeUnit.Year, DateTime.UtcNow));
    }

    [Fact]
    public void Create_FastingHours_WithoutIndication_Throws()
    {
        Assert.Throws<ArgumentException>(() => Patient.Create(PatientId.Create(1), "Ali", Sex.Male, 20, AgeUnit.Year, DateTime.UtcNow, isFastingIndicated: false, fastingHours: 8));
    }

    [Fact]
    public void AssignLabId_Valid_Sets()
    {
        var p = Patient.Create(PatientId.Create(1), "Ali", Sex.Female, 25, AgeUnit.Year, DateTime.UtcNow);
        p.AssignLabId(LabId.Create("LAB-123"));
        Assert.Equal("LAB-123", p.LabId!.Value);
    }

    [Fact]
    public void PatientPhoneNumber_Create_Valid()
    {
        var pn = PatientPhoneNumber.Create(PatientPhoneNumberId.Create(1), PatientId.Create(10), "01012345678", 0);
        Assert.Equal("01012345678", pn.PhoneNumber);
    }

    [Fact]
    public void PatientPhoneNumber_Empty_Throws()
    {
        Assert.Throws<ArgumentException>(() => PatientPhoneNumber.Create(PatientPhoneNumberId.Create(1), PatientId.Create(10), "", 0));
    }

    [Fact]
    public void ReferenceRange_Matches_AgeUnitSensitive()
    {
        var rr = ReferenceRange.Create(ReferenceRangeId.Create(1), TestId.Create(1), AgeUnit.Day, 1, 60, 1.0m, 10.0m, Sex.Male);
        Assert.True(rr.Matches(Sex.Male, AgeUnit.Day, 15));
        Assert.True(rr.Matches(Sex.Male, AgeUnit.Day, 35));
        Assert.False(rr.Matches(Sex.Male, AgeUnit.Month, 1));
        Assert.False(rr.Matches(Sex.Female, AgeUnit.Day, 15));
    }

    [Fact]
    public void ReferenceRange_MinGreaterThanMax_Throws()
    {
        Assert.Throws<ArgumentException>(() => ReferenceRange.Create(ReferenceRangeId.Create(1), TestId.Create(1), AgeUnit.Year, 10, 5, 1.0m, 10.0m));
        Assert.Throws<ArgumentException>(() => ReferenceRange.Create(ReferenceRangeId.Create(1), TestId.Create(1), AgeUnit.Year, 5, 10, 20.0m, 10.0m));
    }

    [Fact]
    public void StrongIds_NotInterchangeable()
    {
        var pid = PatientId.Create(1);
        var tid = TestId.Create(1);
        Assert.IsType<PatientId>(pid);
        Assert.IsType<TestId>(tid);
        Assert.False(pid.Equals(tid));
        Assert.Equal(1, pid.Value);
        Assert.Equal(1, tid.Value);
    }

    [Fact]
    public void SetPhoneNumbers_Replaces_NotAppends()
    {
        var p = Patient.Create(PatientId.Create(1), "Ali", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);
        p.SetPhoneNumbers(new[]
        {
            new PatientNumberInput("01012345678", 0),
            new PatientNumberInput("01098765432", 1)
        });
        Assert.Equal(2, p.PhoneNumbers.Count);

        p.SetPhoneNumbers(new[]
        {
            new PatientNumberInput("01111111111", 0)
        });
        Assert.Single(p.PhoneNumbers);
        Assert.Equal("01111111111", p.PhoneNumbers.First().PhoneNumber);
    }

    [Fact]
    public void SetPhoneNumbers_Trims_And_Skips_Empty()
    {
        var p = Patient.Create(PatientId.Create(1), "Ali", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);
        p.SetPhoneNumbers(new[]
        {
            new PatientNumberInput("  01012345678  ", 0),
            new PatientNumberInput("", 1),
            new PatientNumberInput("  ", 2),
            new PatientNumberInput("01098765432", 3)
        });
        Assert.Equal(2, p.PhoneNumbers.Count);
        Assert.Equal("01012345678", p.PhoneNumbers.ElementAt(0).PhoneNumber);
        Assert.Equal("01098765432", p.PhoneNumbers.ElementAt(1).PhoneNumber);
    }

    [Fact]
    public void SetPhoneNumbers_Preserves_SortOrder()
    {
        var p = Patient.Create(PatientId.Create(1), "Ali", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);
        p.SetPhoneNumbers(new[]
        {
            new PatientNumberInput("01012345678", 5),
            new PatientNumberInput("01098765432", 2)
        });
        Assert.Equal(5, p.PhoneNumbers.ElementAt(0).SortOrder);
        Assert.Equal(2, p.PhoneNumbers.ElementAt(1).SortOrder);
    }

    [Fact]
    public void SetPhoneNumbers_Empty_ClearsList()
    {
        var p = Patient.Create(PatientId.Create(1), "Ali", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);
        p.SetPhoneNumbers(new[]
        {
            new PatientNumberInput("01012345678", 0)
        });
        Assert.Single(p.PhoneNumbers);

        p.SetPhoneNumbers(Array.Empty<PatientNumberInput>());
        Assert.Empty(p.PhoneNumbers);
    }

    [Fact]
    public void AddMedicalCondition_Adds()
    {
        var p = Patient.Create(PatientId.Create(1), "Ali", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);
        p.AddMedicalCondition(MedicalConditionTypeId.Create(5));
        Assert.Single(p.MedicalConditions);
        Assert.Equal(5, p.MedicalConditions.First().MedicalConditionTypeId.Value);
    }

    [Fact]
    public void AddMedicalCondition_Duplicate_Idempotent()
    {
        var p = Patient.Create(PatientId.Create(1), "Ali", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);
        p.AddMedicalCondition(MedicalConditionTypeId.Create(5));
        p.AddMedicalCondition(MedicalConditionTypeId.Create(5));
        Assert.Single(p.MedicalConditions);
    }

    [Fact]
    public void RemoveMedicalCondition_Removes()
    {
        var p = Patient.Create(PatientId.Create(1), "Ali", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);
        p.AddMedicalCondition(MedicalConditionTypeId.Create(5));
        p.RemoveMedicalCondition(MedicalConditionTypeId.Create(5));
        Assert.Empty(p.MedicalConditions);
    }

    [Fact]
    public void RemoveMedicalCondition_Missing_NoOp()
    {
        var p = Patient.Create(PatientId.Create(1), "Ali", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);
        p.RemoveMedicalCondition(MedicalConditionTypeId.Create(5));
        Assert.Empty(p.MedicalConditions);
    }

    [Fact]
    public void SoftDelete_SetsFlag_And_Blocks_Update()
    {
        var p = Patient.Create(PatientId.Create(1), "Ali", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);
        p.SoftDelete();
        Assert.True(p.IsDeleted);

        Assert.Throws<InvalidOperationException>(() => p.Update(
            "Sara", Sex.Female, 25, AgeUnit.Year, null, null, null, false, AccountType.Individual,
            null, false, null, false));
    }

    [Fact]
    public void SoftDelete_Idempotent()
    {
        var p = Patient.Create(PatientId.Create(1), "Ali", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);
        p.SoftDelete();
        p.SoftDelete();
        Assert.True(p.IsDeleted);
    }

    [Fact]
    public void SoftDelete_Blocks_AssignLabId()
    {
        var p = Patient.Create(PatientId.Create(1), "Ali", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);
        p.SoftDelete();
        Assert.Throws<InvalidOperationException>(() => p.AssignLabId(LabId.Create("LAB-1")));
    }

    [Fact]
    public void Restore_Reenables_Mutation()
    {
        var p = Patient.Create(PatientId.Create(1), "Ali", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);
        p.SoftDelete();
        p.Restore();
        Assert.False(p.IsDeleted);

        p.AssignLabId(LabId.Create("LAB-1"));
        Assert.Equal("LAB-1", p.LabId!.Value);
    }
}
