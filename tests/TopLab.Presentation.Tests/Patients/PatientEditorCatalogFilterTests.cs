using System.Collections.ObjectModel;
using System.IO;
using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientBilling.Common;
using TopLab.Application.Features.PatientBilling.Queries.GetPatientAccount;
using TopLab.Application.Features.PatientRegistration.Common;
using TopLab.Application.Features.PatientRegistration.Queries.GetPatientById;
using TopLab.Application.Features.PatientRegistration.Queries.GetPatientVisitHistory;
using TopLab.Application.Features.AnalyteProfiles.Common;
using TopLab.Application.Features.AnalyteProfiles.Queries.GetProfileDefinitions;
using TopLab.Application.Features.PatientRegistration.Queries.GetNextLabId;
using TopLab.Application.Features.PatientRegistration.Queries.GetRegistrationCatalog;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Common;
using TopLab.Application.Features.WorkSheets.Common;
using TopLab.Application.Features.WorkSheets.Queries.GetVisitWorkSheet;
using TopLab.Domain.Common.Enums;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Common.Navigation;
using TopLab.Presentation.Tests.Common;
using TopLab.Presentation.ViewModels.Patients;
using Xunit;

namespace TopLab.Presentation.Tests.Patients;

/// <summary>
/// R1 Round 4 / A1: the test-catalogue filter field (canonical reference 1.2.2.5.1.1).
/// The filter narrows the master test list by test NAME or test CODE, case-insensitively,
/// and an empty filter restores the whole catalogue.
/// </summary>
public class PatientEditorCatalogFilterTests
{
    private sealed class CatalogSender : ISender
    {
        /// <summary>Ids that belong to at least one custom group (D-02 mode CG).</summary>
        public IReadOnlyList<int> MemberTestIds { get; set; } = Array.Empty<int>();

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is GetRegistrationCatalogQuery)
            {
                return Task.FromResult((TResponse)(object)Result<RegistrationCatalogDto>.Success(Catalog()));
            }

            if (request is TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Queries.GetCustomGroupTestIds.GetCustomGroupTestIdsQuery)
            {
                return Task.FromResult((TResponse)(object)Result<IReadOnlyList<int>>.Success(MemberTestIds));
            }

            if (request is GetProfileDefinitionsQuery)
            {
                return Task.FromResult((TResponse)(object)Result<IReadOnlyList<ProfileDefinitionDto>>.Success(
                    Array.Empty<ProfileDefinitionDto>()));
            }

            if (request is TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Queries.GetCustomGroups.GetCustomGroupsQuery)
            {
                return Task.FromResult((TResponse)(object)Result<IReadOnlyList<TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Common.CustomGroupSummaryDto>>.Success(
                    Array.Empty<TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Common.CustomGroupSummaryDto>()));
            }

            if (request is GetNextLabIdQuery)
            {
                return Task.FromResult((TResponse)(object)Result<string>.Success("LAB-1"));
            }

            throw new NotSupportedException($"CatalogSender has no canned response for {request.GetType().Name}.");
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest => throw new NotSupportedException();
        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        private static RegistrationCatalogDto Catalog() => new(
            Tests: new[]
            {
                Test(1, "CBC", "صورة دم كاملة"),
                Test(2, "FBS", "سكر صائم"),
                Test(3, "LFT", "وظائف كبد"),
                Test(4, "CULT", "مزرعة")
            },
            TestGroups: Array.Empty<TestGroupDto>(),
            PatientTitles: Array.Empty<PatientTitleDto>(),
            MedicalConditionTypes: Array.Empty<MedicalConditionTypeDto>(),
            DefaultAccountType: AccountType.Individual,
            DisableAutoTitleInsertion: false,
            ReferralPlaceholderMale: string.Empty,
            ReferralPlaceholderFemale: string.Empty);

