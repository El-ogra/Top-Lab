using System.IO;
using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.AnalyteProfiles.Common;
using TopLab.Application.Features.AnalyteProfiles.Queries.GetProfileDefinitions;
using TopLab.Application.Features.PatientRegistration.Common;
using TopLab.Application.Features.PatientRegistration.Queries.GetNextLabId;
using TopLab.Application.Features.PatientRegistration.Queries.GetRegistrationCatalog;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Queries.GetCustomGroupTestIds;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Queries.GetCustomGroups;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Common.Navigation;
using TopLab.Presentation.Tests.Common;
using TopLab.Presentation.ViewModels.Patients;
using Xunit;

namespace TopLab.Presentation.Tests.Patients;

/// <summary>
/// R1 Round 5 — D-02: the three mutually exclusive catalogue modes TM / TG / CG.
/// <para>
/// A view filter only: no database write, no schema change, and TM shows exactly the base set
/// the unfiltered catalogue shows today. The search box is active in TG only and its text is
/// cleared when the user leaves TG.
/// </para>
/// </summary>
public class PatientEditorCatalogModeTests
{
    private sealed class ModeSender : ISender
    {
        public IReadOnlyList<int> MemberTestIds { get; set; } = Array.Empty<int>();

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is GetRegistrationCatalogQuery)
            {
                return Task.FromResult((TResponse)(object)Result<RegistrationCatalogDto>.Success(Catalog()));
            }

            if (request is GetCustomGroupTestIdsQuery)
            {
                return Task.FromResult((TResponse)(object)Result<IReadOnlyList<int>>.Success(MemberTestIds));
            }

            if (request is GetProfileDefinitionsQuery)
            {
                return Task.FromResult((TResponse)(object)Result<IReadOnlyList<ProfileDefinitionDto>>.Success(
                    Array.Empty<ProfileDefinitionDto>()));
            }

            if (request is GetCustomGroupsQuery)
            {
                return Task.FromResult((TResponse)(object)Result<IReadOnlyList<TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Common.CustomGroupSummaryDto>>.Success(
                    Array.Empty<TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Common.CustomGroupSummaryDto>()));
            }

            if (request is GetNextLabIdQuery)
            {
                return Task.FromResult((TResponse)(object)Result<string>.Success("LAB-1"));
            }

            throw new NotSupportedException($"ModeSender has no canned response for {request.GetType().Name}.");
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

    private static async Task<PatientEditorViewModel> LoadedAsync(params int[] memberTestIds)
    {
        var sender = new ModeSender { MemberTestIds = memberTestIds };
        var vm = new PatientEditorViewModel(
            sender, new SilentNavigation(), new ResultErrorPresenter(),
            new FakeDialogService(), new StubServices());
        await vm.LoadCatalogAsync();
        return vm;
    }

    // ================================================================
    // Default mode and mutual exclusivity
    // ================================================================

    [Fact]
    public async Task DefaultMode_IsTg_AndItsEmptySearchEqualsTodaysList()
    {
        var vm = await LoadedAsync();

        Assert.Equal(CatalogFilterMode.Tg, vm.CatalogFilterMode);
        Assert.True(vm.IsTgMode);
        Assert.False(vm.IsTmMode);
        Assert.False(vm.IsCgMode);
        Assert.Equal(4, vm.FilteredCatalogTests.Count);
        Assert.True(vm.IsCatalogSearchEnabled);
    }

    [Fact]
    public async Task ExactlyOneModeIsActive_AtAnyTime()
    {
        var vm = await LoadedAsync(1);

        foreach (var mode in new[] { CatalogFilterMode.Tm, CatalogFilterMode.Tg, CatalogFilterMode.Cg })
        {
            vm.CatalogFilterMode = mode;

            Assert.Equal(mode, vm.CatalogFilterMode);
            Assert.Equal(mode == CatalogFilterMode.Tm, vm.IsTmMode);
            Assert.Equal(mode == CatalogFilterMode.Tg, vm.IsTgMode);
            Assert.Equal(mode == CatalogFilterMode.Cg, vm.IsCgMode);

            var active = new[] { vm.IsTmMode, vm.IsTgMode, vm.IsCgMode }.Count(x => x);
            Assert.Equal(1, active);
        }
    }

