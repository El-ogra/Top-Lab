using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Domain.Tests.Tests;

public class ProfileTests
{
    private static Test CreateSpecialisedTest(TestId id, decimal patientPrice = 200m, string code = "T-PROF")
    {
        return Test.Create(id, "Profile Test", "Profile Test", "Profile Test", code, 30, patientPrice, ResultKind.SpecializedProfile);
    }

    [Fact]
    public void Create_RejectsNonSpecialisedTest()
    {
        var simple = Test.Create(TestId.Create(10), "Simple", "Simple", "Simple", "T10", 30, 50m, ResultKind.Simple);

        Assert.Throws<ArgumentException>(() => Profile.Create(
            ProfileId.Create(1),
            "Profile",
            simple.Id,
            simple.ResultKind,
            100m));
    }

    [Fact]
    public void Create_RejectsNegativeFixedPrice()
    {
        var test = CreateSpecialisedTest(TestId.Create(10));

        Assert.Throws<ArgumentException>(() => Profile.Create(
            ProfileId.Create(1),
            "Profile",
            test.Id,
            test.ResultKind,
            -1m));
    }

    [Fact]
    public void FixedPrice_IsImmutable_AfterCreation()
    {
        var test = CreateSpecialisedTest(TestId.Create(10));
        var profile = Profile.Create(ProfileId.Create(1), "Profile", test.Id, test.ResultKind, 200m);

        Assert.Equal(200m, profile.FixedPrice);

        var setter = typeof(Profile).GetProperty(nameof(Profile.FixedPrice))!.SetMethod;
        Assert.True(setter is null || !setter.IsPublic);

        Assert.Throws<ArgumentException>(() => profile.UpdateName(string.Empty));
        Assert.Equal(200m, profile.FixedPrice);
    }

    [Fact]
    public void AddAnalyte_RejectsDuplicateLink()
    {
        var test = CreateSpecialisedTest(TestId.Create(10));
        var profile = Profile.Create(ProfileId.Create(1), "Profile", test.Id, test.ResultKind, 200m);

        profile.AddAnalyte(ProfileAnalyte.Create(ProfileAnalyteId.Create(1), profile.Id, AnalyteId.Create(1)));

        Assert.Throws<InvalidOperationException>(() => profile.AddAnalyte(ProfileAnalyte.Create(ProfileAnalyteId.Create(2), profile.Id, AnalyteId.Create(1))));
    }

    [Fact]
    public void RemoveAnalyte_RemovesExistingLink_Only()
    {
        var test = CreateSpecialisedTest(TestId.Create(10));
        var profile = Profile.Create(ProfileId.Create(1), "Profile", test.Id, test.ResultKind, 200m);

        profile.AddAnalyte(ProfileAnalyte.Create(ProfileAnalyteId.Create(1), profile.Id, AnalyteId.Create(1)));

        Assert.Throws<InvalidOperationException>(() => profile.RemoveAnalyte(AnalyteId.Create(99)));
        profile.RemoveAnalyte(AnalyteId.Create(1));
        Assert.Empty(profile.Analytes);
    }

    [Fact]
    public void DeactivateReactivate_TogglesActive()
    {
        var test = CreateSpecialisedTest(TestId.Create(10));
        var profile = Profile.Create(ProfileId.Create(1), "Profile", test.Id, test.ResultKind, 200m);

        profile.Deactivate();
        Assert.False(profile.IsActive);
        profile.Reactivate();
        Assert.True(profile.IsActive);
    }
}