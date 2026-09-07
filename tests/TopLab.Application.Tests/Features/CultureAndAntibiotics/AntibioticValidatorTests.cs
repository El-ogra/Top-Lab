using FluentValidation.TestHelper;
using TopLab.Application.Features.CultureAndAntibiotics.Commands.AttachAntibioticToCulture;
using TopLab.Application.Features.CultureAndAntibiotics.Commands.CreateAntibiotic;
using TopLab.Application.Features.CultureAndAntibiotics.Commands.DeleteAntibiotic;
using TopLab.Application.Features.CultureAndAntibiotics.Commands.DetachAntibioticFromCulture;
using TopLab.Application.Features.CultureAndAntibiotics.Commands.UpdateAntibiotic;
using Xunit;

namespace TopLab.Application.Tests.Features.CultureAndAntibiotics;

public class AntibioticValidatorTests
{
    private readonly CreateAntibioticCommandValidator _create =
        new();

    private readonly UpdateAntibioticCommandValidator _update =
        new();

    private readonly DeleteAntibioticCommandValidator _delete =
        new();

    private readonly AttachAntibioticToCultureCommandValidator _attach =
        new();

    private readonly DetachAntibioticFromCultureCommandValidator _detach =
        new();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_NameMissing_Fails(string name)
    {
        var result = _create.TestValidate(new CreateAntibioticCommand(name, false, false));
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Create_NameTooLong_Fails()
    {
        var longName = new string('x', 151);
        var result = _create.TestValidate(new CreateAntibioticCommand(longName, false, false));
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Create_HappyPath_Succeeds()
    {
        var result = _create.TestValidate(new CreateAntibioticCommand("Amoxicillin", false, false));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Update_IdZero_Fails()
    {
        var result = _update.TestValidate(new UpdateAntibioticCommand(0, "X", false, false));
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_NameMissing_Fails(string name)
    {
        var result = _update.TestValidate(new UpdateAntibioticCommand(1, name, false, false));
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Delete_IdZero_Fails()
    {
        var result = _delete.TestValidate(new DeleteAntibioticCommand(0));
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    public void Attach_IdsGreaterThanZero_Required(int testId, int antibioticId)
    {
        var result = _attach.TestValidate(
            new AttachAntibioticToCultureCommand(testId, antibioticId));
        if (testId == 0) result.ShouldHaveValidationErrorFor(x => x.TestId);
        if (antibioticId == 0) result.ShouldHaveValidationErrorFor(x => x.AntibioticId);
    }

    [Fact]
    public void Attach_HappyPath_Succeeds()
    {
        var result = _attach.TestValidate(new AttachAntibioticToCultureCommand(1, 1));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    public void Detach_IdsGreaterThanZero_Required(int testId, int antibioticId)
    {
        var result = _detach.TestValidate(
            new DetachAntibioticFromCultureCommand(testId, antibioticId));
        if (testId == 0) result.ShouldHaveValidationErrorFor(x => x.TestId);
        if (antibioticId == 0) result.ShouldHaveValidationErrorFor(x => x.AntibioticId);
    }
}