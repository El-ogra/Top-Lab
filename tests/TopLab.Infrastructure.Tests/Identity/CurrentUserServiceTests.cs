using TopLab.Application.Common.Interfaces;
using TopLab.Infrastructure.Identity;

namespace TopLab.Infrastructure.Tests.Identity;

public class CurrentUserServiceTests
{
    [Fact]
    public void GivenFreshService_WhenInspected_ThenSessionIsAnonymous()
    {
        ICurrentUserService service = new CurrentUserService(new TestServiceProvider());

        Assert.False(service.IsAuthenticated);
        Assert.Equal(0, service.UserId);
        Assert.Equal(string.Empty, service.UserName);
        Assert.False(service.IsAbsolutePermission);
        Assert.False(service.HasPermission("ANY"));
    }

    [Fact]
    public void GivenSetSession_WhenInspected_ThenValuesAreExposed()
    {
        var service = new CurrentUserService(new TestServiceProvider());
        service.SetSession(userId: 7, userName: "ahmed", isAbsolutePermission: true, grantedPermissions: new[] { "ADD_EDIT_PATIENT" });

        Assert.True(service.IsAuthenticated);
        Assert.Equal(7, service.UserId);
        Assert.Equal("ahmed", service.UserName);
        Assert.True(service.IsAbsolutePermission);
        Assert.True(service.HasPermission("ADD_EDIT_PATIENT"));
        Assert.False(service.HasPermission("OTHER"));
    }

    [Fact]
    public void GivenClearSession_ThenSessionIsAnonymous()
    {
        var service = new CurrentUserService(new TestServiceProvider());
        service.SetSession(7, "ahmed", true, new[] { "P1" });

        service.ClearSession();

        Assert.False(service.IsAuthenticated);
        Assert.Equal(0, service.UserId);
        Assert.Equal(string.Empty, service.UserName);
        Assert.False(service.IsAbsolutePermission);
        Assert.False(service.HasPermission("P1"));
    }

    [Fact]
    public void GivenSetSession_ThenGrantedPermissionsAreCaseSensitive()
    {
        // Permission codes are UPPER_SNAKE_CASE (Coding Standards §4.4); the
        // service must compare exactly so a code like "Add_Patient" cannot
        // accidentally match "ADD_PATIENT".
        var service = new CurrentUserService(new TestServiceProvider());
        service.SetSession(1, "user1", false, new[] { "ADD_PATIENT" });

        Assert.True(service.HasPermission("ADD_PATIENT"));
        Assert.False(service.HasPermission("add_patient"));
    }

    private sealed class TestServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }
}