    [Fact]
    public async Task UndefinedMode_IsRejected_AndTheStateIsUnchanged()
    {
        var vm = await LoadedAsync();
        vm.CatalogFilterMode = CatalogFilterMode.Tm;

        vm.CatalogFilterMode = (CatalogFilterMode)99;

        Assert.Equal(CatalogFilterMode.Tm, vm.CatalogFilterMode);
        Assert.True(vm.IsTmMode);
        Assert.Equal(4, vm.FilteredCatalogTests.Count);
    }

    [Fact]
    public async Task TheThreeCommandsSetTheirMode()
    {
        var vm = await LoadedAsync();

        vm.SetTmModeCommand.Execute(null);
        Assert.Equal(CatalogFilterMode.Tm, vm.CatalogFilterMode);

        vm.SetTgModeCommand.Execute(null);
        Assert.Equal(CatalogFilterMode.Tg, vm.CatalogFilterMode);

        vm.SetCgModeCommand.Execute(null);
        Assert.Equal(CatalogFilterMode.Cg, vm.CatalogFilterMode);
    }

    // ================================================================
    // TM — every test
    // ================================================================

    [Fact]
    public async Task Tm_ShowsAllTests_EvenWithTextInTheBox()
    {
        var vm = await LoadedAsync();
        vm.CatalogFilterText = "CBC";     // in TG this narrows
        Assert.Single(vm.FilteredCatalogTests);

        vm.CatalogFilterMode = CatalogFilterMode.Tm;

        Assert.Equal(4, vm.FilteredCatalogTests.Count);
    }

    [Fact]
    public async Task Tm_ShowsTheSameBaseSetAsTheUnfilteredCatalogue()
    {
        var vm = await LoadedAsync();
        var baseSet = vm.CatalogTests.Select(t => t.Id).OrderBy(x => x).ToList();

        vm.CatalogFilterMode = CatalogFilterMode.Tm;

        Assert.Equal(baseSet, vm.FilteredCatalogTests.Select(t => t.Id).OrderBy(x => x).ToList());
    }

    // ================================================================
    // TG — name or code
    // ================================================================

    [Theory]
    [InlineData("CBC", 1)]      // code match
    [InlineData("cbc", 1)]      // case-insensitive
    [InlineData("سكر", 1)]       // name match
    [InlineData("F", 2)]        // partial code: FBS + LFT
    [InlineData("ZZZ", 0)]      // no match
    [InlineData("", 4)]         // empty -> all
    [InlineData("   ", 4)]      // whitespace -> all
    public async Task Tg_MatchesByNameOrCode(string text, int expected)
    {
        var vm = await LoadedAsync();
        vm.CatalogFilterMode = CatalogFilterMode.Tg;

        vm.CatalogFilterText = text;

        Assert.Equal(expected, vm.FilteredCatalogTests.Count);
    }

    [Fact]
    public async Task Tg_WithNoMatch_RaisesTheNoMatchFlag()
    {
        var vm = await LoadedAsync();
        vm.CatalogFilterMode = CatalogFilterMode.Tg;

        vm.CatalogFilterText = "ZZZ";

        Assert.True(vm.ShowCatalogFilterNoMatch);
        Assert.False(vm.ShowCatalogEmpty);
    }

    // ================================================================
    // CG — custom-group members
    // ================================================================

    [Fact]
    public async Task Cg_WithNoGroups_ShowsNothing()
    {
        var vm = await LoadedAsync();

        vm.CatalogFilterMode = CatalogFilterMode.Cg;

        Assert.Empty(vm.FilteredCatalogTests);
        Assert.False(vm.ShowCatalogEmpty);       // the catalogue itself is not empty
        Assert.False(vm.ShowCatalogFilterNoMatch); // that flag belongs to TG
    }

    [Fact]
    public async Task Cg_WithOneGroup_ShowsOnlyItsMembers()
    {
        var vm = await LoadedAsync(2);

        vm.CatalogFilterMode = CatalogFilterMode.Cg;

        var only = Assert.Single(vm.FilteredCatalogTests);
        Assert.Equal("FBS", only.TestCode);
    }

