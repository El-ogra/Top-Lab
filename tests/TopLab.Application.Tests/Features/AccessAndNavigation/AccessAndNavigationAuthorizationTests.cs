using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Behaviors;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.AccessAndNavigation.Commands.LockWorkstation;
using TopLab.Application.Features.AccessAndNavigation.Common;
using TopLab.Application.Tests.Common.Fakes;
using Xunit;

namespace TopLab.Application.Tests.Features.AccessAndNavigation;

public class AccessAndNavigationAuthorizationTests
{
    [Fact]
    public void LockWorkstationCommand_Requires_EditSystemSettings()
    {
        var command = new LockWorkstationCommand();

        Assert.Equal(AccessAndNavigationAccessPolicy.EditSystemSettings, command.RequiredPermissionCode);
        Assert.Equal("EDIT_SYSTEM_SETTINGS", command.RequiredPermissionCode);

        // Guarded via IAuthorizedRequest so AuthorizationBehavior picks it up
        // automatically (Architecture §6.3, ADR-0009).
        Assert.IsAssignableFrom<IAuthorizedRequest>(command);
    }

    [Fact]
    public async Task WithoutEditPermission_ReturnsForbidden_WithSharedMessage()
    {
        var user = new FakeCurrentUserService { IsAbsolutePermission = false };
        var behavior = new AuthorizationBehavior<LockWorkstationCommand, Result>(user);

        var response = await behavior.Handle(
            new LockWorkstationCommand(),
            _ => throw new InvalidOperationException("handler must not run"),
            CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Equal(ErrorType.Forbidden, response.Error!.Type);
        Assert.Equal("أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام", response.Error.Message);
        Assert.Equal("EDIT_SYSTEM_SETTINGS", response.Error.Code);
    }

    [Fact]
    public async Task WithEditSystemSettings_GrantsPermission()
    {
        var user = new FakeCurrentUserService
        {
            IsAbsolutePermission = false,
            GrantedPermissions = { "EDIT_SYSTEM_SETTINGS" }
        };
        var behavior = new AuthorizationBehavior<LockWorkstationCommand, Result>(user);

        var response = await behavior.Handle(
            new LockWorkstationCommand(),
            _ => Task.FromResult(Result.Success()),
            CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Empty(response.Errors);
    }

    [Fact]
    public async Task AbsoluteUser_BypassesPermissionCheck()
    {
        var user = new FakeCurrentUserService { IsAbsolutePermission = true };
        var behavior = new AuthorizationBehavior<LockWorkstationCommand, Result>(user);

        var response = await behavior.Handle(
            new LockWorkstationCommand(),
            _ => Task.FromResult(Result.Success()),
            CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Empty(response.Errors);
    }
}