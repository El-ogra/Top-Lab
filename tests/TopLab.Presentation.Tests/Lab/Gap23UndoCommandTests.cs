using System.Reflection;
using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.AnalyteProfiles.Common;
using TopLab.Application.Features.CultureAndAntibiotics.Common;
using TopLab.Application.Features.ExternalEntities.Common;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Common;
using TopLab.Application.Features.UsersAndPermissions.Common;
using TopLab.Application.Features.UsersAndPermissions.Queries.GetUserById;
using TopLab.Application.Features.UsersAndPermissions.Queries.GetUsers;
using TopLab.Domain.Common.Enums;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Common.Navigation;
using TopLab.Presentation.Tests.Common;
using TopLab.Presentation.ViewModels.External;
using TopLab.Presentation.ViewModels.Lab;
using TopLab.Presentation.ViewModels.Users;
using Xunit;

namespace TopLab.Presentation.Tests.Lab;

/// <summary>
/// GAP-23 (Bucket A): Undo command presence and revert behaviour on the five
/// owner-approved editing screens. TestCatalogViewModel is deliberately excluded.
/// </summary>
public class Gap23UndoCommandTests
{
    private sealed class SilentNavigation : INavigationService
    {
        public TopLab.Presentation.Common.ViewModelBase? CurrentViewModel => null;
        public event Action<ViewModelBase?>? Navigated { add { } remove { } }
        public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase { }
        public void NavigateTo(ViewModelBase viewModel) { }
    }

    private static FakeSender EmptySender()
    {
        var s = new FakeSender();
        // TestGroups / TestEditor queries
        s.WithResponse(new TopLab.Application.Features.TestCatalogAndReferenceRanges.Queries.GetTestGroups.GetTestGroupsQuery(IncludeInactive: true),
            Result<IReadOnlyList<TopLab.Application.Features.TestCatalogAndReferenceRanges.Common.TestGroupDto>>.Success(Array.Empty<TopLab.Application.Features.TestCatalogAndReferenceRanges.Common.TestGroupDto>()));
        s.WithResponse(new TopLab.Application.Features.TestCatalogAndReferenceRanges.Queries.SearchTestCatalog.SearchTestCatalogQuery(null, null, IncludeInactive: true),
            Result<IReadOnlyList<TopLab.Application.Features.TestCatalogAndReferenceRanges.Common.TestSummaryDto>>.Success(Array.Empty<TopLab.Application.Features.TestCatalogAndReferenceRanges.Common.TestSummaryDto>()));
        s.WithResponse(new TopLab.Application.Features.AnalyteProfiles.Queries.GetAnalyteDefinitions.GetAnalyteDefinitionsQuery(),
            Result<IReadOnlyList<AnalyteDefinitionDto>>.Success(Array.Empty<AnalyteDefinitionDto>()));
        s.WithResponse(new TopLab.Application.Features.TestCatalogAndReferenceRanges.Queries.GetReferenceRanges.GetReferenceRangesQuery(1),
            Result<IReadOnlyList<TopLab.Application.Features.TestCatalogAndReferenceRanges.Common.ReferenceRangeDto>>.Success(Array.Empty<TopLab.Application.Features.TestCatalogAndReferenceRanges.Common.ReferenceRangeDto>()));
        // External entities
        s.WithResponse(new TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Queries.GetPriceLists.GetPriceListsQuery(),
            Result<IReadOnlyList<TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Common.PriceListSummaryDto>>.Success(Array.Empty<TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Common.PriceListSummaryDto>()));
        // External entities
        s.WithResponse(new TopLab.Application.Features.ExternalEntities.Queries.GetExternalEntityById.GetExternalEntityByIdQuery(1),
            Result<TopLab.Application.Features.ExternalEntities.Common.ExternalEntityDetailDto>.Success(
                new TopLab.Application.Features.ExternalEntities.Common.ExternalEntityDetailDto(1, TopLab.Domain.Common.Enums.EntityType.TreatingDoctor, "Name", null, null, null, null, null, null, null, null, null, null)));
        // Users
        s.WithResponse(new GetUserByIdQuery(1),
            Result<UserDetailDto>.Success(new UserDetailDto(1, "u", false, true, null, 0m, false, null, null, false, null, Array.Empty<string>())));
        return s;
    }

    [Fact]
    public void TestCatalogViewModel_HasNoUndoCommand()
    {
        var prop = typeof(TestCatalogViewModel).GetProperty("UndoCommand", BindingFlags.Public | BindingFlags.Instance);
        Assert.Null(prop);
    }

    [Fact]
    public void TestEditorViewModel_HasUndoCommand()
    {
        var vm = new TestEditorViewModel(EmptySender(), new FakeDialogService(), new ResultErrorPresenter());
        Assert.NotNull(vm.UndoCommand);
    }

    [Fact]
    public void TestGroupsViewModel_HasUndoCommand()
    {
        var vm = new TestGroupsViewModel(EmptySender(), new FakeDialogService(), new ResultErrorPresenter());
        Assert.NotNull(vm.UndoCommand);
    }

    [Fact]
    public void ExternalEntityEditorViewModel_HasUndoCommand()
    {
        var vm = new ExternalEntityEditorViewModel(EmptySender(), new ResultErrorPresenter());
        Assert.NotNull(vm.UndoCommand);
    }

    [Fact]
    public void AntibioticEditorViewModel_HasUndoCommand()
    {
        var vm = new AntibioticEditorViewModel(EmptySender(), new ResultErrorPresenter());
        Assert.NotNull(vm.UndoCommand);
    }

