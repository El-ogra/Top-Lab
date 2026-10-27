using MediatR;
using TopLab.Application.Features.CultureAndAntibiotics.Commands.CreateAntibiotic;
using TopLab.Application.Features.CultureAndAntibiotics.Commands.UpdateAntibiotic;
using TopLab.Application.Features.CultureAndAntibiotics.Common;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.ErrorPresentation;

namespace TopLab.Presentation.ViewModels.Lab;

/// <summary>
/// Antibiotic editor dialog VM: Name + Symbol + ScientificName + CommercialName + the two flags.
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
    private string? _symbol;
    private string? _scientificName;
    private string? _commercialName;
    private string _errorMessage = string.Empty;
    private bool _isBusy;

    // GAP-23: snapshot of the last-loaded/saved values, so Undo can revert
    // every editable field without persisting anything.
    private string _loadedName = string.Empty;
    private bool _loadedIsPregnancyFlagged;
    private bool _loadedIsChildrenFlagged;
    private string? _loadedSymbol;
    private string? _loadedScientificName;
    private string? _loadedCommercialName;

    public AntibioticEditorViewModel(ISender mediator, ResultErrorPresenter presenter)
    {
        _mediator = mediator;
        _presenter = presenter;

        SaveCommand = new AsyncRelayCommand(_ => SaveAsync());
        UndoCommand = new AsyncRelayCommand(async (_, ct) => await UndoAsync(ct));
    }

    public string Name { get => _name; set => SetProperty(ref _name, value); }
    public bool IsPregnancyFlagged { get => _isPregnancyFlagged; set => SetProperty(ref _isPregnancyFlagged, value); }
    public bool IsChildrenFlagged { get => _isChildrenFlagged; set => SetProperty(ref _isChildrenFlagged, value); }
    public string? Symbol { get => _symbol; set => SetProperty(ref _symbol, value); }
    public string? ScientificName { get => _scientificName; set => SetProperty(ref _scientificName, value); }
    public string? CommercialName { get => _commercialName; set => SetProperty(ref _commercialName, value); }
    public string ErrorMessage { get => _errorMessage; private set => SetProperty(ref _errorMessage, value); }
    public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }

    public AsyncRelayCommand SaveCommand { get; }
    public AsyncRelayCommand UndoCommand { get; }

    public void InitializeNew()
    {
        _editingId = 0;
        _isEditMode = false;
        Name = string.Empty;
        IsPregnancyFlagged = false;
        IsChildrenFlagged = false;
        Symbol = null;
        ScientificName = null;
        CommercialName = null;
        CaptureSnapshot();
    }

    public void InitializeEdit(AntibioticDto dto)
    {
        _editingId = dto.Id;
        _isEditMode = true;
        Name = dto.Name;
        IsPregnancyFlagged = dto.IsPregnancyFlagged;
        IsChildrenFlagged = dto.IsChildrenFlagged;
        Symbol = dto.Symbol;
        ScientificName = dto.ScientificName;
        CommercialName = dto.CommercialName;
        CaptureSnapshot();
    }

    private void CaptureSnapshot()
    {
        _loadedName = Name;
        _loadedIsPregnancyFlagged = IsPregnancyFlagged;
        _loadedIsChildrenFlagged = IsChildrenFlagged;
        _loadedSymbol = Symbol;
        _loadedScientificName = ScientificName;
        _loadedCommercialName = CommercialName;
    }

    private async Task UndoAsync(CancellationToken cancellationToken = default)
    {
        // GAP-23: revert every editable field to its last-loaded/saved value.
        // No save/update command is dispatched — Undo never persists.
        ErrorMessage = string.Empty;
        Name = _loadedName;
        IsPregnancyFlagged = _loadedIsPregnancyFlagged;
        IsChildrenFlagged = _loadedIsChildrenFlagged;
        Symbol = _loadedSymbol;
        ScientificName = _loadedScientificName;
        CommercialName = _loadedCommercialName;
        await Task.CompletedTask;
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
                    Name.Trim(), IsPregnancyFlagged, IsChildrenFlagged,
                    BlankToNull(Symbol), BlankToNull(ScientificName), BlankToNull(CommercialName)));
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
                    _editingId, Name.Trim(), IsPregnancyFlagged, IsChildrenFlagged,
                    BlankToNull(Symbol), BlankToNull(ScientificName), BlankToNull(CommercialName)));
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

    private static string? BlankToNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