    [Fact]
    public async Task Cg_WithSeveralGroups_ShowsTheUnion()
    {
        var vm = await LoadedAsync(1, 3, 4);

        vm.CatalogFilterMode = CatalogFilterMode.Cg;

        Assert.Equal(3, vm.FilteredCatalogTests.Count);
        Assert.Equal(
            new[] { 1, 3, 4 },
            vm.FilteredCatalogTests.Select(t => t.Id).OrderBy(x => x).ToArray());
    }

    [Fact]
    public async Task Cg_ATestInSeveralGroupsAppearsOnce()
    {
        // The Application query is DISTINCT, so the same id arriving twice must not duplicate
        // the row; the ViewModel stores the ids in a HashSet for the same reason.
        var vm = await LoadedAsync(1, 1, 1);

        vm.CatalogFilterMode = CatalogFilterMode.Cg;

        var only = Assert.Single(vm.FilteredCatalogTests);
        Assert.Equal(1, only.Id);
    }

    [Fact]
    public async Task Cg_IgnoresTheSearchText()
    {
        var vm = await LoadedAsync(1, 2);
        vm.CatalogFilterMode = CatalogFilterMode.Tg;
        vm.CatalogFilterText = "سكر";     // would narrow TG to FBS

        vm.CatalogFilterMode = CatalogFilterMode.Cg;

        Assert.Equal(2, vm.FilteredCatalogTests.Count);
    }

    // ================================================================
    // The search box's enabled state and the cleared text
    // ================================================================

    [Fact]
    public async Task SearchBox_IsEnabledInTgOnly()
    {
        var vm = await LoadedAsync();

        vm.CatalogFilterMode = CatalogFilterMode.Tg;
        Assert.True(vm.IsCatalogSearchEnabled);

        vm.CatalogFilterMode = CatalogFilterMode.Tm;
        Assert.False(vm.IsCatalogSearchEnabled);

        vm.CatalogFilterMode = CatalogFilterMode.Cg;
        Assert.False(vm.IsCatalogSearchEnabled);
    }

    [Fact]
    public async Task LeavingTg_ClearsTheSearchText()
    {
        var vm = await LoadedAsync();
        vm.CatalogFilterMode = CatalogFilterMode.Tg;
        vm.CatalogFilterText = "CBC";

        vm.CatalogFilterMode = CatalogFilterMode.Tm;

        Assert.Equal(string.Empty, vm.CatalogFilterText);
    }

    [Fact]
    public async Task SwitchingBetweenTmAndCg_KeepsTheTextEmpty()
    {
        var vm = await LoadedAsync(1);

        vm.CatalogFilterMode = CatalogFilterMode.Tm;
        vm.CatalogFilterMode = CatalogFilterMode.Cg;
        vm.CatalogFilterMode = CatalogFilterMode.Tm;

        Assert.Equal(string.Empty, vm.CatalogFilterText);
        Assert.Equal(4, vm.FilteredCatalogTests.Count);
    }

    // ================================================================
    // XAML wiring (read as text — no WPF element is instantiated)
    // ================================================================

    [Fact]
    public void View_HasTheThreeRadioButtonsInOneGroupWithTooltips()
    {
        var xaml = ReadPresentationFile(Path.Combine("Views", "Patients", "PatientEditorView.xaml"));

        Assert.Contains("Content=\"TM\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Content=\"TG\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Content=\"CG\"", xaml, StringComparison.Ordinal);
        Assert.Contains("GroupName=\"CatalogMode\"", xaml, StringComparison.Ordinal);
        Assert.Contains("ToolTip=\"كل التحاليل\"", xaml, StringComparison.Ordinal);
        Assert.Contains("ToolTip=\"بحث بالاسم أو الكود\"", xaml, StringComparison.Ordinal);
        Assert.Contains("ToolTip=\"مجموعاتي المخصصة\"", xaml, StringComparison.Ordinal);
        Assert.Contains("IsEnabled=\"{Binding IsCatalogSearchEnabled}\"", xaml, StringComparison.Ordinal);
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