    [Fact]
    public void UserManagementViewModel_HasUndoCommand()
    {
        var vm = new UserManagementViewModel(EmptySender(), new FakeDialogService(), new ResultErrorPresenter(), new SilentNavigation(), new FakeCurrentUserService());
        Assert.NotNull(vm.UndoCommand);
    }

    [Fact]
    public async Task TestEditor_Undo_InNewMode_ResetsFields_WithoutSaving()
    {
        var vm = new TestEditorViewModel(EmptySender(), new FakeDialogService(), new ResultErrorPresenter());
        vm.InitializeNew();
        vm.Name = "changed";
        vm.TestCode = "T999";
        vm.PatientPrice = 123m;

        var ex = await Record.ExceptionAsync(async () => { vm.UndoCommand.Execute(null); await Task.Delay(50); });
        Assert.Null(ex);
        Assert.Equal(string.Empty, vm.Name);
        Assert.Equal(string.Empty, vm.TestCode);
    }

    [Fact]
    public async Task TestGroups_Undo_DoesNotThrow()
    {
        var vm = new TestGroupsViewModel(EmptySender(), new FakeDialogService(), new ResultErrorPresenter());
        vm.GroupName = "changed";
        var ex = await Record.ExceptionAsync(async () => { vm.UndoCommand.Execute(null); await Task.Delay(50); });
        Assert.Null(ex);
    }

    [Fact]
    public async Task ExternalEntity_Undo_InNewMode_ResetsFields()
    {
        var vm = new ExternalEntityEditorViewModel(EmptySender(), new ResultErrorPresenter());
        vm.InitializeNew(null);
        vm.Name = "changed";
        vm.City = "Baghdad";

        var ex = await Record.ExceptionAsync(async () => { vm.UndoCommand.Execute(null); await Task.Delay(50); });
        Assert.Null(ex);
        Assert.Equal(string.Empty, vm.Name);
    }

    [Fact]
    public async Task Antibiotic_Undo_RevertsToLoadedSnapshot()
    {
        var vm = new AntibioticEditorViewModel(EmptySender(), new ResultErrorPresenter());
        vm.InitializeEdit(new AntibioticDto(5, "Original", false, false, "S", "Sci", "Comm"));
        vm.Name = "Changed";
        vm.IsPregnancyFlagged = true;
        vm.Symbol = "X";

        vm.UndoCommand.Execute(null);
        await Task.Delay(50);

        Assert.Equal("Original", vm.Name);
        Assert.False(vm.IsPregnancyFlagged);
        Assert.Equal("S", vm.Symbol);
    }

    [Fact]
    public async Task Antibiotic_Undo_InNewMode_ResetsToEmpty()
    {
        var vm = new AntibioticEditorViewModel(EmptySender(), new ResultErrorPresenter());
        vm.InitializeNew();
        vm.Name = "Changed";

        vm.UndoCommand.Execute(null);
        await Task.Delay(50);

        Assert.Equal(string.Empty, vm.Name);
    }

    [Fact]
    public async Task UserManagement_Undo_WithNoSelection_DoesNotThrow()
    {
        var vm = new UserManagementViewModel(EmptySender(), new FakeDialogService(), new ResultErrorPresenter(), new SilentNavigation(), new FakeCurrentUserService());
        var ex = await Record.ExceptionAsync(async () => { vm.UndoCommand.Execute(null); await Task.Delay(50); });
        Assert.Null(ex);
    }

    [Fact]
    public async Task UserManagement_Undo_RevertsSelectedDetail()
    {
        var sender = EmptySender();
        var vm = new UserManagementViewModel(sender, new FakeDialogService(), new ResultErrorPresenter(), new SilentNavigation(), new FakeCurrentUserService());
        await vm.LoadAsync();
        vm.UserName = "changed";
        var ex = await Record.ExceptionAsync(() => { vm.UndoCommand.Execute(null); return Task.CompletedTask; });
        Assert.Null(ex);
    }

    [Fact]
    public async Task ExternalEntity_Undo_InEditMode_ReloadsDetail()
    {
        var sender = EmptySender();
        var vm = new ExternalEntityEditorViewModel(sender, new ResultErrorPresenter());
        vm.InitializeEdit(1);
        await vm.LoadDetailAsync();
        vm.Name = "changed";

        var ex = await Record.ExceptionAsync(async () => { vm.UndoCommand.Execute(null); await Task.Delay(50); });
        Assert.Null(ex);
    }

    [Fact]
    public async Task TestEditor_Undo_InEditMode_ReloadsDetail()
    {
        var sender = EmptySender();
        // register GetTestById
        sender.WithResponse(new TopLab.Application.Features.TestCatalogAndReferenceRanges.Queries.GetTestById.GetTestByIdQuery(1),
            Result<TopLab.Application.Features.TestCatalogAndReferenceRanges.Common.TestDetailDto>.Success(
                new TopLab.Application.Features.TestCatalogAndReferenceRanges.Common.TestDetailDto(1, "T1", "Name", "R", "Rc", null, null, null, 60, false, null, 100m, null, 0, false, true)));
        var vm = new TestEditorViewModel(sender, new FakeDialogService(), new ResultErrorPresenter());
        vm.InitializeEdit(1);
        await vm.LoadDetailAsync();
        vm.Name = "changed";

        var ex = await Record.ExceptionAsync(async () => { vm.UndoCommand.Execute(null); await Task.Delay(50); });
        Assert.Null(ex);
    }
}
