using System.Collections.ObjectModel;
using System.Globalization;
using MediatR;
using TopLab.Application.Features.SampleCollection.Common;
using TopLab.Application.Features.SampleCollection.Queries.GetPatientsWithUncollectedSamples;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.ErrorPresentation;

namespace TopLab.Presentation.ViewModels.Lab;

/// <summary>
/// S-04 Slice 0: Sample Collection worklist tab (S1) inside the «المعمل» hub.
/// Read-only grid of today's (or selected day's) patients with undrawn samples.
/// Open-patient affordance ships disabled — wired in Slice 1.
/// </summary>
public sealed class SampleCollectionViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly ResultErrorPresenter _presenter;

    private DateTime? _dayDateTime = DateTime.UtcNow.Date;
    private int _page = 1;
    private int _pageSize = 100;
    private ObservableCollection<PatientWithUndrawnTestsDto> _items = new();
    private bool _isBusy;
    private string _errorMessage = string.Empty;

    public SampleCollectionViewModel(
        ISender mediator,
        IDialogService dialogs,
        ResultErrorPresenter presenter)
    {
        _mediator = mediator;
        _presenter = presenter;

        RefreshCommand = new AsyncRelayCommand(_ => LoadAsync());
        OpenPatientCommand = new AsyncRelayCommand(async _ => await Task.CompletedTask);
    }

    public DateTime? DayDateTime
    {
        get => _dayDateTime;
        set => SetProperty(ref _dayDateTime, value);
    }

    public int Page
    {
        get => _page;
        set => SetProperty(ref _page, value);
    }

    public int PageSize
    {
        get => _pageSize;
        set => SetProperty(ref _pageSize, value);
    }

    public ObservableCollection<PatientWithUndrawnTestsDto> Items
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
    public bool ShowEmpty => Items.Count == 0;

    public bool OpenPatientEnabled { get; set; } = false;

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

    public AsyncRelayCommand RefreshCommand { get; }
    public AsyncRelayCommand OpenPatientCommand { get; }

    public async Task LoadAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var result = await _mediator.Send(new GetPatientsWithUncollectedSamplesQuery(
                ToDateOnly(DayDateTime), Page, PageSize));
            if (result.IsSuccess && result.Value is not null)
            {
                Items = new ObservableCollection<PatientWithUndrawnTestsDto>(result.Value);
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

    private static DateOnly? ToDateOnly(DateTime? value)
        => value.HasValue ? DateOnly.FromDateTime(value.Value) : null;
}
