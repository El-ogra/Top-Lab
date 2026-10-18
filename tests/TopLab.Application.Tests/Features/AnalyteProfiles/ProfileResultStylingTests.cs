using FluentValidation.TestHelper;
using TopLab.Application.Features.AnalyteProfiles.Commands.CreateProfile;
using TopLab.Application.Features.AnalyteProfiles.Queries.GetProfileDefinitions;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.AnalyteProfiles;

/// <summary>
/// GAP-017: the four per-profile result-styling fields must travel from the command through
/// the handler into the aggregate, be length-validated, tolerate null, and come back out of the
/// profile-definitions query so the editor can load them.
/// </summary>
public class ProfileResultStylingTests
{
    private const int TestIdValue = 50;
    private const int ProfileIdValue = 60;

    private static readonly CreateProfileCommandValidator Validator = new();

    private static Test MakeSpecializedTest() =>
        Test.Create(TestId.Create(TestIdValue), "T50", "T50", "T50", "T50", 60, 500m, ResultKind.SpecializedProfile);

    private static Profile SeedProfile(FakeApplicationDbContext db)
    {
        var profile = Profile.Create(ProfileId.Create(ProfileIdValue), "Liver", TestId.Create(TestIdValue), ResultKind.SpecializedProfile, 300m);
        db.Profiles.Add(profile);
        return profile;
    }

    private static CreateProfileCommand Command(
        string? normalFont = null,
        string? normalColor = null,
        string? abnormalFont = null,
        string? abnormalBackgroundColor = null)
        => new("Liver", TestIdValue, 300m, new[] { 1 }, normalFont, normalColor, abnormalFont, abnormalBackgroundColor);

    // ---- handler: command values are carried to persistence ----

