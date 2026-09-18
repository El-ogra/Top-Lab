using System.Collections.ObjectModel;
using MediatR;
using TopLab.Application.Features.ProfileResults.Common;
using TopLab.Application.Features.ProfileResults.Queries.GetProfileResultAmendments;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.ErrorPresentation;

namespace TopLab.Presentation.ViewModels.Patients;

/// <summary>
/// S-04 Slice 6: Amendments log dialog (P4) — shows amendment history for a profile result item.
/// Gated on PT_AUDIT_ACCESS — button visible, denial surfaces verbatim.
/// </summary>
public sealed class AmendmentsLogViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly ResultErrorPresenter _presenter;

    private ObservableCollection<ProfileAmendmentDto> _amendments = new();
    private bool _isBusy;
    private string _errorMessage = string.Empty;

    public AmendmentsLogViewModel(ISender mediator, ResultErrorPresenter presenter)
    {
        _mediator = mediator;
        _presenter = presenter;
    }

    public ObservableCollection<ProfileAmendmentDto> Amendments
    {
        get => _amendments;
        private set
        {
            if (SetProperty(ref _amendments, value))
            {
                OnPropertyChanged(nameof(HasAmendments));
                OnPropertyChanged(nameof(ShowEmpty));
            }
        }
    }

    public bool HasAmendments => Amendments.Count > 0;
    public bool ShowEmpty => Amendments.Count == 0 && !IsBusy && string.IsNullOrEmpty(ErrorMessage);

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

    public async Task LoadAsync(int profileResultItemId, CancellationToken cancellationToken = default)
    {
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _mediator.Send(new GetProfileResultAmendmentsQuery(profileResultItemId), cancellationToken);
            if (result.IsSuccess && result.Value is not null)
            {
                Amendments = new ObservableCollection<ProfileAmendmentDto>(result.Value);
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
