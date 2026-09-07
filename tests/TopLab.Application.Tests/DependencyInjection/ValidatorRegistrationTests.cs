using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TopLab.Application.Features.CultureAndAntibiotics.Commands.AttachAntibioticToCulture;
using TopLab.Application.Features.CultureAndAntibiotics.Commands.CreateAntibiotic;
using TopLab.Application.Features.CultureAndAntibiotics.Commands.DeleteAntibiotic;
using TopLab.Application.Features.CultureAndAntibiotics.Commands.DetachAntibioticFromCulture;
using TopLab.Application.Features.CultureAndAntibiotics.Commands.UpdateAntibiotic;
using TopLab.Application.Features.ExternalEntities.Commands.CreateExternalEntity;
using TopLab.Application.Features.ExternalEntities.Commands.DeleteExternalEntity;
using TopLab.Application.Features.ExternalEntities.Commands.GenerateEntityIdCode;
using TopLab.Application.Features.ExternalEntities.Commands.UpdateExternalEntity;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.CreateCustomGroup;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.CreatePriceList;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.CreateTestComment;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.DeleteCustomGroup;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.DeletePriceList;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.DeleteTestComment;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.RemoveCustomGroupItem;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.RemovePriceListItem;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.RenameCustomGroup;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.RenamePriceList;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.SetCustomGroupItemPrice;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.SetPriceListItemPrice;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.UpdateTestComment;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.CreateTest;

namespace TopLab.Application.Tests.DependencyInjection;

public class ValidatorRegistrationTests
{
    [Fact]
    public void HostBuiltLikeApp_ResolvesCreateTestValidator()
    {
        var services = new ServiceCollection();
        services.AddApplication();
        using var provider = services.BuildServiceProvider();

        var validator = provider.GetService<IValidator<CreateTestCommand>>();

        Assert.NotNull(validator);
    }

    [Fact]
    public void HostBuiltLikeApp_RegisteredValidator_ActuallyValidates()
    {
        var services = new ServiceCollection();
        services.AddApplication();
        using var provider = services.BuildServiceProvider();

        var validator = provider.GetRequiredService<IValidator<CreateTestCommand>>();
        var command = new CreateTestCommand("", "", "", "", 0, -5m, default, false, null, null, false, null, null);

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(typeof(IValidator<CreateExternalEntityCommand>))]
    [InlineData(typeof(IValidator<UpdateExternalEntityCommand>))]
    [InlineData(typeof(IValidator<DeleteExternalEntityCommand>))]
    [InlineData(typeof(IValidator<GenerateEntityIdCodeCommand>))]
    public void HostBuiltLikeApp_ResolvesExternalEntityValidators(System.Type validatorType)
    {
        var services = new ServiceCollection();
        services.AddApplication();
        using var provider = services.BuildServiceProvider();

        var validator = provider.GetService(validatorType);

        Assert.NotNull(validator);
    }

    [Theory]
    [InlineData(typeof(IValidator<CreatePriceListCommand>))]
    [InlineData(typeof(IValidator<RenamePriceListCommand>))]
    [InlineData(typeof(IValidator<DeletePriceListCommand>))]
    [InlineData(typeof(IValidator<SetPriceListItemPriceCommand>))]
    [InlineData(typeof(IValidator<RemovePriceListItemCommand>))]
    [InlineData(typeof(IValidator<CreateTestCommentCommand>))]
    [InlineData(typeof(IValidator<UpdateTestCommentCommand>))]
    [InlineData(typeof(IValidator<DeleteTestCommentCommand>))]
    [InlineData(typeof(IValidator<CreateCustomGroupCommand>))]
    [InlineData(typeof(IValidator<RenameCustomGroupCommand>))]
    [InlineData(typeof(IValidator<DeleteCustomGroupCommand>))]
    [InlineData(typeof(IValidator<SetCustomGroupItemPriceCommand>))]
    [InlineData(typeof(IValidator<RemoveCustomGroupItemCommand>))]
    public void HostBuiltLikeApp_ResolvesM13Validators(System.Type validatorType)
    {
        var services = new ServiceCollection();
        services.AddApplication();
        using var provider = services.BuildServiceProvider();

        var validator = provider.GetService(validatorType);

        Assert.NotNull(validator);
    }

    [Theory]
    [InlineData(typeof(IValidator<CreateAntibioticCommand>))]
    [InlineData(typeof(IValidator<UpdateAntibioticCommand>))]
    [InlineData(typeof(IValidator<DeleteAntibioticCommand>))]
    [InlineData(typeof(IValidator<AttachAntibioticToCultureCommand>))]
    [InlineData(typeof(IValidator<DetachAntibioticFromCultureCommand>))]
    public void HostBuiltLikeApp_ResolvesM15Validators(System.Type validatorType)
    {
        var services = new ServiceCollection();
        services.AddApplication();
        using var provider = services.BuildServiceProvider();

        var validator = provider.GetService(validatorType);

        Assert.NotNull(validator);
    }
}