        private static TestSummaryDto Test(int id, string code, string name) => new(
            Id: id, TestCode: code, Name: name, ReportName: name, TestGroupId: null,
            TestGroupName: null, Barcode: null, CompletionDurationMinutes: 0, IsSentOut: false,
            PatientPrice: 100m, LabToLabPrice: null, IsActive: true);
    }

    private sealed class SilentNavigation : INavigationService
    {
        public ViewModelBase? CurrentViewModel => null;
        public event Action<ViewModelBase?>? Navigated
        {
            add { }
            remove { }
        }
        public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase { }
        public void NavigateTo(ViewModelBase viewModel) { }
    }

    private sealed class StubServices : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private static async Task<PatientEditorViewModel> LoadedAsync()
    {
        var vm = new PatientEditorViewModel(
            new CatalogSender(), new SilentNavigation(), new ResultErrorPresenter(),
            new FakeDialogService(), new StubServices());
        await vm.LoadCatalogAsync();
        return vm;
    }

    [Fact]
    public async Task NoFilter_ShowsTheWholeCatalogue()
    {
        var vm = await LoadedAsync();

        Assert.Equal(4, vm.CatalogTests.Count);
        Assert.Equal(4, vm.FilteredCatalogTests.Count);
    }

    [Fact]
    public async Task FilterByTestCode_NarrowsTheList()
    {
        var vm = await LoadedAsync();

        vm.CatalogFilterText = "CBC";

        var only = Assert.Single(vm.FilteredCatalogTests);
        Assert.Equal("CBC", only.TestCode);
        Assert.Equal(4, vm.CatalogTests.Count); // the source collection is untouched
    }

    [Fact]
    public async Task FilterByTestName_NarrowsTheList()
    {
        var vm = await LoadedAsync();

        vm.CatalogFilterText = "سكر";

        var only = Assert.Single(vm.FilteredCatalogTests);
        Assert.Equal("FBS", only.TestCode);
    }

    [Fact]
    public async Task FilterIsCaseInsensitiveOnCode()
    {
        var vm = await LoadedAsync();

        vm.CatalogFilterText = "cbc";

        Assert.Single(vm.FilteredCatalogTests);
    }

    [Fact]
    public async Task FilterMatchesAPartialCode()
    {
        var vm = await LoadedAsync();

        vm.CatalogFilterText = "F";

        // "FBS" (code) and "LFT" (code) both contain F; "CULT" does not.
        Assert.Equal(2, vm.FilteredCatalogTests.Count);
    }

    [Fact]
    public async Task NoMatch_ShowsAnEmptyListAndTheNoMatchFlag()
    {
        var vm = await LoadedAsync();

        vm.CatalogFilterText = "ZZZ";

        Assert.Empty(vm.FilteredCatalogTests);
        Assert.True(vm.ShowCatalogFilterNoMatch);
        Assert.False(vm.ShowCatalogEmpty); // the catalogue itself is not empty
    }

    [Fact]
    public async Task EmptyInput_RestoresTheWholeCatalogue()
    {
        var vm = await LoadedAsync();
        vm.CatalogFilterText = "CBC";

        vm.CatalogFilterText = string.Empty;

        Assert.Equal(4, vm.FilteredCatalogTests.Count);
        Assert.False(vm.ShowCatalogFilterNoMatch);
    }

    [Fact]
    public async Task WhitespaceInput_IsTreatedAsNoFilter()
    {
        var vm = await LoadedAsync();

        vm.CatalogFilterText = "   ";

        Assert.Equal(4, vm.FilteredCatalogTests.Count);
        Assert.False(vm.ShowCatalogFilterNoMatch);
    }

    [Fact]
    public async Task ClearCommand_RestoresTheWholeCatalogue()
    {
        var vm = await LoadedAsync();
        vm.CatalogFilterText = "CBC";

        vm.ClearCatalogFilterCommand.Execute(null);

        Assert.Equal(string.Empty, vm.CatalogFilterText);
        Assert.Equal(4, vm.FilteredCatalogTests.Count);
    }

    [Fact]
    public void View_BindsTheFilterFieldAndTheFilteredList()
    {
        var xaml = ReadPresentationFile(Path.Combine("Views", "Patients", "PatientEditorView.xaml"));

        Assert.Contains("Binding CatalogFilterText", xaml, StringComparison.Ordinal);
        Assert.Contains("ItemsSource=\"{Binding FilteredCatalogTests}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Command=\"{Binding ClearCatalogFilterCommand}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("FlowDirection=\"RightToLeft\"", xaml, StringComparison.Ordinal);
    }

    private static string ReadPresentationFile(string relativePath)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "src", "TopLab.Presentation")))
        {
            dir = dir.Parent;
        }

        Assert.NotNull(dir);
        return File.ReadAllText(Path.Combine(dir!.FullName, "src", "TopLab.Presentation", relativePath));
    }
}
