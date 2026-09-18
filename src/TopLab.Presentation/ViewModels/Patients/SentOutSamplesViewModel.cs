using System.Collections.ObjectModel;
using MediatR;
using TopLab.Application.Features.ExternalEntities.Queries.SearchExternalEntities;
using TopLab.Application.Features.SentOutSamples.Common;
using TopLab.Application.Features.SentOutSamples.Queries.GetSentOutLabAccount;
using TopLab.Application.Features.SentOutSamples.Queries.GetSentOutSamples;
using TopLab.Domain.Common.Enums;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Common.Navigation;

namespace TopLab.Presentation.ViewModels.Patients;

/// <summary>
/// S-05 Slice 5: Sent-out samples list screen (M16).
/// Lab filter ComboBox sourced from SearchExternalEntitiesQuery(EntityType.PartnerLab) — D10 closed.
/// </summary>
public sealed class SentOutSamplesViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly ResultErrorPresenter _presenter;
    private readonly INavigationService _navigation;

    private DateOnly? _from = DateOnly.FromDateTime(DateTime.UtcNow);
    private DateOnly? _to = DateOnly.FromDateTime(DateTime.UtcNow);
    private int? _externalLabEntityId;
    private int _page = 1;
    private int _pageSize = 50;
    private ObservableCollection<SentOutSampleDto> _items = new();
    private ObservableCollection<LabFilterItem> _labFilterItems = new();
    private SentOutSampleDto? _selectedItem;
    private bool _isBusy;
    private string _errorMessage = string.Empty;

    public SentOutSamplesViewModel(ISender mediator, ResultErrorPresenter presenter, INavigationService navigation)
    {
        _mediator = mediator;
        _presenter = presenter;
        _navigation = navigation;

        LoadCommand = new AsyncRelayCommand(async (_, ct) => await LoadAsync(ct));
        OpenLabAccountCommand = new RelayCommand(param => OpenLabAccount(param as SentOutSampleDto));
        BackCommand = new RelayCommand(_ => _navigation.NavigateTo<PatientVisitHistoryViewModel>());
    }

    public DateOnly? From { get => _from; set => SetProperty(ref _from, value); }
    public DateOnly? To { get => _to; set => SetProperty(ref _to, value); }

    public int? ExternalLabEntityId
    {
        get => _externalLabEntityId;
        set
        {
            if (SetProperty(ref _externalLabEntityId, value))
            {
                _ = LoadAsync();
            }
        }
    }

    public int Page { get => _page; set => SetProperty(ref _page, value); }
    public int PageSize { get => _pageSize; set => SetProperty(ref _pageSize, value); }

    public ObservableCollection<LabFilterItem> LabFilterItems
    {
        get => _labFilterItems;
        private set => SetProperty(ref _labFilterItems, value);
    }

    public SentOutSampleDto? SelectedItem
    {
        get => _selectedItem;
        set => SetProperty(ref _selectedItem, value);
    }

    public ObservableCollection<SentOutSampleDto> Items
    {
        get => _items;
        private set
        {
            if (SetProperty(ref _items, value))
            {
                OnPropertyChanged(nameof(HasResults));
                OnPropertyChanged(nameof(ShowEmpty));
            }
        }
    }

    public bool HasResults => Items.Count > 0;
    public bool ShowEmpty => Items.Count == 0 && !IsBusy && string.IsNullOrEmpty(ErrorMessage);

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

    public AsyncRelayCommand LoadCommand { get; }
    public RelayCommand OpenLabAccountCommand { get; }
    public RelayCommand BackCommand { get; }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            // D10: load lab filter items from SearchExternalEntitiesQuery with EntityType.PartnerLab
            if (LabFilterItems.Count == 0)
            {
                var labsResult = await _mediator.Send(
                    new SearchExternalEntitiesQuery(EntityType.PartnerLab, null, 1, 100), cancellationToken);
                if (labsResult.IsSuccess && labsResult.Value is not null)
                {
                    var filterItems = new ObservableCollection<LabFilterItem>
                    {
                        new(null, "الكل")
                    };
                    foreach (var lab in labsResult.Value)
                    {
                        filterItems.Add(new LabFilterItem(lab.Id, lab.Name));
                    }

                    LabFilterItems = filterItems;
                }
            }

            var result = await _mediator.Send(
                new GetSentOutSamplesQuery(From, To, ExternalLabEntityId, Page, PageSize), cancellationToken);

            if (result.IsSuccess && result.Value is not null)
            {
                Items = new ObservableCollection<SentOutSampleDto>(result.Value);
            }
            else if (result.Error is not null)
            {
                ErrorMessage = _presenter.Present(result.Error);
                Items = new ObservableCollection<SentOutSampleDto>();
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void OpenLabAccount(SentOutSampleDto? sample)
    {
        if (sample is null || sample.ExternalLabEntityId <= 0)
        {
            return;
        }

        _navigation.NavigateTo<SentOutLabAccountViewModel>();
        if (_navigation.CurrentViewModel is SentOutLabAccountViewModel vm)
        {
            _ = vm.LoadAsync(sample.ExternalLabEntityId, From, To);
        }
    }
}

/// <summary>Lab filter ComboBox item (D10: Id from ExternalEntityListItemDto, Name for display).</summary>
public sealed record LabFilterItem(int? Id, string Name);
