using System.Collections.ObjectModel;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.DeactivateTest;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.ReactivateTest;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Common;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Queries.GetTestGroups;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Queries.SearchTestCatalog;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Views.Lab;

namespace TopLab.Presentation.ViewModels.Lab;

public sealed class GroupFilterOption
{
    public GroupFilterOption(int? id, string name)
    {
        Id = id;
        Name = name;
    }

    public int? Id { get; }
    public string Name { get; }
}

/// <summary>
/// M12 test catalog (S-02 Slice 3): name search + group filter + grid with
/// counter, dual empty states, and per-row edit/deactivate.
/// </summary>
public sealed class TestCatalogViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly IDialogService _dialogs;
    private readonly ResultErrorPresenter _presenter;
    private readonly IServiceProvider _services;

    private string _searchTerm = string.Empty;
    private GroupFilterOption? _selectedGroupFilter;
    private ObservableCollection<TestSummaryDto> _summaries = new();
    private int _totalCount;
    private bool _isBusy;
    private string _errorMessage = string.Empty;

    public TestCatalogViewModel(
        ISender mediator,
        IDialogService dialogs,
        ResultErrorPresenter presenter,
        IServiceProvider services)
    {
        _mediator = mediator;
        _dialogs = dialogs;
        _presenter = presenter;
        _services = services;

        GroupFilters = new ObservableCollection<GroupFilterOption>();

        LoadCatalogCommand = new AsyncRelayCommand(_ => LoadAsync());
        OpenCreateCommand = new AsyncRelayCommand(_ => OpenCreateAsync());
        OpenEditCommand = new AsyncRelayCommand((p, _) => OpenEditAsync(p as TestSummaryDto));
        ToggleActiveCommand = new AsyncRelayCommand((p, _) => ToggleActiveAsync(p as TestSummaryDto));
    }

    public ObservableCollection<GroupFilterOption> GroupFilters { get; }

    public string SearchTerm
    {
        get => _searchTerm;
        set
        {
            if (SetProperty(ref _searchTerm, value))
            {
                RefreshEmptyStates();
            }
        }
    }

    public GroupFilterOption? SelectedGroupFilter
    {
        get => _selectedGroupFilter;
        set
        {
            if (SetProperty(ref _selectedGroupFilter, value))
            {
                RefreshEmptyStates();
            }
        }
    }

    public ObservableCollection<TestSummaryDto> Summaries
    {
        get => _summaries;
        private         set
        {
            if (SetProperty(ref _summaries, value))
            {
                RefreshEmptyStates();
            }
        }
    }

    public int TotalCount
    {
        get => _totalCount;
        private set => SetProperty(ref _totalCount, value);
    }

    public bool HasResults => Summaries.Count > 0;

    public bool ShowFilteredEmpty => Summaries.Count == 0 && HasActiveFilter;

    public bool ShowCriticalEmpty => Summaries.Count == 0 && !HasActiveFilter;

    public bool IsBusy
    {
        get => _isBusy;
        private set => SetProperty(ref _isBusy, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public AsyncRelayCommand LoadCatalogCommand { get; }
    public AsyncRelayCommand OpenCreateCommand { get; }
    public AsyncRelayCommand OpenEditCommand { get; }
    public AsyncRelayCommand ToggleActiveCommand { get; }

    private bool HasActiveFilter =>
        !string.IsNullOrWhiteSpace(SearchTerm) || SelectedGroupFilter?.Id is not null;

    private void RefreshEmptyStates()
    {
        OnPropertyChanged(nameof(HasResults));
        OnPropertyChanged(nameof(ShowFilteredEmpty));
        OnPropertyChanged(nameof(ShowCriticalEmpty));
    }

    public async Task LoadAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var groupsResult = await _mediator.Send(new GetTestGroupsQuery(IncludeInactive: true));
            var filters = new List<GroupFilterOption> { new(null, "الكل") };
            if (groupsResult.IsSuccess && groupsResult.Value is not null)
            {
                filters.AddRange(groupsResult.Value.Select(g => new GroupFilterOption(g.Id, g.Name)));
            }

            GroupFilters.Clear();
            foreach (var f in filters)
            {
                GroupFilters.Add(f);
            }

            SelectedGroupFilter = GroupFilters[0];

            await SearchAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task SearchAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var result = await _mediator.Send(new SearchTestCatalogQuery(
                string.IsNullOrWhiteSpace(SearchTerm) ? null : SearchTerm.Trim(),
                SelectedGroupFilter?.Id,
                IncludeInactive: true));
            if (result.IsSuccess && result.Value is not null)
            {
                Summaries = new ObservableCollection<TestSummaryDto>(result.Value);
                TotalCount = result.Value.Count;
            }
            else if (result.Error is not null)
            {
                ErrorMessage = _presenter.Present(result.Error);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task OpenCreateAsync()
    {
        var vm = _services.GetRequiredService<TestEditorViewModel>();
        vm.InitializeNew();
        var window = new TestEditorWindow(vm)
        {
            Owner = System.Windows.Application.Current?.MainWindow
        };
        window.ShowDialog();
        await SearchAsync();
    }

    private async Task OpenEditAsync(TestSummaryDto? summary)
    {
        if (summary is null)
        {
            return;
        }

        var vm = _services.GetRequiredService<TestEditorViewModel>();
        vm.InitializeEdit(summary.Id);
        await vm.LoadDetailAsync();
        var window = new TestEditorWindow(vm)
        {
            Owner = System.Windows.Application.Current?.MainWindow
        };
        window.ShowDialog();
        await SearchAsync();
    }

    private async Task ToggleActiveAsync(TestSummaryDto? summary)
    {
        if (summary is null)
        {
            return;
        }

        if (summary.IsActive)
        {
            bool confirm = await _dialogs.ShowConfirmationAsync(
                "تعطيل التحليل",
                "هذا التحليل له نتائج سابقة؛ التعطيل يمنع طلبه مستقبلاً ولا يمس النتائج — متابعة؟");
            if (!confirm)
            {
                return;
            }

            var result = await _mediator.Send(new DeactivateTestCommand(summary.Id));
            if (!result.IsSuccess && result.Error is not null)
            {
                ErrorMessage = _presenter.Present(result.Error);
                return;
            }
        }
        else
        {
            bool confirm = await _dialogs.ShowConfirmationAsync(
                "إعادة تفعيل التحليل",
                $"سيتم إعادة تفعيل التحليل \"{summary.Name}\" — متابعة؟");
            if (!confirm)
            {
                return;
            }

            var result = await _mediator.Send(new ReactivateTestCommand(summary.Id));
            if (!result.IsSuccess && result.Error is not null)
            {
                ErrorMessage = _presenter.Present(result.Error);
                return;
            }
        }

        await SearchAsync();
    }
}
