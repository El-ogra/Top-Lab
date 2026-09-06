using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Behaviors;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ExternalEntities.Commands.CreateExternalEntity;
using TopLab.Application.Features.ExternalEntities.Commands.DeleteExternalEntity;
using TopLab.Application.Features.ExternalEntities.Commands.GenerateEntityIdCode;
using TopLab.Application.Features.ExternalEntities.Commands.UpdateExternalEntity;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using Xunit;

namespace TopLab.Application.Tests.Features.ExternalEntities;

public class ExternalEntitiesAuthorizationTests
{
    private static object CreateInstance(System.Type commandType)
    {
        return commandType switch
        {
            { } t when t == typeof(CreateExternalEntityCommand) => new CreateExternalEntityCommand(
                EntityType.TreatingDoctor, "Dr. Ahmed", null, null, null, null, null, null, null, null),
            { } t when t == typeof(UpdateExternalEntityCommand) => new UpdateExternalEntityCommand(
                1, EntityType.TreatingDoctor, "Dr. Ahmed", null, null, null, null, null, null, null, null),
            { } t when t == typeof(DeleteExternalEntityCommand) => new DeleteExternalEntityCommand(1),
            { } t when t == typeof(GenerateEntityIdCodeCommand) => new GenerateEntityIdCodeCommand(1),
            _ => throw new InvalidOperationException($"Unhandled command type {commandType.Name} in test.")
        };
    }

    [Theory]
    [InlineData(typeof(CreateExternalEntityCommand))]
    [InlineData(typeof(UpdateExternalEntityCommand))]
    [InlineData(typeof(DeleteExternalEntityCommand))]
    [InlineData(typeof(GenerateEntityIdCodeCommand))]
    public void EveryWriteCommand_Requires_EditSystemSettings(System.Type commandType)
    {
        var authorized = (IAuthorizedRequest)CreateInstance(commandType);

        Assert.Equal("EDIT_SYSTEM_SETTINGS", authorized.RequiredPermissionCode);
    }

    [Fact]
    public async Task WithoutEditPermission_ReturnsForbidden_WithSharedMessage()
    {
        var user = new FakeCurrentUserService { IsAbsolutePermission = false };
        var behavior = new AuthorizationBehavior<DeleteExternalEntityCommand, Result>(user);

        var response = await behavior.Handle(
            new DeleteExternalEntityCommand(1),
            _ => throw new Exception("handler must not run"),
            CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Equal(ErrorType.Forbidden, response.Error!.Type);
        Assert.Equal("أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام", response.Error.Message);
    }

    [Fact]
    public async Task WithoutEditPermission_ReturnsForbidden_ForGenericResult()
    {
        var user = new FakeCurrentUserService { IsAbsolutePermission = false };
        var behavior = new AuthorizationBehavior<CreateExternalEntityCommand, Result<int>>(user);

        var response = await behavior.Handle(
            new CreateExternalEntityCommand(EntityType.TreatingDoctor, "Dr", null, null, null, null, null, null, null, null),
            _ => throw new Exception("handler must not run"),
            CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Equal(ErrorType.Forbidden, response.Error!.Type);
    }

    [Fact]
    public async Task AbsoluteUser_BypassesPermissionCheck()
    {
        var user = new FakeCurrentUserService { IsAbsolutePermission = true };
        var behavior = new AuthorizationBehavior<GenerateEntityIdCodeCommand, Result<string>>(user);

        var response = await behavior.Handle(
            new GenerateEntityIdCodeCommand(1),
            _ => Task.FromResult(Result<string>.Success("CODE1")),
            CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal("CODE1", response.Value);
    }
}
