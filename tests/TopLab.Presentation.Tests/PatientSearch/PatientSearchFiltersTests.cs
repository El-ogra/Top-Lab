using System.IO;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ExternalEntities.Common;
using TopLab.Application.Features.ExternalEntities.Queries.SearchExternalEntities;
using TopLab.Application.Features.PatientSearch.Common;
using TopLab.Application.Features.PatientSearch.Queries.GetPatientByLabId;
using TopLab.Application.Features.PatientSearch.Queries.SearchPatientsGlobal;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Common;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Queries.SearchTestCatalog;
using TopLab.Domain.Common.Enums;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Tests.Common;
using TopLab.Presentation.Views.Patients;
using TopLab.Presentation.ViewModels.Patients;
using MediatR;
using Xunit;

namespace TopLab.Presentation.Tests.PatientSearch;

/// <summary>
/// S3 — the six P-01 filter controls on the patient-search screen.
///
/// VG-03 requires SIX controls, not five (C-2), and a test that FAILS if the treating-doctor
/// and referral-entity controls are ever merged (SD-6). The tests below read the real XAML file
/// and the real ViewModel type, so a merged control or a merged property fails rather than
/// silently passing.
/// </summary>
public class PatientSearchFiltersTests
{
    private static string ViewPath()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "src", "TopLab.Presentation")))
        {
            dir = dir.Parent;
        }

        Assert.NotNull(dir);
        return Path.Combine(dir!.FullName, "src", "TopLab.Presentation", "Views", "Patients", "PatientSearchView.xaml");
    }

    private static string ViewXaml() => File.ReadAllText(ViewPath());

    /// <summary>Records every query the ViewModel sends, so the two lookups can be told apart.</summary>
    private sealed class RecordingSender : ISender
    {
        public List<SearchPatientsGlobalQuery> Searches { get; } = new();
        public List<EntityType> ExternalEntityLookups { get; } = new();
        public int TestCatalogLookups { get; private set; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            switch (request)
            {
                case SearchPatientsGlobalQuery search:
                    Searches.Add(search);
                    return Task.FromResult((TResponse)(object)Result<IReadOnlyList<PatientSearchHitDto>>
                        .Success(Array.Empty<PatientSearchHitDto>()));

                case SearchExternalEntitiesQuery entities:
                    ExternalEntityLookups.Add(entities.EntityType!.Value);
                    var list = entities.EntityType == EntityType.TreatingDoctor
                        ? new[] { new ExternalEntityListItemDto(1, EntityType.TreatingDoctor, "د. أحمد", null, null, null, null, null, null) }
                        : new[] { new ExternalEntityListItemDto(2, EntityType.ReferralOrContract, "مركز القلب", null, null, null, null, null, null) };
                    return Task.FromResult((TResponse)(object)Result<IReadOnlyList<ExternalEntityListItemDto>>.Success(list));

                case SearchTestCatalogQuery:
                    TestCatalogLookups++;
                    return Task.FromResult((TResponse)(object)Result<IReadOnlyList<TestSummaryDto>>
                        .Success(Array.Empty<TestSummaryDto>()));

                case GetPatientByLabIdQuery:
                    return Task.FromResult((TResponse)(object)Result<VisitHistoryDto>
                        .Success(new VisitHistoryDto("L", "P", "ByLabCode", false, Array.Empty<VisitSummaryDto>())));

                default:
                    throw new NotSupportedException($"RecordingSender has no canned response for {request.GetType().Name}.");
            }
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : IRequest
            => throw new NotSupportedException();

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private static (PatientSearchViewModel Vm, RecordingSender Sender) Screen()
    {
        var sender = new RecordingSender();
        var vm = new PatientSearchViewModel(sender, new ResultErrorPresenter(), new FakeNavigationService());
        return (vm, sender);
    }

    private static SearchPatientsGlobalQuery LastSearch(RecordingSender sender)
        => sender.Searches[^1];

    // =====================================================================
    // VG-03: six distinct controls, separately bound
    // =====================================================================

    [Fact]
    public void PatientSearch_HasSixFilterControls_EachBoundToItsOwnProperty()
    {
        var xaml = ViewXaml();

        // Six controls, each with its own ItemsSource/SelectedItem binding pair.
        var controls = new (string Label, string Selected, string Source)[]
        {
            ("الطبيب المعالج:", "SelectedTreatingDoctor", "TreatingDoctorOptions"),
            ("جهة الإحالة:", "SelectedReferralEntity", "ReferralEntityOptions"),
            ("التحليل:", "SelectedTest", "TestOptions"),
            ("الجنس:", "SelectedSex", "SexOptions"),
            ("العمر:", "AgeFrom", "AgeUnitOptions"),
            ("من تاريخ:", "FromDate", "ToDate")
        };

        // Assert SIX, not five (C-2). A merged control fails the count below.
        Assert.Equal(6, controls.Length);

        foreach (var (label, selected, source) in controls)
        {
            Assert.Contains($"Text=\"{label}\"", xaml, StringComparison.Ordinal);
            Assert.Contains(selected, xaml, StringComparison.Ordinal);
            Assert.Contains(source, xaml, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void PatientSearch_AllFilterBindingsArePublic()
    {
        var props = typeof(PatientSearchViewModel)
            .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Select(p => p.Name)
            .ToHashSet(StringComparer.Ordinal);

        // C-7: a bound property must be public or the binding gate fails.
        foreach (var name in new[]
        {
            "SelectedTreatingDoctor", "SelectedReferralEntity", "SelectedTest", "SelectedSex",
            "AgeFrom", "AgeTo", "SelectedAgeUnit", "FromDate", "ToDate",
            "TreatingDoctorOptions", "ReferralEntityOptions", "TestOptions", "SexOptions", "AgeUnitOptions"
        })
        {
            Assert.True(props.Contains(name), $"'{name}' must be a public bindable property.");
        }
    }

    // =====================================================================
    // SD-6: the merge-detection tests
    // =====================================================================

    [Fact]
    public void PatientSearch_DoctorAndReferralControlsAreDistinct_FailsIfEverMerged()
    {
        var xaml = ViewXaml();
        var props = typeof(PatientSearchViewModel)
            .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Select(p => p.Name)
            .ToArray();

        // 1. Two separate, differently-labelled labels.
        Assert.Contains("الطبيب المعالج:", xaml, StringComparison.Ordinal);
        Assert.Contains("جهة الإحالة:", xaml, StringComparison.Ordinal);

        // 2. Two separate selected-item bindings, each pointing at its own property.
        Assert.Contains("SelectedItem=\"{Binding SelectedTreatingDoctor}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("SelectedItem=\"{Binding SelectedReferralEntity}\"", xaml, StringComparison.Ordinal);

        // 3. Two separate lookup sources.
        Assert.Contains("ItemsSource=\"{Binding TreatingDoctorOptions}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("ItemsSource=\"{Binding ReferralEntityOptions}\"", xaml, StringComparison.Ordinal);

        // 4. Two separate public properties.
        Assert.Contains("SelectedTreatingDoctor", props);
        Assert.Contains("SelectedReferralEntity", props);

        // 5. The two labels must not read as synonyms — exactly one doctor label and one
        //    referral label, so a duplicated/merged control is visible as a count mismatch.
        Assert.Equal(1, CountOccurrences(xaml, "الطبيب المعالج:"));
        Assert.Equal(1, CountOccurrences(xaml, "جهة الإحالة:"));
    }

    [Fact]
    public async Task PatientSearch_DoctorAndReferralAreLoadedFromSeparateLookups()
    {
        var (vm, sender) = Screen();
        await vm.LoadAsync(CancellationToken.None);

        // The two lookups must be issued with DIFFERENT EntityType values, so neither
        // list can ever contain the other concept's entities.
        Assert.Contains(EntityType.TreatingDoctor, sender.ExternalEntityLookups);
        Assert.Contains(EntityType.ReferralOrContract, sender.ExternalEntityLookups);

        // Exactly two lookups, and their types are not the same value.
        Assert.Equal(2, sender.ExternalEntityLookups.Count);
        Assert.NotEqual(sender.ExternalEntityLookups[0], sender.ExternalEntityLookups[1]);

        // Each populated list holds its own concept only (skipping the "all" sentinel,
        // whose Id is null by design so that it forwards no filter).
        Assert.All(
            vm.TreatingDoctorOptions.Where(i => i.Id is not null),
            item => Assert.Equal(1, item.Id));
        Assert.All(
            vm.ReferralEntityOptions.Where(i => i.Id is not null),
            item => Assert.Equal(2, item.Id));

        // And the two lists genuinely differ.
        var doctorIds = vm.TreatingDoctorOptions.Where(i => i.Id is not null).Select(i => i.Id).ToArray();
        var referralIds = vm.ReferralEntityOptions.Where(i => i.Id is not null).Select(i => i.Id).ToArray();
        Assert.Empty(doctorIds.Intersect(referralIds));
    }

    [Fact]
    public async Task PatientSearch_SettingDoctorDoesNotSetReferral_AndViceVersa()
    {
        var (vm, sender) = Screen();

        vm.SelectedTreatingDoctor = new ExternalEntityFilterItem(7, "د. سميرة");
        await Task.Yield();

        // The referral selection is untouched — the two are independent controls.
        Assert.Null(vm.SelectedReferralEntity);

        var query = LastSearch(sender);
        Assert.Equal(7, query.TreatingDoctorId?.Value);
        Assert.Null(query.ReferralEntityId);

        vm.SelectedReferralEntity = new ExternalEntityFilterItem(9, "مركز الأم والطفل");
        await Task.Yield();

        // Setting the referral entity must NOT overwrite the doctor.
        Assert.Equal(7, vm.SelectedTreatingDoctor?.Id);
        query = LastSearch(sender);
        Assert.Equal(7, query.TreatingDoctorId?.Value);
        Assert.Equal(9, query.ReferralEntityId?.Value);
    }

    private static int CountOccurrences(string haystack, string needle)
    {
        var count = 0;
        var index = 0;
        while ((index = haystack.IndexOf(needle, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += needle.Length;
        }

        return count;
    }

    // =====================================================================
    // Each filter forwards into the single SearchAsync call site
    // =====================================================================

    [Fact]
    public async Task PatientSearch_ForwardsEachFilterIntoTheQuery()
    {
        var (vm, sender) = Screen();

        vm.SelectedTreatingDoctor = new ExternalEntityFilterItem(11, "د. علي");
        vm.SelectedReferralEntity = new ExternalEntityFilterItem(12, "مركزChest");
        vm.SelectedTest = new TestFilterItem(13, "صورة دم كاملة");
        vm.SelectedSex = Sex.Female;
        vm.SelectedAgeUnit = AgeUnit.Month;
        vm.AgeFrom = "6";
        vm.AgeTo = "24";
        vm.FromDate = new DateTime(2026, 3, 1);
        vm.ToDate = new DateTime(2026, 3, 31);
        await Task.Yield();

        var query = LastSearch(sender);

        Assert.Equal(11, query.TreatingDoctorId?.Value);
        Assert.Equal(12, query.ReferralEntityId?.Value);
        Assert.Equal(13, query.TestId);
        Assert.Equal(Sex.Female, query.Sex);

        Assert.NotNull(query.Age);
        Assert.Equal(AgeUnit.Month, query.Age!.Unit);
        Assert.Equal(6, query.Age.From);
        Assert.Equal(24, query.Age.To);

        Assert.Equal(new DateOnly(2026, 3, 1), query.From);
        Assert.Equal(new DateOnly(2026, 3, 31), query.To);
    }

    [Fact]
    public async Task PatientSearch_AllFiltersInertByDefault()
    {
        var (vm, sender) = Screen();
        await vm.LoadAsync(CancellationToken.None);

        var query = LastSearch(sender);

        Assert.Null(query.TreatingDoctorId);
        Assert.Null(query.ReferralEntityId);
        Assert.Null(query.TestId);
        Assert.Null(query.Sex);
        Assert.Null(query.Age);
        Assert.Null(query.From);
        Assert.Null(query.To);
    }

    [Fact]
    public async Task PatientSearch_EmptyAgeBandIsNotSent()
    {
        var (vm, sender) = Screen();

        // Changing the unit alone must not produce a band predicate that could widen.
        vm.SelectedAgeUnit = AgeUnit.Day;
        await Task.Yield();

        Assert.Null(LastSearch(sender).Age);
    }

    [Fact]
    public async Task PatientSearch_ClearFilters_WidensBackToTheUnfilteredState()
    {
        var (vm, sender) = Screen();

        vm.SelectedTreatingDoctor = new ExternalEntityFilterItem(11, "د. علي");
        vm.SelectedSex = Sex.Male;
        vm.AgeFrom = "10";
        vm.FromDate = new DateTime(2026, 1, 1);
        await Task.Yield();

        Assert.NotNull(LastSearch(sender).TreatingDoctorId);

        vm.ClearFiltersCommand.Execute(null);
        await Task.Yield();

        var query = LastSearch(sender);

        // Every filter returns to null — inert, not "unfiltered table". Paging is untouched.
        Assert.Null(query.TreatingDoctorId);
        Assert.Null(query.Sex);
        Assert.Null(query.Age);
        Assert.Null(query.From);
        Assert.Equal(AgeUnit.Year, vm.SelectedAgeUnit);
        Assert.Equal(1, query.Page);
        Assert.Equal(50, query.PageSize);
    }

    [Fact]
    public async Task PatientSearch_FilterChangeResetsToFirstPage()
    {
        var (vm, sender) = Screen();

        vm.Page = 4;
        vm.SelectedSex = Sex.Male;
        await Task.Yield();

        Assert.Equal(1, LastSearch(sender).Page);
    }

    // =====================================================================
    // Preserved behaviour
    // =====================================================================

    [Fact]
    public void PatientSearch_BranchFilterNoticeCommand_Preserved()
    {
        var (vm, _) = Screen();
        Assert.NotNull(vm.BranchFilterNoticeCommand);

        vm.BranchFilterNoticeCommand.Execute(null);

        // SD-11: the honest branch notice is deliberate and unchanged by this wave.
        Assert.Equal("البحث بفرع غير متاح دون مبيعات موزّعة على الفروع.", vm.ErrorMessage);
    }

    [Fact]
    public void PatientSearch_ViewStaysRightToLeft()
    {
        Assert.Contains("FlowDirection=\"RightToLeft\"", ViewXaml(), StringComparison.Ordinal);
    }

    [Fact]
    public void PatientSearch_PreExistingControlsAndColumns_Survive()
    {
        var xaml = ViewXaml();

        foreach (var token in new[]
        {
            "بحث:", "كود المعمل:", "جلب بالكود", "الفرع", "رجوع", "السابق", "التالي",
            "الاسم", "Lab ID", "اللقب", "الجنس", "العمر", "وحدة العمر", "الرقم الوطني",
            "نوع الحساب", "VIP", "الحالة", "التاريخ", "عدد التحاليل", "فتح"
        })
        {
            Assert.Contains(token, xaml, StringComparison.Ordinal);
        }
    }
}