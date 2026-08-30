using TopLab.Domain.Common;
using TopLab.Domain.Common.Ids;
using Xunit;

namespace TopLab.Domain.Tests.Common;

public class StronglyTypedIdTests
{
    [Fact]
    public void GivenTwoIdsWithSameValue_WhenCompared_ThenTheyAreEqual()
    {
        var a = new PatientId(1);
        var b = new PatientId(1);

        Assert.Equal(a, b);
        Assert.Equal(a.Value, b.Value);
        Assert.Equal("1", a.ToString());
    }

    [Fact]
    public void GivenTwoIdsOfDifferentConceptsWithSamePrimitiveValue_WhenCompared_ThenTheyAreDistinctTypes()
    {
        // PatientId(1) and TestId(1) are distinct types: a raw-int design could
        // swap them silently, but the strongly-typed wrappers cannot be confused.
        var patientId = new PatientId(1);
        var testId = new TestId(1);

        Assert.IsType<PatientId>(patientId);
        Assert.IsType<TestId>(testId);
        Assert.False(patientId.Equals(testId));
    }

    [Fact]
    public void GivenStringBackedId_WhenCompared_ThenStructuralEqualityHolds()
    {
        Assert.Equal(LabId.Create("LAB-001"), LabId.Create("LAB-001"));
        Assert.NotEqual(LabId.Create("LAB-001"), LabId.Create("LAB-002"));
    }

    [Fact]
    public void GivenNullValue_WhenConstructingId_ThenThrowsArgumentNullException()
    {
        Assert.Throws<System.ArgumentNullException>(() => LabId.Create(null!));
    }
}
