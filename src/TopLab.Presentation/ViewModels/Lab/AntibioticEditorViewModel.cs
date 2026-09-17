using MediatR;
using TopLab.Application.Features.CultureAndAntibiotics.Commands.CreateAntibiotic;
using TopLab.Application.Features.CultureAndAntibiotics.Commands.UpdateAntibiotic;
using TopLab.Application.Features.CultureAndAntibiotics.Common;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.ErrorPresentation;

namespace TopLab.Presentation.ViewModels.Lab;

/// <summary>
/// Antibiotic editor dialog VM (S-02 Slice 5): Name + the two flags only
/// (no Abbreviation/ScientificName/SensitivityCategory exists in code).
/// </summary>
public sealed class AntibioticEditorViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly ResultErrorPresenter _presenter;

    private int _editingId;
    private bool _isEditMode;
    private string _name = string.Empty;
    private bool _isPregnancyFlagged;
    private bool _isChildrenFlagged;
    private string _errorMessage = string.Empty;
    private bool _isBusy;

    public AntibioticEditorViewModel(ISender mediator, ResultErrorPresenter presenter)
    {
        _mediator = mediator;
        _presenter = presenter;

        SaveCommand = new AsyncRelayCommand(_ => SaveAsync());
    }

    public string Name { get => _name; set => SetProperty(ref _name, value); }
    public bool IsPregnancyFlagged { get => _isPregnancyFlagged; set => SetProperty(ref _isPregnancyFlagged, value); }
    public bool IsChildrenFlagged { get => _isChildrenFlagged; set => SetProperty(ref _isChildrenFlagged, value); }
    public string ErrorMessage { get => _errorMessage; private set => SetProperty(ref _errorMessage, value); }
    public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }

    public AsyncRelayCommand SaveCommand { get; }

    public void InitializeNew()
    {
        _editingId = 0;
        _isEditMode = false;
        Name = string.Empty;
        IsPregnancyFlagged = false;
        IsChildrenFlagged = false;
    }

    public void InitializeEdit(AntibioticDto dto)
    {
        _editingId = dto.Id;
        _isEditMode = true;
        Name = dto.Name;
        IsPregnancyFlagged = dto.IsPregnancyFlagged;
        IsChildrenFlagged = dto.IsChildrenFlagged;
    }

    public async Task<bool> SaveAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            if (!_isEditMode)
            {
                var result = await _mediator.Send(new CreateAntibioticCommand(
                    Name.Trim(), IsPregnancyFlagged, IsChildrenFlagged));
                if (!result.IsSuccess)
                {
                    ErrorMessage = result.Error is not null ? _presenter.Present(result.Error) : ErrorMessage;
                    return false;
                }

                return true;
            }
            else
            {
                var result = await _mediator.Send(new UpdateAntibioticCommand(
                    _editingId, Name.Trim(), IsPregnancyFlagged, IsChildrenFlagged));
                if (!result.IsSuccess)
                {
                    ErrorMessage = result.Error is not null ? _presenter.Present(result.Error) : ErrorMessage;
                    return false;
                }

                return true;
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}
