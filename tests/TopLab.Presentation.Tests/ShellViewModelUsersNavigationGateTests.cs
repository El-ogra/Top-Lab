using Microsoft.Extensions.DependencyInjection;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Tests.Common;
using TopLab.Presentation.ViewModels.Shell;

namespace TopLab.Presentation.Tests;

/// <summary>
/// FIX-N4: «المستخدمون» navigation must be enabled only for absolute-permission sessions.
/// Constructs the real ShellViewModel with explicit FakeCurrentUserService state.
/// </summary>
public sealed class ShellViewModelUsersNavigationGateTests
{
    private const string UsersTitle = "المستخدمون";

    private static ShellViewModel Create(FakeCurrentUserService currentUser)
    {
        var services = new ServiceCollection().BuildServiceProvider();
        return new ShellViewModel(
            new FakeSender(),
            new FakeNavigationService(),
            currentUser,
            new FakeDateTimeProvider(),
            new ResultErrorPresenter(),
            new FakeDialogService(),
            services,
            new HomeViewModel());
    }

    private static bool IsUsersEnabled(ShellViewModel vm)
    {
        var item = vm.NavigationItems.First(i => i.Title == UsersTitle);
        return item.IsEnabled;
    }

    [Fact]
    public void UsersNavigation_IsEnabled_WhenSessionHasAbsolutePermission()
    {
        var currentUser = new FakeCurrentUserService
        {
            IsAuthenticated = true,
            IsAbsolutePermission = true
        };

        var vm = Create(currentUser);

        Assert.True(IsUsersEnabled(vm));
    }

    [Fact]
    public void UsersNavigation_IsDisabled_WhenSessionLacksAbsolutePermission()
    {
        var currentUser = new FakeCurrentUserService
        {
            IsAuthenticated = true,
            IsAbsolutePermission = false
        };

        var vm = Create(currentUser);

        Assert.False(IsUsersEnabled(vm));
    }

    [Fact]
    public void UsersNavigation_IsDisabled_WhenSessionIsNotAuthenticated()
    {
        var currentUser = new FakeCurrentUserService
        {
            IsAuthenticated = false,
            IsAbsolutePermission = false
        };

        var vm = Create(currentUser);

        Assert.False(IsUsersEnabled(vm));
    }

    [Fact]
    public void OtherPermissionGatedItems_StillRespectCatalogPermissions()
    {
        var nonAbsolute = new FakeCurrentUserService
        {
            IsAuthenticated = true,
            IsAbsolutePermission = false
        };
        nonAbsolute.GrantedPermissions.Add("PRINT_WORKSHEET");

        var vm = Create(nonAbsolute);

        Assert.True(vm.NavigationItems.First(i => i.Title == "ورقة العمل").IsEnabled);
        Assert.False(vm.NavigationItems.First(i => i.Title == "الإحصائيات").IsEnabled);
        Assert.False(IsUsersEnabled(vm));
    }
}