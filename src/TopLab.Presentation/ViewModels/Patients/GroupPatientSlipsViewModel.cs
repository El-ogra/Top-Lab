using System.Collections.ObjectModel;
using MediatR;
using TopLab.Application.Features.ResultDelivery.Commands.PrintGroupPatientSlips;
using TopLab.Application.Features.ResultDelivery.Queries.GetUndeliveredResults;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Common.Navigation;

namespace TopLab.Presentation.ViewModels.Patients;

/// <summary>
/// Selectable row wrapper for the group-slip patient list (Phase 6, GAP-013).
/// Mirrors <c>DeliveryGridRow</c> / <c>CombinableTestRow</c>.
/// </summary>
public sealed class GroupSlipPatientRow : ViewModelBase
{
    private bool _isSelected;

    public int PatientId { get; init; }
    public string PatientFullName { get; init; } = string.Empty;
    public string? LabId { get; init; }
    public DateTime RegistrationDateUtc { get; init; }
    public int UndeliveredCount { get; init; }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}

/// <summary>
/// Group-patient report slip screen (Phase 6, GAP-013): pick a group of patients
/// from the follow-up/delivery list and generate one independently archivable
/// slip per patient.
/// Patient selection reuses the delivery surface's own list query
/// (<c>GetUndeliveredResultsQuery</c> — period + paging) rather than introducing a
/// second patient-selection concept, and the selected ids flow to
/// <c>PrintGroupPatientSlipsCommand</c> through the established explicit-id-list
/// mechanism.
/// </summary>
public sealed class GroupPatientSlipsViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly ResultErrorPresenter _presenter;
    private readonly INavigationService _navigation;

    private DateOnly? _from = DateOnly.FromDateTime(DateTime.UtcNow);
    private DateOnly? _to = DateOnly.FromDateTime(DateTime.UtcNow);
    private ObservableCollection<GroupSlipPatientRow> _items = new();
    private bool _isBusy;
    private string _errorMessage = string.Empty;
    private string _statusMessage = string.Empty;

    public GroupPatientSlipsViewModel(
        ISender mediator,
        ResultErrorPresenter presenter,
        INavigationService navigation)
    {
        _mediator = mediator;
        _presenter = presenter;
        _navigation = navigation;

        LoadCommand = new AsyncRelayCommand(async (_, ct) => await LoadAsync(ct));
        GenerateCommand = new AsyncRelayCommand(async (_, ct) => await GenerateAsync(ct));
        SelectAllCommand = new RelayCommand(_ => SetAllSelected(true));
        ClearSelectionCommand = new RelayCommand(_ => SetAllSelected(false));
        BackCommand = new RelayCommand(_ => _navigation.NavigateTo<ResultDeliveryViewModel>());
    }

    public DateOnly? From
    {
        get => _from;
        set => SetProperty(ref _from, value);
    }

    public DateOnly? To
    {
        get => _to;
        set => SetProperty(ref _to, value);
    }

    public ObservableCollection<GroupSlipPatientRow> Items
    {
        get => _items;
        private set
        {
            if (SetProperty(ref _items, value))
            {
                OnPropertyChanged(nameof(HasRows));
                OnPropertyChanged(nameof(ShowEmpty));
                OnPropertyChanged(nameof(SelectedCount));
            }
        }
    }

    public bool HasRows => Items.Count > 0;

    public bool ShowEmpty => Items.Count == 0 && !IsBusy && string.IsNullOrEmpty(ErrorMessage);

    public int SelectedCount => Items.Count(r => r.IsSelected);

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

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public AsyncRelayCommand LoadCommand { get; }
    public AsyncRelayCommand GenerateCommand { get; }
    public RelayCommand SelectAllCommand { get; }
    public RelayCommand ClearSelectionCommand { get; }
    public RelayCommand BackCommand { get; }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;

        try
        {
            var result = await _mediator.Send(
                new GetUndeliveredResultsQuery(From, To, 1, 500), cancellationToken);

            if (result.IsSuccess && result.Value is not null)
            {
                var rows = new ObservableCollection<GroupSlipPatientRow>();
                foreach (var item in result.Value)
                {
                    rows.Add(new GroupSlipPatientRow
                    {
                        PatientId = item.PatientId,
                        PatientFullName = item.PatientFullName,
                        LabId = item.LabId,
                        RegistrationDateUtc = item.RegistrationDateUtc,
                        UndeliveredCount = item.UndeliveredCount
                    });
                }

                Items = rows;
            }
            else if (result.Error is not null)
            {
                ErrorMessage = _presenter.Present(result.Error);
                Items = new ObservableCollection<GroupSlipPatientRow>();
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void SetAllSelected(bool selected)
    {
        foreach (var row in Items)
        {
            row.IsSelected = selected;
        }

        OnPropertyChanged(nameof(SelectedCount));
    }

    private async Task GenerateAsync(CancellationToken cancellationToken)
    {
        var selectedIds = Items.Where(r => r.IsSelected).Select(r => r.PatientId).ToList();
        if (selectedIds.Count == 0)
        {
            ErrorMessage = "حدد مريضاً واحداً على الأقل لطباعة القصاصات.";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;

        try
        {
            var result = await _mediator.Send(
                new PrintGroupPatientSlipsCommand(selectedIds), cancellationToken);

            if (result.IsSuccess)
            {
                StatusMessage = $"تمت طباعة {selectedIds.Count} قصاصة.";
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
