using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Results;
using Xunit;

namespace TopLab.Domain.Tests.Results;

public class ProfileResultAmendmentTests
{
    private static ProfileResultAmendment CreateAmendment()
    {
        return ProfileResultAmendment.Create(
            ProfileResultAmendmentId.Create(1),
            ProfileResultItemId.Create(1),
            7,
            new DateTime(2026, 9, 9, 12, 0, 0, DateTimeKind.Utc),
            "5.5",
            "mg/dL",
            null,
            "6.0",
            "mg/dL",
            ProfileResultFlag.High,
            "repeat after fasting");
    }

    [Fact]
    public void Create_RecordsOldAndNewValues()
    {
        var amendment = CreateAmendment();

        Assert.Equal("5.5", amendment.OldResultValue);
        Assert.Equal("6.0", amendment.NewResultValue);
        Assert.Equal("mg/dL", amendment.OldUnit);
        Assert.Equal("mg/dL", amendment.NewUnit);
        Assert.Null(amendment.OldFlag);
        Assert.Equal(ProfileResultFlag.High, amendment.NewFlag);
        Assert.Equal(7, amendment.AmendedByUserId);
        Assert.Equal("repeat after fasting", amendment.Reason);
    }

    [Fact]
    public void Create_Guards_InvalidInputs()
    {
        Assert.Throws<ArgumentException>(() => ProfileResultAmendment.Create(
            ProfileResultAmendmentId.Create(1),
            ProfileResultItemId.Create(1),
            0,
            DateTime.UtcNow,
            "5.5",
            null,
            null,
            "6.0",
            null,
            null,
            null));

        Assert.Throws<ArgumentException>(() => ProfileResultAmendment.Create(
            ProfileResultAmendmentId.Create(1),
            ProfileResultItemId.Create(1),
            7,
            DateTime.UtcNow,
            " ",
            null,
            null,
            "6.0",
            null,
            null,
            null));
    }

    [Fact]
    public void AuditRecord_IsImmutable_ByConstruction()
    {
        var amendment = CreateAmendment();

        // Creation-only aggregate: no public mutators, no public setters, no navigation
        // back to the live item. A caller cannot alter the recorded event after the fact.
        Assert.DoesNotContain(typeof(ProfileResultAmendment).GetMethods(), m => m.Name.StartsWith("Update") || m.Name.StartsWith("Void") || m.Name.StartsWith("Amend"));
        Assert.All(
            typeof(ProfileResultAmendment).GetProperties(),
            p => Assert.True(p.SetMethod is null || !p.SetMethod.IsPublic, $"{p.Name} must not expose a public setter"));
    }
}