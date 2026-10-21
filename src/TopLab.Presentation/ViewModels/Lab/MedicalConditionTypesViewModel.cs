using System.Collections.ObjectModel;
using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientRegistration.Commands.AddMedicalConditionType;
using TopLab.Application.Features.PatientRegistration.Commands.RenameMedicalConditionType;
using TopLab.Application.Features.PatientRegistration.Common;
using TopLab.Application.Features.PatientRegistration.Queries.GetMedicalConditionTypes;
using TopLab.Domain.Common.Enums;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.ErrorPresentation;

namespace TopLab.Presentation.ViewModels.Lab;

/// <summary>One catalogue row as the maintenance screen shows it.</summary>
public sealed class MedicalConditionTypeRow
{
    public MedicalConditionTypeRow(int id, string name, string categoryLabel, bool isSeeded)
    {
        Id = id;
        Name = name;
        CategoryLabel = categoryLabel;
        IsSeeded = isSeeded;
    }

    public int Id { get; }

    public string Name { get; }

    public string CategoryLabel { get; }

    /// <summary>Seeded rows belong to the migration's snapshot and are read-only.</summary>
    public bool IsSeeded { get; }
}

/// <summary>
/// STAGE-01 (baseline 1.2.2.3): the medical-history catalogue's maintenance
/// tab. Catalogue rows can be added and renamed here so that ordinary
/// catalogue changes do not need a migration. The seeded rows (the ones the
/// migration inserts) are read-only, and nothing can be deleted: the join
/// table carries no foreign key, so a deleted row would silently drop a
/// patient's recorded answer.
/// </summary>
public sealed class MedicalConditionTypesViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly ResultErrorPresenter _presenter;

    private ObservableCollection<MedicalConditionTypeRow> _rows = new();
    private MedicalConditionTypeRow? _selectedRow;
    private string _name = string.Empty;
    private MedicalConditionCategoryOption _selectedCategory;
    private bool _isBusy;
    private string _errorMessage = string.Empty;
    private string _statusMessage = string.Empty;

    public MedicalConditionTypesViewModel(ISender mediator, ResultErrorPresenter presenter)
    {
        _mediator = mediator;
        _presenter = presenter;

        CategoryOptions = new List<MedicalConditionCategoryOption>
        {
            new(MedicalConditionCategory.Medication, "علاج"),
            new(MedicalConditionCategory.Condition, "حالة مرضية"),
            new(MedicalConditionCategory.Pregnancy, "حمل")
        };
        _selectedCategory = CategoryOptions[0];

        LoadCommand = new AsyncRelayCommand(_ => LoadAsync());
        AddCommand = new AsyncRelayCommand(_ => AddAsync());
        RenameCommand = new AsyncRelayCommand(_ => RenameAsync());
    }

    public IReadOnlyList<MedicalConditionCategoryOption> CategoryOptions { get; }

    public ObservableCollection<MedicalConditionTypeRow> Rows
    {
        get => _rows;
        private set => SetProperty(ref _rows, value);
    }

    public MedicalConditionTypeRow? SelectedRow
    {
        get => _selectedRow;
        set
        {
            if (SetProperty(ref _selectedRow, value))
            {
                if (value is not null)
                {
                    Name = value.Name;
                }

                OnPropertyChanged(nameof(CanRenameSelected));
            }
        }
    }

    public string Name { get => _name; set => SetProperty(ref _name, value); }

    public MedicalConditionCategoryOption SelectedCategory
    {
        get => _selectedCategory;
        set => SetProperty(ref _selectedCategory, value);
    }

    /// <summary>Seeded rows are read-only in this screen.</summary>
    public bool CanRenameSelected => SelectedRow is not null && !SelectedRow.IsSeeded;

    public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }
    public string ErrorMessage { get => _errorMessage; private set => SetProperty(ref _errorMessage, value); }
    public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }

    public AsyncRelayCommand LoadCommand { get; }
    public AsyncRelayCommand AddCommand { get; }
    public AsyncRelayCommand RenameCommand { get; }

    public async Task LoadAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var result = await _mediator.Send(new GetMedicalConditionTypesQuery());
            if (!result.IsSuccess || result.Value is null)
            {
                if (result.Error is not null)
                {
                    ErrorMessage = _presenter.Present(result.Error);
                }

                return;
            }

            Rows = new ObservableCollection<MedicalConditionTypeRow>(
                result.Value.Select(r => new MedicalConditionTypeRow(
                    r.MedicalConditionTypeId,
                    r.Name,
                    CategoryLabel(r.Category),
                    MedicalConditionCatalogue.IsSeeded(r.MedicalConditionTypeId))));

            OnPropertyChanged(nameof(CanRenameSelected));
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task AddAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        try
        {
            var result = await _mediator.Send(
                new AddMedicalConditionTypeCommand(Name.Trim(), SelectedCategory.Value));

            if (!result.IsSuccess)
            {
                ErrorMessage = result.Error is not null ? _presenter.Present(result.Error) : ErrorMessage;
                return;
            }

            Name = string.Empty;
            StatusMessage = "تمت إضافة نوع الحالة الصحية.";
            await LoadAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RenameAsync()
    {
        if (SelectedRow is null)
        {
            ErrorMessage = "اختر النوع المراد تعديل اسمه.";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        try
        {
            var result = await _mediator.Send(
                new RenameMedicalConditionTypeCommand(SelectedRow.Id, Name.Trim()));

            if (!result.IsSuccess)
            {
                ErrorMessage = result.Error is not null ? _presenter.Present(result.Error) : ErrorMessage;
                return;
            }

            StatusMessage = "تم حفظ الاسم.";
            await LoadAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static string CategoryLabel(MedicalConditionCategory category) => category switch
    {
        MedicalConditionCategory.Medication => "علاج",
        MedicalConditionCategory.Condition => "حالة مرضية",
        MedicalConditionCategory.Pregnancy => "حمل",
        _ => category.ToString()
    };
}

/// <summary>A catalogue category with its Arabic label.</summary>
public sealed record MedicalConditionCategoryOption(MedicalConditionCategory Value, string Label);
