using System.Collections.ObjectModel;
using MediatR;
using TopLab.Application.Features.ExternalEntities.Queries.SearchExternalEntities;
using TopLab.Application.Features.SentOutSamples.Commands.SendSampleOut;
using TopLab.Domain.Common.Enums;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.ErrorPresentation;

namespace TopLab.Presentation.ViewModels.Patients;

/// <summary>
/// S-05 Slice 5: Send sample out dialog (M16) — D10 closed.
/// Lab ComboBox sourced from SearchExternalEntitiesQuery(EntityType.PartnerLab).
/// </summary>
public sealed class SendSampleOutDialogViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly ResultErrorPresenter _presenter;
    private readonly IDialogService _dialogs;

    private int _patientTestId;
    private string _testName = string.Empty;
    private int? _selectedLabId;
    private string _costPriceText = string.Empty;
    private string _patientPriceText = string.Empty;
    private ObservableCollection<LabFilterItem> _labItems = new();
    private bool _isBusy;
    private string _errorMessage = string.Empty;

    public SendSampleOutDialogViewModel(ISender mediator, ResultErrorPresenter presenter, IDialogService dialogs)
    {
        _mediator = mediator;
        _presenter = presenter;
        _dialogs = dialogs;
    }

    public string TestName { get => _testName; private set => SetProperty(ref _testName, value); }

    public int? SelectedLabId
    {
        get => _selectedLabId;
        set => SetProperty(ref _selectedLabId, value);
    }

    public string CostPriceText { get => _costPriceText; set => SetProperty(ref _costPriceText, value); }
    public string PatientPriceText { get => _patientPriceText; set => SetProperty(ref _patientPriceText, value); }

    public ObservableCollection<LabFilterItem> LabItems
    {
        get => _labItems;
        private set => SetProperty(ref _labItems, value);
    }

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

    public async Task SetupAsync(int patientTestId, string testName, CancellationToken cancellationToken = default)
    {
        _patientTestId = patientTestId;
        TestName = testName;
        SelectedLabId = null;
        CostPriceText = string.Empty;
        PatientPriceText = string.Empty;
        ErrorMessage = string.Empty;
        IsBusy = true;

        try
        {
            // D10: lab ComboBox sourced from SearchExternalEntitiesQuery with EntityType.PartnerLab
            var result = await _mediator.Send(
                new SearchExternalEntitiesQuery(EntityType.PartnerLab, null, 1, 100), cancellationToken);

            if (result.IsSuccess && result.Value is not null)
            {
                var items = new ObservableCollection<LabFilterItem>();
                foreach (var lab in result.Value)
                {
                    items.Add(new LabFilterItem(lab.Id, lab.Name));
                }

                LabItems = items;
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

    public async Task<bool> SendAsync()
    {
        ErrorMessage = string.Empty;

        if (SelectedLabId is null or <= 0)
        {
            ErrorMessage = "الجهة الخارجية غير موجودة.";
            return false;
        }

        decimal? costPrice = null;
        if (!string.IsNullOrWhiteSpace(CostPriceText))
        {
            if (!decimal.TryParse(CostPriceText, out var parsed) || parsed < 0)
            {
                ErrorMessage = "السعر يجب ألا يكون سالبًا.";
                return false;
            }

            costPrice = parsed;
        }

        decimal? patientPrice = null;
        if (!string.IsNullOrWhiteSpace(PatientPriceText))
        {
            if (!decimal.TryParse(PatientPriceText, out var parsed) || parsed < 0)
            {
                ErrorMessage = "السعر يجب ألا يكون سالبًا.";
                return false;
            }

            patientPrice = parsed;
        }

        var labName = LabItems.FirstOrDefault(l => l.Id == SelectedLabId)?.Name ?? SelectedLabId.ToString();
        var confirmed = await _dialogs.ShowConfirmationAsync(
            "تأكيد الإرسال الخارجي",
            $"سيتم إرسال التحليل «{TestName}» إلى «{labName}». هل تريد المتابعة؟");
        if (!confirmed)
        {
            return false;
        }

        IsBusy = true;
        try
        {
            var result = await _mediator.Send(
                new SendSampleOutCommand(_patientTestId, SelectedLabId.Value, costPrice, patientPrice));

            if (result.IsSuccess)
            {
                return true;
            }

            if (result.Error is not null)
            {
                ErrorMessage = _presenter.Present(result.Error);
            }

            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
