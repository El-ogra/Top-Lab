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

    // ---- GAP-017: per-profile normal/abnormal result styling ----

    [Fact]
    public void NewProfile_ResultStyling_DefaultsToNull()
    {
        var test = CreateSpecialisedTest(TestId.Create(10));
        var profile = Profile.Create(ProfileId.Create(1), "Profile", test.Id, test.ResultKind, 200m);

        Assert.Null(profile.NormalFont);
        Assert.Null(profile.NormalColor);
        Assert.Null(profile.AbnormalFont);
        Assert.Null(profile.AbnormalBackgroundColor);
    }

    [Fact]
    public void SetResultStyling_StoresAllFourValues()
    {
        var test = CreateSpecialisedTest(TestId.Create(10));
        var profile = Profile.Create(ProfileId.Create(1), "Profile", test.Id, test.ResultKind, 200m);

        profile.SetResultStyling("#00FF00", "#EEEEEE", "#FF0000", "#D9D9D9");

        Assert.Equal("#00FF00", profile.NormalFont);
        Assert.Equal("#EEEEEE", profile.NormalColor);
        Assert.Equal("#FF0000", profile.AbnormalFont);
        Assert.Equal("#D9D9D9", profile.AbnormalBackgroundColor);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void SetResultStyling_Blank_ClearsToNull(string? blank)
    {
        var test = CreateSpecialisedTest(TestId.Create(10));
        var profile = Profile.Create(ProfileId.Create(1), "Profile", test.Id, test.ResultKind, 200m);

        profile.SetResultStyling("#00FF00", "#EEEEEE", "#FF0000", "#D9D9D9");
        profile.SetResultStyling(blank, blank, blank, blank);

        Assert.Null(profile.NormalFont);
        Assert.Null(profile.NormalColor);
        Assert.Null(profile.AbnormalFont);
        Assert.Null(profile.AbnormalBackgroundColor);
    }

    [Fact]
    public void SetResultStyling_TrimsSurroundingWhitespace()
    {
        var test = CreateSpecialisedTest(TestId.Create(10));
        var profile = Profile.Create(ProfileId.Create(1), "Profile", test.Id, test.ResultKind, 200m);

        profile.SetResultStyling("  #00FF00  ", null, null, null);

        Assert.Equal("#00FF00", profile.NormalFont);
    }

    [Fact]
    public void SetResultStyling_AcceptsFontAtMaxLength()
    {
        var test = CreateSpecialisedTest(TestId.Create(10));
        var profile = Profile.Create(ProfileId.Create(1), "Profile", test.Id, test.ResultKind, 200m);
        var atLimit = new string('x', Profile.MaxResultFontLength);

        profile.SetResultStyling(atLimit, null, atLimit, null);

        Assert.Equal(atLimit, profile.NormalFont);
        Assert.Equal(atLimit, profile.AbnormalFont);
    }

    [Fact]
    public void SetResultStyling_AcceptsColourAtMaxLength()
    {
        var test = CreateSpecialisedTest(TestId.Create(10));
        var profile = Profile.Create(ProfileId.Create(1), "Profile", test.Id, test.ResultKind, 200m);
        var atLimit = new string('x', Profile.MaxResultColorLength);

        profile.SetResultStyling(null, atLimit, null, atLimit);

        Assert.Equal(atLimit, profile.NormalColor);
        Assert.Equal(atLimit, profile.AbnormalBackgroundColor);
    }

    [Fact]
    public void SetResultStyling_RejectsOverLengthFont()
    {
        var test = CreateSpecialisedTest(TestId.Create(10));
        var profile = Profile.Create(ProfileId.Create(1), "Profile", test.Id, test.ResultKind, 200m);
        var tooLong = new string('x', Profile.MaxResultFontLength + 1);

        Assert.Throws<ArgumentException>(() => profile.SetResultStyling(tooLong, null, null, null));
        Assert.Throws<ArgumentException>(() => profile.SetResultStyling(null, null, tooLong, null));
    }

    [Fact]
    public void SetResultStyling_RejectsOverLengthColour()
    {
        var test = CreateSpecialisedTest(TestId.Create(10));
        var profile = Profile.Create(ProfileId.Create(1), "Profile", test.Id, test.ResultKind, 200m);
        var tooLong = new string('x', Profile.MaxResultColorLength + 1);

        Assert.Throws<ArgumentException>(() => profile.SetResultStyling(null, tooLong, null, null));
        Assert.Throws<ArgumentException>(() => profile.SetResultStyling(null, null, null, tooLong));
    }

    [Fact]
    public void ResultStyling_SettersAreNotPubliclyWritable()
    {
        foreach (var name in new[]
                 {
                     nameof(Profile.NormalFont),
                     nameof(Profile.NormalColor),
                     nameof(Profile.AbnormalFont),
                     nameof(Profile.AbnormalBackgroundColor)
                 })
        {
            var setter = typeof(Profile).GetProperty(name)!.SetMethod;
            Assert.True(setter is null || !setter.IsPublic, name);
        }
    }
}