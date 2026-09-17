using System.Globalization;
using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientBilling.Commands.RecordExtraCharge;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.ErrorPresentation;

namespace TopLab.Presentation.ViewModels.Patients;

/// <summary>
/// S-03 Slice 5: extra-charge dialog VM (S-M03-4). Single decimal amount only —
/// an extra charge cannot carry a discount by domain design (no discount field).
/// Window idiom follows ExternalEntityEditorWindow (SaveAsync → bool).
/// </summary>
public sealed class ExtraChargeDialogViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly ResultErrorPresenter _presenter;
    private readonly IDialogService _dialogs;

    private int _patientId;
    private string _amountText = string.Empty;
    private string _errorMessage = string.Empty;

    public ExtraChargeDialogViewModel(
        ISender mediator,
        ResultErrorPresenter presenter,
        IDialogService dialogs)
    {
        _mediator = mediator;
        _presenter = presenter;
        _dialogs = dialogs;
    }

    /// <summary>Dialog setup idiom (cf. PresetType / SetTreatingDoctor).</summary>
    public void Setup(int patientId)
    {
        _patientId = patientId;
    }

    public string AmountText { get => _amountText; set => SetProperty(ref _amountText, value); }

    public string ErrorMessage { get => _errorMessage; private set => SetProperty(ref _errorMessage, value); }

    public async Task<bool> SaveAsync(CancellationToken cancellationToken = default)
    {
        ErrorMessage = string.Empty;

        if (!decimal.TryParse(AmountText, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) || amount <= 0)
        {
            ErrorMessage = "قيمة المبلغ غير صالحة.";
            return false;
        }

        var confirm = await _dialogs.ShowConfirmationAsync(
            "مبلغ إضافي",
            $"سيتم تسجيل مبلغ إضافي بمبلغ {amount}. هل تريد المتابعة؟");
        if (!confirm)
        {
            return false;
        }

        Result<int> result = await _mediator.Send(new RecordExtraChargeCommand(_patientId, amount), cancellationToken);
        if (!result.IsSuccess)
        {
            ErrorMessage = _presenter.Present(result.Error!);
            return false;
        }

        return true;
    }
}
