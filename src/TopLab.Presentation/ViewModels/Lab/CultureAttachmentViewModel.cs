using System.Collections.ObjectModel;
using MediatR;
using TopLab.Application.Features.CultureAndAntibiotics.Commands.AttachAntibioticToCulture;
using TopLab.Application.Features.CultureAndAntibiotics.Commands.DetachAntibioticFromCulture;
using TopLab.Application.Features.CultureAndAntibiotics.Common;
using TopLab.Application.Features.CultureAndAntibiotics.Queries.GetAntibiotics;
using TopLab.Application.Features.CultureAndAntibiotics.Queries.GetCultureAntibiotics;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Common;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Queries.GetTestById;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Queries.SearchTestCatalog;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.ErrorPresentation;

namespace TopLab.Presentation.ViewModels.Lab;

/// <summary>
/// Culture↔antibiotic attachment tab (S-02 Slice 5, D9): immediate per-item
/// attach/detach with live counter; both lists + counter reload after each
/// successful operation. No bulk save.
/// </summary>
public sealed class CultureAttachmentViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly ResultErrorPresenter _presenter;

    private ObservableCollection<TestSummaryDto> _cultureTests = new();
    private TestSummaryDto? _selectedCulture;
    private ObservableCollection<AttachedAntibioticDto> _attached = new();
    private ObservableCollection<AntibioticDto> _available = new();
    private int _attachedCount;
    private bool _isBusy;
    private string _errorMessage = string.Empty;

    public CultureAttachmentViewModel(ISender mediator, ResultErrorPresenter presenter)
    {
        _mediator = mediator;
        _presenter = presenter;

        LoadAttachmentCommand = new AsyncRelayCommand(_ => LoadAsync());
        AttachCommand = new AsyncRelayCommand((p, _) => AttachAsync(p as AntibioticDto));
        DetachCommand = new AsyncRelayCommand((p, _) => DetachAsync(p as AttachedAntibioticDto));
    }

    public ObservableCollection<TestSummaryDto> CultureTests
    {
        get => _cultureTests;
        private set => SetProperty(ref _cultureTests, value);
    }

    public TestSummaryDto? SelectedCulture
    {
        get => _selectedCulture;
        set
        {
            if (SetProperty(ref _selectedCulture, value))
            {
                _ = ReloadListsAsync();
            }
        }
    }

    public ObservableCollection<AttachedAntibioticDto> Attached
    {
        get => _attached;
        private set => SetProperty(ref _attached, value);
    }

    public ObservableCollection<AntibioticDto> Available
    {
        get => _available;
        private set => SetProperty(ref _available, value);
    }

    public int AttachedCount { get => _attachedCount; private set => SetProperty(ref _attachedCount, value); }
    public bool HasSelectedCulture => SelectedCulture is not null;
    public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }
    public string ErrorMessage { get => _errorMessage; private set => SetProperty(ref _errorMessage, value); }

    public AsyncRelayCommand LoadAttachmentCommand { get; }
    public AsyncRelayCommand AttachCommand { get; }
    public AsyncRelayCommand DetachCommand { get; }

    public async Task LoadAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var catalogResult = await _mediator.Send(new SearchTestCatalogQuery(null, null, IncludeInactive: false));
            var cultures = new List<TestSummaryDto>();
            if (catalogResult.IsSuccess && catalogResult.Value is not null)
            {
                foreach (var summary in catalogResult.Value)
                {
                    var detail = await _mediator.Send(new GetTestByIdQuery(summary.Id));
                    if (detail.IsSuccess && detail.Value is not null && detail.Value.IsCultureType)
                    {
                        cultures.Add(summary);
                    }
                }
            }
            else if (catalogResult.Error is not null)
            {
                ErrorMessage = _presenter.Present(catalogResult.Error);
                return;
            }

            CultureTests = new ObservableCollection<TestSummaryDto>(cultures);
            SelectedCulture = null;
            Attached = new ObservableCollection<AttachedAntibioticDto>();
            Available = new ObservableCollection<AntibioticDto>();
            AttachedCount = 0;
            OnPropertyChanged(nameof(HasSelectedCulture));
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ReloadListsAsync()
    {
        OnPropertyChanged(nameof(HasSelectedCulture));
        if (SelectedCulture is null)
        {
            Attached = new ObservableCollection<AttachedAntibioticDto>();
            Available = new ObservableCollection<AntibioticDto>();
            AttachedCount = 0;
            return;
        }

        IsBusy = true;
        try
        {
            var attachedResult = await _mediator.Send(new GetCultureAntibioticsQuery(SelectedCulture.Id));
            HashSet<int> attachedIds = new();
            if (attachedResult.IsSuccess && attachedResult.Value is not null)
            {
                Attached = new ObservableCollection<AttachedAntibioticDto>(attachedResult.Value.Antibiotics);
                AttachedCount = attachedResult.Value.AttachedCount;
                attachedIds = attachedResult.Value.Antibiotics.Select(a => a.AntibioticId).ToHashSet();
            }
            else if (attachedResult.Error is not null)
            {
                ErrorMessage = _presenter.Present(attachedResult.Error);
                return;
            }

            var dictionaryResult = await _mediator.Send(new GetAntibioticsQuery(null));
            if (dictionaryResult.IsSuccess && dictionaryResult.Value is not null)
            {
                Available = new ObservableCollection<AntibioticDto>(
                    dictionaryResult.Value.Where(a => !attachedIds.Contains(a.Id)));
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task AttachAsync(AntibioticDto? dto)
    {
        if (SelectedCulture is null || dto is null)
        {
            return;
        }

        var result = await _mediator.Send(new AttachAntibioticToCultureCommand(SelectedCulture.Id, dto.Id));
        if (!result.IsSuccess)
        {
            ErrorMessage = result.Error is not null ? _presenter.Present(result.Error) : ErrorMessage;
            return;
        }

        ErrorMessage = string.Empty;
        await ReloadListsAsync();
    }

    private async Task DetachAsync(AttachedAntibioticDto? dto)
    {
        if (SelectedCulture is null || dto is null)
        {
            return;
        }

        var result = await _mediator.Send(new DetachAntibioticFromCultureCommand(SelectedCulture.Id, dto.AntibioticId));
        if (!result.IsSuccess)
        {
            ErrorMessage = result.Error is not null ? _presenter.Present(result.Error) : ErrorMessage;
            return;
        }

        ErrorMessage = string.Empty;
        await ReloadListsAsync();
    }
}
