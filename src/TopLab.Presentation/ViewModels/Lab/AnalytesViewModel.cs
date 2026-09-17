using System.Collections.ObjectModel;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TopLab.Application.Features.AnalyteProfiles.Commands.DeactivateAnalyte;
using TopLab.Application.Features.AnalyteProfiles.Common;
using TopLab.Application.Features.AnalyteProfiles.Queries.GetAnalyteDefinitions;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Views.Lab;

namespace TopLab.Presentation.ViewModels.Lab;

/// <summary>
/// Analytes tab (S-02 Slice 4): search + grid with the confirmed
/// `AnalyteDefinitionDto` columns (no unit of measure exists in code).
/// Deactivate only — no reactivate/delete commands exist (open gap).
/// </summary>
public sealed class AnalytesViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly IDialogService _dialogs;
    private readonly ResultErrorPresenter _presenter;
    private readonly IServiceProvider _services;

    private string _searchTerm = string.Empty;
    private ObservableCollection<AnalyteDefinitionDto> _analytes = new();
    private ObservableCollection<AnalyteDefinitionDto> _filtered = new();
    private bool _isBusy;
    private string _errorMessage = string.Empty;

    public AnalytesViewModel(
        ISender mediator,
        IDialogService dialogs,
        ResultErrorPresenter presenter,
        IServiceProvider services)
    {
        _mediator = mediator;
        _dialogs = dialogs;
        _presenter = presenter;
        _services = services;

        LoadAnalytesCommand = new AsyncRelayCommand(_ => LoadAsync());
        OpenCreateCommand = new AsyncRelayCommand(_ => OpenCreateAsync());
        OpenEditCommand = new AsyncRelayCommand((p, _) => OpenEditAsync(p as AnalyteDefinitionDto));
        SearchCommand = new RelayCommand(_ => ApplyFilter());
        DeactivateCommand = new AsyncRelayCommand((p, _) => DeactivateAsync(p as AnalyteDefinitionDto));
    }

    public string SearchTerm
    {
        get => _searchTerm;
        set => SetProperty(ref _searchTerm, value);
    }

    public ObservableCollection<AnalyteDefinitionDto> Filtered
    {
        get => _filtered;
        private set
        {
            if (SetProperty(ref _filtered, value))
            {
                OnPropertyChanged(nameof(ShowEmpty));
            }
        }
    }

    public bool ShowEmpty => Filtered.Count == 0;
    public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }
    public string ErrorMessage { get => _errorMessage; private set => SetProperty(ref _errorMessage, value); }

    public AsyncRelayCommand LoadAnalytesCommand { get; }
    public AsyncRelayCommand OpenCreateCommand { get; }
    public AsyncRelayCommand OpenEditCommand { get; }
    public RelayCommand SearchCommand { get; }
    public AsyncRelayCommand DeactivateCommand { get; }

    public async Task LoadAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var result = await _mediator.Send(new GetAnalyteDefinitionsQuery());
            if (result.IsSuccess && result.Value is not null)
            {
                _analytes = new ObservableCollection<AnalyteDefinitionDto>(result.Value);
                ApplyFilter();
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

    private void ApplyFilter()
    {
        if (string.IsNullOrWhiteSpace(SearchTerm))
        {
            Filtered = new ObservableCollection<AnalyteDefinitionDto>(_analytes);
            return;
        }

        string term = SearchTerm.Trim();
        Filtered = new ObservableCollection<AnalyteDefinitionDto>(
            _analytes.Where(a => a.Name.Contains(term, StringComparison.OrdinalIgnoreCase)
                || a.ReportName.Contains(term, StringComparison.OrdinalIgnoreCase)));
    }

    private async Task OpenCreateAsync()
    {
        var vm = _services.GetRequiredService<AnalyteEditorViewModel>();
        vm.InitializeNew();
        var window = new AnalyteEditorWindow(vm)
        {
            Owner = System.Windows.Application.Current?.MainWindow
        };
        window.ShowDialog();
        await LoadAsync();
    }

    private async Task OpenEditAsync(AnalyteDefinitionDto? dto)
    {
        if (dto is null)
        {
            return;
        }

        var vm = _services.GetRequiredService<AnalyteEditorViewModel>();
        vm.InitializeEdit(dto);
        var window = new AnalyteEditorWindow(vm)
        {
            Owner = System.Windows.Application.Current?.MainWindow
        };
        window.ShowDialog();
        await LoadAsync();
    }

    private async Task DeactivateAsync(AnalyteDefinitionDto? dto)
    {
        if (dto is null)
        {
            return;
        }

        bool confirm = await _dialogs.ShowConfirmationAsync(
            "تعطيل المكوّن",
            $"سيتم تعطيل المكوّن \"{dto.Name}\" — متابعة؟");
        if (!confirm)
        {
            return;
        }

        var result = await _mediator.Send(new DeactivateAnalyteCommand(dto.AnalyteId));
        if (!result.IsSuccess && result.Error is not null)
        {
            ErrorMessage = _presenter.Present(result.Error);
            return;
        }

        await LoadAsync();
    }
}