    [Fact]
    public async Task CreateProfile_WithStyling_PersistsAllFourValues()
    {
        var db = new FakeApplicationDbContext();
        db.Tests.Add(MakeSpecializedTest());
        var analyte = Analyte.Create(AnalyteId.Create(1), "A1", "A1");
        db.Analytes.Add(analyte);
        db.AnalyteReferenceRanges.Add(AnalyteReferenceRange.Create(AnalyteReferenceRangeId.Create(1), analyte.Id));

        var handler = new CreateProfileCommandHandler(db);
        var result = await handler.Handle(
            Command("#00FF00", "#EEEEEE", "#FF0000", "#D9D9D9"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var profile = Assert.Single(db.Profiles);
        Assert.Equal("#00FF00", profile.NormalFont);
        Assert.Equal("#EEEEEE", profile.NormalColor);
        Assert.Equal("#FF0000", profile.AbnormalFont);
        Assert.Equal("#D9D9D9", profile.AbnormalBackgroundColor);
    }

    [Fact]
    public async Task CreateProfile_WithoutStyling_StoresNulls()
    {
        var db = new FakeApplicationDbContext();
        db.Tests.Add(MakeSpecializedTest());
        var analyte = Analyte.Create(AnalyteId.Create(1), "A1", "A1");
        db.Analytes.Add(analyte);
        db.AnalyteReferenceRanges.Add(AnalyteReferenceRange.Create(AnalyteReferenceRangeId.Create(1), analyte.Id));

        var handler = new CreateProfileCommandHandler(db);
        var result = await handler.Handle(Command(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var profile = Assert.Single(db.Profiles);
        Assert.Null(profile.NormalFont);
        Assert.Null(profile.NormalColor);
        Assert.Null(profile.AbnormalFont);
        Assert.Null(profile.AbnormalBackgroundColor);
    }

    [Theory]
    [InlineData(null, null, null, null)]
    [InlineData("", "", "", "")]
    [InlineData("   ", "   ", "   ", "   ")]
    public async Task CreateProfile_BlankStyling_AcceptedAndStoredAsNull(
        string? normalFont, string? normalColor, string? abnormalFont, string? abnormalBackgroundColor)
    {
        var db = new FakeApplicationDbContext();
        db.Tests.Add(MakeSpecializedTest());
        var analyte = Analyte.Create(AnalyteId.Create(1), "A1", "A1");
        db.Analytes.Add(analyte);
        db.AnalyteReferenceRanges.Add(AnalyteReferenceRange.Create(AnalyteReferenceRangeId.Create(1), analyte.Id));

        var handler = new CreateProfileCommandHandler(db);
        var result = await handler.Handle(
            Command(normalFont, normalColor, abnormalFont, abnormalBackgroundColor),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var profile = Assert.Single(db.Profiles);
        Assert.Null(profile.NormalFont);
        Assert.Null(profile.AbnormalBackgroundColor);
    }

    [Fact]
    public async Task CreateProfile_OverLengthFont_IsRejectedByTheHandlerGuard()
    {
        var db = new FakeApplicationDbContext();
        db.Tests.Add(MakeSpecializedTest());
        var analyte = Analyte.Create(AnalyteId.Create(1), "A1", "A1");
        db.Analytes.Add(analyte);
        db.AnalyteReferenceRanges.Add(AnalyteReferenceRange.Create(AnalyteReferenceRangeId.Create(1), analyte.Id));

        var handler = new CreateProfileCommandHandler(db);
        var result = await handler.Handle(
            Command(new string('x', Profile.MaxResultFontLength + 1), null, null, null),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Empty(db.Profiles);
    }

    [Fact]
    public async Task CreateProfile_OverLengthColour_IsRejectedByTheHandlerGuard()
    {
        var db = new FakeApplicationDbContext();
        db.Tests.Add(MakeSpecializedTest());
        var analyte = Analyte.Create(AnalyteId.Create(1), "A1", "A1");
        db.Analytes.Add(analyte);
        db.AnalyteReferenceRanges.Add(AnalyteReferenceRange.Create(AnalyteReferenceRangeId.Create(1), analyte.Id));

        var handler = new CreateProfileCommandHandler(db);
        var result = await handler.Handle(
            Command(null, null, null, new string('x', Profile.MaxResultColorLength + 1)),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Empty(db.Profiles);
    }

    // ---- validator ----

    [Fact]
    public void Validator_FontAtMaxLength_Passes()
    {
        var result = Validator.TestValidate(Command(new string('x', Profile.MaxResultFontLength), null, new string('x', Profile.MaxResultFontLength), null));
        result.ShouldNotHaveValidationErrorFor(x => x.NormalFont);
        result.ShouldNotHaveValidationErrorFor(x => x.AbnormalFont);
    }

    [Fact]
    public void Validator_FontOverMaxLength_Fails()
    {
        var tooLong = new string('x', Profile.MaxResultFontLength + 1);

        Validator.TestValidate(Command(tooLong, null, null, null))
            .ShouldHaveValidationErrorFor(x => x.NormalFont);
        Validator.TestValidate(Command(null, null, tooLong, null))
            .ShouldHaveValidationErrorFor(x => x.AbnormalFont);
    }

    [Fact]
    public void Validator_ColourAtMaxLength_Passes()
    {
        var result = Validator.TestValidate(Command(null, new string('x', Profile.MaxResultColorLength), null, new string('x', Profile.MaxResultColorLength)));
        result.ShouldNotHaveValidationErrorFor(x => x.NormalColor);
        result.ShouldNotHaveValidationErrorFor(x => x.AbnormalBackgroundColor);
    }

    [Fact]
    public void Validator_ColourOverMaxLength_Fails()
    {
        var tooLong = new string('x', Profile.MaxResultColorLength + 1);

        Validator.TestValidate(Command(null, tooLong, null, null))
            .ShouldHaveValidationErrorFor(x => x.NormalColor);
        Validator.TestValidate(Command(null, null, null, tooLong))
            .ShouldHaveValidationErrorFor(x => x.AbnormalBackgroundColor);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validator_BlankStyling_Passes(string? blank)
    {
        var result = Validator.TestValidate(Command(blank, blank, blank, blank));
        result.ShouldNotHaveValidationErrorFor(x => x.NormalFont);
        result.ShouldNotHaveValidationErrorFor(x => x.NormalColor);
        result.ShouldNotHaveValidationErrorFor(x => x.AbnormalFont);
        result.ShouldNotHaveValidationErrorFor(x => x.AbnormalBackgroundColor);
    }

    // ---- query ----

    [Fact]
    public async Task GetProfileDefinitions_ReturnsTheFourStylingValues()
    {
        var db = new FakeApplicationDbContext();
        db.Tests.Add(MakeSpecializedTest());
        var profile = SeedProfile(db);
        profile.SetResultStyling("#00FF00", "#EEEEEE", "#FF0000", "#D9D9D9");

        var handler = new GetProfileDefinitionsQueryHandler(db);
        var result = await handler.Handle(new GetProfileDefinitionsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var dto = Assert.Single(result.Value!);
        Assert.Equal("#00FF00", dto.NormalFont);
        Assert.Equal("#EEEEEE", dto.NormalColor);
        Assert.Equal("#FF0000", dto.AbnormalFont);
        Assert.Equal("#D9D9D9", dto.AbnormalBackgroundColor);
    }

    [Fact]
    public async Task GetProfileDefinitions_WithoutStyling_ReturnsNulls()
    {
        var db = new FakeApplicationDbContext();
        db.Tests.Add(MakeSpecializedTest());
        SeedProfile(db);

        var handler = new GetProfileDefinitionsQueryHandler(db);
        var result = await handler.Handle(new GetProfileDefinitionsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var dto = Assert.Single(result.Value!);
        Assert.Null(dto.NormalFont);
        Assert.Null(dto.NormalColor);
        Assert.Null(dto.AbnormalFont);
        Assert.Null(dto.AbnormalBackgroundColor);
    }
}