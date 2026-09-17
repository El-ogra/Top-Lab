using System.Collections.ObjectModel;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TopLab.Application.Features.ExternalEntities.Commands.DeleteExternalEntity;
using TopLab.Application.Features.ExternalEntities.Common;
using TopLab.Application.Features.ExternalEntities.Queries.SearchExternalEntities;
using TopLab.Domain.Common.Enums;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Views.External;

namespace TopLab.Presentation.ViewModels.External;

/// <summary>
/// M14 external-entities list (S-02 Slice 6, D3): text search + the three
/// confirmed EntityType values + grid. No new shell button.
/// </summary>
public sealed class ExternalEntitiesViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly IDialogService _dialogs;
    private readonly ResultErrorPresenter _presenter;
    private readonly IServiceProvider _services;

    private string _searchTerm = string.Empty;
    private EntityTypeOption _selectedTypeFilter;
    private ObservableCollection<ExternalEntityListItemDto> _results = new();
    private bool _isBusy;
    private string _errorMessage = string.Empty;
    private string _statusMessage = string.Empty;

    public ExternalEntitiesViewModel(
        ISender mediator,
        IDialogService dialogs,
        ResultErrorPresenter presenter,
        IServiceProvider services)
    {
        _mediator = mediator;
        _dialogs = dialogs;
        _presenter = presenter;
        _services = services;

        TypeFilters = new List<EntityTypeOption>
        {
            new(null, "الكل"),
            new(EntityType.TreatingDoctor, EntityTypeOption.LabelFor(EntityType.TreatingDoctor)),
            new(EntityType.ReferralOrContract, EntityTypeOption.LabelFor(EntityType.ReferralOrContract)),
            new(EntityType.PartnerLab, EntityTypeOption.LabelFor(EntityType.PartnerLab))
        };
        _selectedTypeFilter = TypeFilters[0];

        LoadEntitiesCommand = new AsyncRelayCommand(_ => LoadAsync());
        OpenCreateCommand = new AsyncRelayCommand(_ => OpenCreateAsync());
        OpenEditCommand = new AsyncRelayCommand((p, _) => OpenEditAsync(p as ExternalEntityListItemDto));
        DeleteCommand = new AsyncRelayCommand((p, _) => DeleteAsync(p as ExternalEntityListItemDto));
    }

    public IReadOnlyList<EntityTypeOption> TypeFilters { get; }

    public string SearchTerm
    {
        get => _searchTerm;
        set => SetProperty(ref _searchTerm, value);
    }

    public EntityTypeOption SelectedTypeFilter
    {
        get => _selectedTypeFilter;
        set => SetProperty(ref _selectedTypeFilter, value);
    }

    public ObservableCollection<ExternalEntityListItemDto> Results
    {
        get => _results;
        private set
        {
            if (SetProperty(ref _results, value))
            {
                OnPropertyChanged(nameof(ShowEmpty));
            }
        }
    }

    public bool ShowEmpty => Results.Count == 0;
    public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }
    public string ErrorMessage { get => _errorMessage; private set => SetProperty(ref _errorMessage, value); }
    public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }

    public AsyncRelayCommand LoadEntitiesCommand { get; }
    public AsyncRelayCommand OpenCreateCommand { get; }
    public AsyncRelayCommand OpenEditCommand { get; }
    public AsyncRelayCommand DeleteCommand { get; }

    public async Task LoadAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var result = await _mediator.Send(new SearchExternalEntitiesQuery(
                SelectedTypeFilter.Value,
                string.IsNullOrWhiteSpace(SearchTerm) ? null : SearchTerm.Trim(),
                Page: 1,
                PageSize: 100));
            if (result.IsSuccess && result.Value is not null)
            {
                Results = new ObservableCollection<ExternalEntityListItemDto>(result.Value);
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
        var vm = _services.GetRequiredService<ExternalEntityEditorViewModel>();
        vm.InitializeNew(SelectedTypeFilter.Value);
        var window = new ExternalEntityEditorWindow(vm)
        {
            Owner = System.Windows.Application.Current?.MainWindow
        };
        window.ShowDialog();
        await LoadAsync();
    }

    private async Task OpenEditAsync(ExternalEntityListItemDto? dto)
    {
        if (dto is null)
        {
            return;
        }

        var vm = _services.GetRequiredService<ExternalEntityEditorViewModel>();
        vm.InitializeEdit(dto.Id);
        await vm.LoadDetailAsync();
        var window = new ExternalEntityEditorWindow(vm)
        {
            Owner = System.Windows.Application.Current?.MainWindow
        };
        window.ShowDialog();
        await LoadAsync();
    }

    private async Task DeleteAsync(ExternalEntityListItemDto? dto)
    {
        if (dto is null)
        {
            return;
        }

        bool confirm = await _dialogs.ShowConfirmationAsync(
            "حذف الجهة",
            $"سيتم حذف الجهة \"{dto.Name}\" — متابعة؟");
        if (!confirm)
        {
            return;
        }

        var result = await _mediator.Send(new DeleteExternalEntityCommand(dto.Id));
        if (!result.IsSuccess)
        {
            ErrorMessage = result.Error is not null ? _presenter.Present(result.Error) : ErrorMessage;
            return;
        }

        StatusMessage = "تم حذف الجهة.";
        await LoadAsync();
    }
}
