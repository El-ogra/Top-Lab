using FluentValidation;
using TopLab.Application;
using Microsoft.Extensions.DependencyInjection;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.UsersAndPermissions.Commands.SignIn;
using TopLab.Application.Features.UsersAndPermissions.Commands.SignOut;
using TopLab.Application.Features.UsersAndPermissions.Queries.GetCurrentSession;
using TopLab.Application.Features.UsersAndPermissions.Queries.VerifySecondaryPassword;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Tests.Common;
using TopLab.Presentation.ViewModels.Shell;

namespace TopLab.Presentation.Tests;

/// <summary>
/// S-07 Slice 5/6: real behavioural tests for lock-workstation and navigation gating.
/// Constructs ShellViewModel with explicit fake session state — no source-text search.
/// </summary>
public class DeferredBehaviourTests
{
    private const string WorksheetTitle = "ورقة العمل";
    private const string StatisticsTitle = "الإحصائيات";
    private const string SystemTitle = "النظام";
    private const string LockTitle = "قفل المحطة";
    private const string UsersTitle = "المستخدمون";

    private static ShellViewModel CreateShell(FakeCurrentUserService currentUser, FakeSender sender, FakeDialogService dialogs)
    {
        var services = new ServiceCollection();
        services.AddSingleton<UnlockViewModel>(_ => new UnlockViewModel(sender));
        var provider = services.BuildServiceProvider();

        return new ShellViewModel(
            sender,
            new FakeNavigationService(),
            currentUser,
            new FakeDateTimeProvider(),
            new ResultErrorPresenter(),
            dialogs,
            provider,
            new HomeViewModel());
    }

    private static bool IsEnabled(ShellViewModel vm, string title) =>
        vm.NavigationItems.First(i => i.Title == title).IsEnabled;

    [Theory]
    [InlineData(WorksheetTitle, "PRINT_WORKSHEET")]
    [InlineData(StatisticsTitle, "STATISTICS")]
    [InlineData(SystemTitle, "PT_AUDIT_ACCESS")]
    [InlineData(LockTitle, "EDIT_SYSTEM_SETTINGS")]
    public void CatalogGate_IsEnabled_WhenPermissionGranted(string title, string permissionCode)
    {
        var user = new FakeCurrentUserService
        {
            IsAuthenticated = true,
            IsAbsolutePermission = false
        };
        user.GrantedPermissions.Add(permissionCode);

        var vm = CreateShell(user, new FakeSender(), new FakeDialogService());

        Assert.True(IsEnabled(vm, title));
    }

    [Theory]
    [InlineData(WorksheetTitle, "PRINT_WORKSHEET")]
    [InlineData(StatisticsTitle, "STATISTICS")]
    [InlineData(SystemTitle, "PT_AUDIT_ACCESS")]
    [InlineData(LockTitle, "EDIT_SYSTEM_SETTINGS")]
    public void CatalogGate_IsDisabled_WhenPermissionMissing(string title, string _)
    {
        var user = new FakeCurrentUserService
        {
            IsAuthenticated = true,
            IsAbsolutePermission = false
        };

        var vm = CreateShell(user, new FakeSender(), new FakeDialogService());

        Assert.False(IsEnabled(vm, title));
    }

    [Fact]
    public void CatalogGate_IsEnabled_WhenAbsolute_EvenWithoutCatalogPermission()
    {
        var user = new FakeCurrentUserService
        {
            IsAuthenticated = true,
            IsAbsolutePermission = true
        };

        var vm = CreateShell(user, new FakeSender(), new FakeDialogService());

        Assert.True(IsEnabled(vm, WorksheetTitle));
        Assert.True(IsEnabled(vm, StatisticsTitle));
        Assert.True(IsEnabled(vm, SystemTitle));
        Assert.True(IsEnabled(vm, LockTitle));
        Assert.True(IsEnabled(vm, UsersTitle));
    }

    [Fact]
    public void UsersGate_IsDisabled_WithoutAbsolute()
    {
        var user = new FakeCurrentUserService
        {
            IsAuthenticated = true,
            IsAbsolutePermission = false
        };
        user.GrantedPermissions.Add("PRINT_WORKSHEET");

        var vm = CreateShell(user, new FakeSender(), new FakeDialogService());

        Assert.False(IsEnabled(vm, UsersTitle));
    }

    [Fact]
    public async Task LockWorkstation_Failure_SurfacesError_AndDoesNotOpenUnlock()
    {
        var user = new FakeCurrentUserService
        {
            IsAuthenticated = true,
            IsAbsolutePermission = true,
            UserName = "admin"
        };
        var sender = new FakeSender().WithLockResult(Result.Failure(Error.Unexpected("lock failed")));
        var dialogs = new FakeDialogService();
        var vm = CreateShell(user, sender, dialogs);

        var unlockOpened = false;
        await vm.LockWorkstationAsync(_ =>
        {
            unlockOpened = true;
            return true;
        });

        Assert.False(unlockOpened);
        Assert.Single(dialogs.Errors);
    }

    [Fact]
    public async Task LockWorkstation_Success_OpensUnlock_AndReloadsStatus()
    {
        var user = new FakeCurrentUserService
        {
            IsAuthenticated = true,
            IsAbsolutePermission = true,
            UserName = "admin"
        };
        var sender = new FakeSender().WithLockResult(Result.Success());
        var dialogs = new FakeDialogService();
        var vm = CreateShell(user, sender, dialogs);

        var unlockOpened = false;
        await vm.LockWorkstationAsync(unlockVm =>
        {
            unlockOpened = true;
            Assert.Equal("admin", unlockVm.UserName);
            return true;
        });

        Assert.True(unlockOpened);
        Assert.Empty(dialogs.Errors);
    }

    [Fact]
    public void LoginPath_Requests_DoNotImplementIAuthorizedRequest()
    {
        Assert.False(typeof(IAuthorizedRequest).IsAssignableFrom(typeof(SignInCommand)));
        Assert.False(typeof(IAuthorizedRequest).IsAssignableFrom(typeof(SignOutCommand)));
        Assert.False(typeof(IAuthorizedRequest).IsAssignableFrom(typeof(GetCurrentSessionQuery)));
        Assert.False(typeof(IAuthorizedRequest).IsAssignableFrom(typeof(VerifySecondaryPasswordQuery)));
    }

    [Fact]
    public void LoginPath_Validators_AreRegistered_InApplicationDi()
    {
        var services = new ServiceCollection();
        services.AddApplication();
        using var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetService<IValidator<SignInCommand>>());
        Assert.NotNull(provider.GetService<IValidator<VerifySecondaryPasswordQuery>>());
    }

    [Fact]
    public void AuthorizationBehavior_IsRegistered_InMediatrPipeline()
    {
        var services = new ServiceCollection();
        services.AddApplication();

        // Inspect descriptors — do not resolve LoggingBehavior (needs IAppLogger).
        Assert.Contains(
            services,
            d => d.ImplementationType?.Name.StartsWith("AuthorizationBehavior") == true);

        Assert.Contains(
            services,
            d => d.ImplementationType?.Name.StartsWith("ValidationBehavior") == true);
    }
}