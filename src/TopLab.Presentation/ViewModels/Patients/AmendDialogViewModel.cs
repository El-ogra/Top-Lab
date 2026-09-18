using MediatR;
using TopLab.Application.Features.ProfileResults.Commands.AmendProfileResult;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.ErrorPresentation;

namespace TopLab.Presentation.ViewModels.Patients;

/// <summary>
/// S-04 Slice 6: Amendment dialog (P3) — amend a printed profile result item.
/// Value mandatory; reason optional but ≤ 500 chars (backend validator enforces).
/// </summary>
public sealed class AmendDialogViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly ResultErrorPresenter _presenter;

    private int _profileResultItemId;
    private string _analyteName = string.Empty;
    private string _resultValue = string.Empty;
    private string? _unit;
    private int? _flag;
    private string? _reason;
    private string _errorMessage = string.Empty;

    public AmendDialogViewModel(ISender mediator, ResultErrorPresenter presenter)
    {
        _mediator = mediator;
        _presenter = presenter;
    }

    public string AnalyteName { get => _analyteName; private set => SetProperty(ref _analyteName, value); }

    public string ResultValue
    {
        get => _resultValue;
        set => SetProperty(ref _resultValue, value);
    }

    public string? Unit
    {
        get => _unit;
        set => SetProperty(ref _unit, value);
    }

    public int? Flag
    {
        get => _flag;
        set => SetProperty(ref _flag, value);
    }

    public string? Reason
    {
        get => _reason;
        set => SetProperty(ref _reason, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public void Setup(int profileResultItemId, string analyteName, string? currentValue, string? currentUnit, int? currentFlag)
    {
        _profileResultItemId = profileResultItemId;
        AnalyteName = analyteName;
        ResultValue = currentValue ?? string.Empty;
        Unit = currentUnit;
        Flag = currentFlag;
        Reason = null;
        ErrorMessage = string.Empty;
    }

    public async Task<bool> SaveAsync()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(ResultValue))
        {
            ErrorMessage = "قيمة النتيجة مطلوبة.";
            return false;
        }

        if (Reason is { Length: > 500 })
        {
            ErrorMessage = "ملاحظة التعديل طويلة جداً.";
            return false;
        }

        var result = await _mediator.Send(new AmendProfileResultCommand(
            _profileResultItemId,
            ResultValue,
            Unit,
            Flag,
            string.IsNullOrWhiteSpace(Reason) ? null : Reason));

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
}
