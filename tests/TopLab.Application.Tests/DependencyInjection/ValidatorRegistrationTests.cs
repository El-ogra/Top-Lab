using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TopLab.Application.Features.ExternalEntities.Commands.CreateExternalEntity;
using TopLab.Application.Features.ExternalEntities.Commands.DeleteExternalEntity;
using TopLab.Application.Features.ExternalEntities.Commands.GenerateEntityIdCode;
using TopLab.Application.Features.ExternalEntities.Commands.UpdateExternalEntity;
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
}