using TopLab.Application.Features.ExternalEntities.Commands.CreateExternalEntity;
using TopLab.Application.Features.ExternalEntities.Commands.DeleteExternalEntity;
using TopLab.Application.Features.ExternalEntities.Commands.GenerateEntityIdCode;
using TopLab.Application.Features.ExternalEntities.Commands.UpdateExternalEntity;
using TopLab.Domain.Common.Enums;
using Xunit;

namespace TopLab.Application.Tests.Features.ExternalEntities;

public class ExternalEntityCommandValidatorTests
{
    private static CreateExternalEntityCommand Create(EntityType type, string name,
        int? priceListId = null, decimal? percent = null) =>
        new(type, name, null, null, null, null, null, null, priceListId, percent);

    [Fact]
    public void Create_DoctorWithoutList_Passes()
    {
        var validator = new CreateExternalEntityCommandValidator();

        Assert.True(validator.Validate(Create(EntityType.TreatingDoctor, "Dr")).IsValid);
    }

    [Fact]
    public void Create_DoctorWithList_Fails()
    {
        var validator = new CreateExternalEntityCommandValidator();

        var result = validator.Validate(Create(EntityType.TreatingDoctor, "Dr", priceListId: 1));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "الطبيب المعالج لا يرتبط بقائمة أسعار.");
    }

    [Fact]
    public void Create_ReferralWithoutList_Fails()
    {
        var validator = new CreateExternalEntityCommandValidator();

        var result = validator.Validate(Create(EntityType.ReferralOrContract, "Delta"));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "جهة الإحالة / التعاقد تتطلب قائمة أسعار.");
    }

    [Fact]
    public void Create_ReferralWithList_Passes()
    {
        var validator = new CreateExternalEntityCommandValidator();

        Assert.True(validator.Validate(Create(EntityType.ReferralOrContract, "Delta", priceListId: 1)).IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyName_Fails(string name)
    {
        var validator = new CreateExternalEntityCommandValidator();

        var result = validator.Validate(Create(EntityType.TreatingDoctor, name));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "اسم الجهة الخارجية مطلوب.");
    }

    [Fact]
    public void Create_NameTooLong_Fails()
    {
        var validator = new CreateExternalEntityCommandValidator();

        var result = validator.Validate(Create(EntityType.TreatingDoctor, new string('N', 201)));

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Create_PercentOutOfRange_Fails(decimal percent)
    {
        var validator = new CreateExternalEntityCommandValidator();

        var result = validator.Validate(Create(EntityType.TreatingDoctor, "Dr", percent: percent));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "نسبة الخصم / العمولة يجب أن تكون بين 0 و 100.");
    }

    [Fact]
    public void Create_UnknownEntityType_Fails()
    {
        var validator = new CreateExternalEntityCommandValidator();

        var result = validator.Validate(Create((EntityType)99, "Dr"));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Update_ReferralWithoutList_Fails()
    {
        var validator = new UpdateExternalEntityCommandValidator();

        var result = validator.Validate(new UpdateExternalEntityCommand(
            1, EntityType.ReferralOrContract, "Delta", null, null, null, null, null, null, null, null));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "جهة الإحالة / التعاقد تتطلب قائمة أسعار.");
    }

    [Fact]
    public void Update_InvalidId_Fails()
    {
        var validator = new UpdateExternalEntityCommandValidator();

        var result = validator.Validate(new UpdateExternalEntityCommand(
            0, EntityType.TreatingDoctor, "Dr", null, null, null, null, null, null, null, null));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Delete_InvalidId_Fails()
    {
        var validator = new DeleteExternalEntityCommandValidator();

        Assert.False(validator.Validate(new DeleteExternalEntityCommand(0)).IsValid);
        Assert.True(validator.Validate(new DeleteExternalEntityCommand(1)).IsValid);
    }

    [Fact]
    public void Generate_InvalidId_Fails()
    {
        var validator = new GenerateEntityIdCodeCommandValidator();

        Assert.False(validator.Validate(new GenerateEntityIdCodeCommand(-3)).IsValid);
        Assert.True(validator.Validate(new GenerateEntityIdCodeCommand(1)).IsValid);
    }
}
