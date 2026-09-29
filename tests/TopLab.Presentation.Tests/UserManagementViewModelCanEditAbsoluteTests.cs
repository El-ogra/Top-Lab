using System.ComponentModel;
using TopLab.Application.Common.Results;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Tests.Common;
using TopLab.Presentation.ViewModels.Users;

namespace TopLab.Presentation.Tests;

/// <summary>
/// FIX-N1: CanEditAbsolute must reflect the session's IsAbsolutePermission
/// (explicit FakeCurrentUserService state; defaults are never relied upon).
/// </summary>
public sealed class UserManagementViewModelCanEditAbsoluteTests
{
    private static UserManagementViewModel Create(FakeCurrentUserService currentUser)
    {
        return new UserManagementViewModel(
            new FakeSender(),
            new FakeDialogService(),
            new ResultErrorPresenter(),
            new FakeNavigationService(),
            currentUser);
    }

    [Fact]
    public void CanEditAbsolute_IsTrue_WhenSessionHasAbsolutePermission()
    {
        var currentUser = new FakeCurrentUserService
        {
            IsAuthenticated = true,
            IsAbsolutePermission = true
        };

        var vm = Create(currentUser);

        Assert.True(vm.CanEditAbsolute);
    }

    [Fact]
    public void CanEditAbsolute_IsFalse_WhenSessionLacksAbsolutePermission()
    {
        var currentUser = new FakeCurrentUserService
        {
            IsAuthenticated = true,
            IsAbsolutePermission = false
        };

        var vm = Create(currentUser);

        Assert.False(vm.CanEditAbsolute);
    }

    [Fact]
    public void CanEditAbsolute_IsFalse_WhenSessionIsNotAuthenticated()
    {
        var currentUser = new FakeCurrentUserService
        {
            IsAuthenticated = false,
            IsAbsolutePermission = false
        };

        var vm = Create(currentUser);

        Assert.False(vm.CanEditAbsolute);
    }

    [Fact]
    public async Task LoadAsync_RaisesPropertyChanged_ForCanEditAbsolute()
    {
        var currentUser = new FakeCurrentUserService
        {
            IsAuthenticated = true,
            IsAbsolutePermission = false
        };
        var vm = Create(currentUser);

        string? raised = null;
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(UserManagementViewModel.CanEditAbsolute))
            {
                raised = e.PropertyName;
            }
        };

        await vm.LoadAsync();

        Assert.Equal(nameof(UserManagementViewModel.CanEditAbsolute), raised);
        Assert.False(vm.CanEditAbsolute);
    }
}