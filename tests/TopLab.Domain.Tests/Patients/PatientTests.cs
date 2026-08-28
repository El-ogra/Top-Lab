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
        p.AssignLabId("LAB-123");
        Assert.Equal("LAB-123", p.LabId);
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
}
