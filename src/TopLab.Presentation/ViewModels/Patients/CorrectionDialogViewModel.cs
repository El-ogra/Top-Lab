using System.Globalization;
using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientBilling.Commands.RecordCorrection;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.ErrorPresentation;

namespace TopLab.Presentation.ViewModels.Patients;

/// <summary>
/// S-03 Slice 5: correction dialog VM (S-M03-3). Single decimal amount only —
/// the command signature has no reason field (gap documented, not filled).
/// Positive amount is a credit reducing the balance (handler-settled sign).
/// Window idiom follows ExternalEntityEditorWindow (SaveAsync → bool).
/// </summary>
public sealed class CorrectionDialogViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly ResultErrorPresenter _presenter;
    private readonly IDialogService _dialogs;

    private int _patientId;
    private string _amountText = string.Empty;
    private string _errorMessage = string.Empty;

    public CorrectionDialogViewModel(
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
            "قيد تصحيحي",
            $"سيتم تسجيل قيد تصحيحي بمبلغ {amount}. هل تريد المتابعة؟");
        if (!confirm)
        {
            return false;
        }

        Result<int> result = await _mediator.Send(new RecordCorrectionCommand(_patientId, amount), cancellationToken);
        if (!result.IsSuccess)
        {
            ErrorMessage = _presenter.Present(result.Error!);
            return false;
        }

        return true;
    }
}
