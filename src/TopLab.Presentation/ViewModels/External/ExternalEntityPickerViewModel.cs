using System.Collections.ObjectModel;
using MediatR;
using TopLab.Application.Features.ExternalEntities.Common;
using TopLab.Application.Features.ExternalEntities.Queries.SearchExternalEntities;
using TopLab.Domain.Common.Enums;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.ErrorPresentation;

namespace TopLab.Presentation.ViewModels.External;

/// <summary>
/// Reusable external-entity picker dialog VM (S-02 Slice 6): same search axes
/// over a read grid; returns the selected entity to the caller.
/// </summary>
public sealed class ExternalEntityPickerViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly ResultErrorPresenter _presenter;

    private string _searchTerm = string.Empty;
    private EntityTypeOption _selectedTypeFilter;
    private ObservableCollection<ExternalEntityListItemDto> _results = new();
    private ExternalEntityListItemDto? _selectedEntity;
    private bool _isBusy;
    private string _errorMessage = string.Empty;

    public ExternalEntityPickerViewModel(ISender mediator, ResultErrorPresenter presenter)
    {
        _mediator = mediator;
        _presenter = presenter;

        TypeFilters = new List<EntityTypeOption>
        {
            new(null, "الكل"),
            new(EntityType.TreatingDoctor, EntityTypeOption.LabelFor(EntityType.TreatingDoctor)),
            new(EntityType.ReferralOrContract, EntityTypeOption.LabelFor(EntityType.ReferralOrContract)),
            new(EntityType.PartnerLab, EntityTypeOption.LabelFor(EntityType.PartnerLab))
        };
        _selectedTypeFilter = TypeFilters[0];

        SearchCommand = new AsyncRelayCommand(_ => LoadAsync());
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
        private set => SetProperty(ref _results, value);
    }

    public ExternalEntityListItemDto? SelectedEntity
    {
        get => _selectedEntity;
        set
        {
            if (SetProperty(ref _selectedEntity, value))
            {
                OnPropertyChanged(nameof(HasSelection));
            }
        }
    }

    public bool HasSelection => SelectedEntity is not null;
    public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }
    public string ErrorMessage { get => _errorMessage; private set => SetProperty(ref _errorMessage, value); }

    public AsyncRelayCommand SearchCommand { get; }

    public void PresetType(EntityType? type)
    {
        SelectedTypeFilter = TypeFilters.First(o => o.Value == type);
    }

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
}
