using TopLab.Domain.Common;
using Xunit;

namespace TopLab.Domain.Tests.Common;

public sealed class AuditableSample : AuditableEntity<PatientId>
{
    public AuditableSample(PatientId id)
        : base(id)
    {
    }
}

public class AuditableEntityTests
{
    [Fact]
    public void GivenNewAuditableEntity_WhenInspected_ThenAuditFieldsHaveTypeDefaults()
    {
        var entity = new AuditableSample(new PatientId(7));

        Assert.Equal(7, entity.Id.Value);
        Assert.Equal(0, entity.CreatedByUserId);
        Assert.Equal(0, entity.LastModifiedByUserId);
        Assert.Equal(0, entity.ModificationCount);
        Assert.Equal(default(System.DateTime), entity.CreatedAtUtc);
        Assert.Equal(default(System.DateTime), entity.LastModifiedAtUtc);
    }

    [Fact]
    public void GivenTwoAuditableEntitiesWithSameId_WhenCompared_ThenTheyAreEqual()
    {
        var a = new AuditableSample(new PatientId(7));
        var b = new AuditableSample(new PatientId(7));

        Assert.Equal(a, b);
    }
}
