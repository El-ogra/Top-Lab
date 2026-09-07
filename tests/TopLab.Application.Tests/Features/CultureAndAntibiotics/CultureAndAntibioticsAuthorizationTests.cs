using TopLab.Application.Common.Authorization;
using TopLab.Application.Features.CultureAndAntibiotics.Commands.AttachAntibioticToCulture;
using TopLab.Application.Features.CultureAndAntibiotics.Commands.CreateAntibiotic;
using TopLab.Application.Features.CultureAndAntibiotics.Commands.DeleteAntibiotic;
using TopLab.Application.Features.CultureAndAntibiotics.Commands.DetachAntibioticFromCulture;
using TopLab.Application.Features.CultureAndAntibiotics.Commands.UpdateAntibiotic;
using Xunit;

namespace TopLab.Application.Tests.Features.CultureAndAntibiotics;

public class CultureAndAntibioticsAuthorizationTests
{
    [Theory]
    [InlineData(typeof(CreateAntibioticCommand))]
    [InlineData(typeof(UpdateAntibioticCommand))]
    [InlineData(typeof(DeleteAntibioticCommand))]
    [InlineData(typeof(AttachAntibioticToCultureCommand))]
    [InlineData(typeof(DetachAntibioticFromCultureCommand))]
    public void EveryWriteCommand_Requires_EditSystemSettings(System.Type commandType)
    {
        object instance = commandType.Name switch
        {
            nameof(CreateAntibioticCommand) => new CreateAntibioticCommand("Amoxicillin", false, false),
            nameof(UpdateAntibioticCommand) => new UpdateAntibioticCommand(1, "Amoxicillin", false, false),
            nameof(DeleteAntibioticCommand) => new DeleteAntibioticCommand(1),
            nameof(AttachAntibioticToCultureCommand) => new AttachAntibioticToCultureCommand(1, 1),
            nameof(DetachAntibioticFromCultureCommand) => new DetachAntibioticFromCultureCommand(1, 1),
            _ => throw new InvalidOperationException($"Unhandled command type {commandType.Name}.")
        };

        var authorized = (IAuthorizedRequest)instance;

        Assert.Equal("EDIT_SYSTEM_SETTINGS", authorized.RequiredPermissionCode);
    }
}