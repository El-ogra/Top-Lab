using System.Collections.ObjectModel;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TopLab.Application.Features.ReportProduction.Commands.AutoInsertHistory;
using TopLab.Application.Features.ReportProduction.Commands.BuildCombinedReport;
using TopLab.Application.Features.ReportProduction.Commands.InsertHistoryResult;
using TopLab.Application.Features.ReportProduction.Commands.PrintCombinedReport;
using TopLab.Application.Features.ReportProduction.Common;
using TopLab.Application.Features.ReportProduction.Queries.GetCombinableTests;
using TopLab.Application.Features.ReportProduction.Queries.GetPatientTestHistory;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Common.Navigation;

namespace TopLab.Presentation.ViewModels.Patients;

/// <summary>
/// Selectable row wrapper for combinable tests.
/// </summary>
public sealed class CombinableTestRow : ViewModelBase
{
    private bool _isSelected;

    public int PatientTestId { get; init; }
    public int TestId { get; init; }
    public string TestName { get; init; } = string.Empty;
    public string TestCode { get; init; } = string.Empty;
    public int ResultKind { get; init; }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}

/// <summary>
/// S-05 Slice 2: Combined report screen (M07) — select reviewed tests, build preview, print.
/// Insert-history dialog for manual/auto insertion behind confirmations.
/// </summary>
public sealed class CombinedReportViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly ResultErrorPresenter _presenter;
    private readonly IDialogService _dialogs;
    private readonly IServiceProvider _services;
    private readonly INavigationService _navigation;

    private int _patientId;
    private string _patientFullName = string.Empty;
    private string? _labId;
    private ObservableCollection<CombinableTestRow> _combinableTests = new();
    private CombinedReportDto? _preview;
    private bool _isBusy;
    private string _errorMessage = string.Empty;
    private string _statusMessage = string.Empty;

    public CombinedReportViewModel(
        ISender mediator,
        ResultErrorPresenter presenter,
        IDialogService dialogs,
        IServiceProvider services,
        INavigationService navigation)
    {
        _mediator = mediator;
        _presenter = presenter;
        _dialogs = dialogs;
        _services = services;
        _navigation = navigation;

        BuildPreviewCommand = new AsyncRelayCommand(async (_, ct) => await BuildPreviewAsync(ct));
        PrintCommand = new AsyncRelayCommand(async (_, ct) => await PrintAsync(ct));
        InsertFromHistoryCommand = new AsyncRelayCommand(async (_, ct) => await InsertFromHistoryAsync(ct));
        AutoInsertCommand = new AsyncRelayCommand(async (_, ct) => await AutoInsertAsync(ct));
        MoveUpCommand = new RelayCommand(_ => MoveUp());
        MoveDownCommand = new RelayCommand(_ => MoveDown());
        BackCommand = new RelayCommand(_ => _navigation.NavigateTo<PatientVisitHistoryViewModel>());
    }

    public int PatientId => _patientId;
    public string PatientFullName { get => _patientFullName; private set => SetProperty(ref _patientFullName, value); }
    public string? LabId { get => _labId; private set => SetProperty(ref _labId, value); }

    public ObservableCollection<CombinableTestRow> CombinableTests
    {
        get => _combinableTests;
        private set
        {
            if (SetProperty(ref _combinableTests, value))
            {
                OnPropertyChanged(nameof(HasCombinableTests));
                OnPropertyChanged(nameof(ShowCombinableEmpty));
                OnPropertyChanged(nameof(SelectedCount));
            }
        }
    }

    public bool HasCombinableTests => CombinableTests.Count > 0;
    public bool ShowCombinableEmpty => CombinableTests.Count == 0 && _patientId > 0 && !IsBusy;
    public int SelectedCount => CombinableTests.Count(t => t.IsSelected);

    public CombinedReportDto? Preview
    {
        get => _preview;
        private set
        {
            if (SetProperty(ref _preview, value))
            {
                OnPropertyChanged(nameof(HasPreview));
            }
        }
    }

    public bool HasPreview => _preview is not null;

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

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public AsyncRelayCommand BuildPreviewCommand { get; }
    public AsyncRelayCommand PrintCommand { get; }
    public AsyncRelayCommand InsertFromHistoryCommand { get; }
    public AsyncRelayCommand AutoInsertCommand { get; }
    public RelayCommand MoveUpCommand { get; }
    public RelayCommand MoveDownCommand { get; }
    public RelayCommand BackCommand { get; }

    public async Task LoadAsync(int patientId, CancellationToken cancellationToken = default)
    {
        _patientId = patientId;
        IsBusy = true;
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        Preview = null;

        try
        {
            var result = await _mediator.Send(new GetCombinableTestsQuery(patientId), cancellationToken);
            if (result.IsSuccess && result.Value is not null)
            {
                var rows = new ObservableCollection<CombinableTestRow>();
                foreach (var item in result.Value)
                {
                    rows.Add(new CombinableTestRow
                    {
                        PatientTestId = item.PatientTestId,
                        TestId = item.TestId,
                        TestName = item.TestName,
                        TestCode = item.TestCode,
                        ResultKind = item.ResultKind
                    });
                }

                CombinableTests = rows;
                PatientFullName = string.Empty; // will be set by preview
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

    private async Task BuildPreviewAsync(CancellationToken cancellationToken)
    {
        var selectedIds = CombinableTests.Where(t => t.IsSelected).Select(t => t.PatientTestId).ToList();
        if (selectedIds.Count == 0)
        {
            ErrorMessage = "قائمة التحاليل مطلوبة.";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;

        try
        {
            var result = await _mediator.Send(
                new BuildCombinedReportCommand(_patientId, selectedIds), cancellationToken);

            if (result.IsSuccess && result.Value is not null)
            {
                Preview = result.Value;
                PatientFullName = result.Value.PatientFullName;
                LabId = result.Value.LabId;
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

    private async Task PrintAsync(CancellationToken cancellationToken)
    {
        var selectedIds = CombinableTests.Where(t => t.IsSelected).Select(t => t.PatientTestId).ToList();
        if (selectedIds.Count == 0)
        {
            ErrorMessage = "قائمة التحاليل مطلوبة.";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;

        try
        {
            var result = await _mediator.Send(
                new PrintCombinedReportCommand(_patientId, selectedIds), cancellationToken);

            if (result.IsSuccess)
            {
                StatusMessage = "تمت الطباعة.";
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

    private async Task InsertFromHistoryAsync(CancellationToken cancellationToken)
    {
        if (CombinableTests.Count == 0)
        {
            ErrorMessage = "قائمة التحاليل مطلوبة.";
            return;
        }

        // Open insert dialog for the first selected test (or first test)
        var targetTest = CombinableTests.FirstOrDefault(t => t.IsSelected) ?? CombinableTests.FirstOrDefault();
        if (targetTest is null)
        {
            return;
        }

        var vm = _services.GetRequiredService<InsertHistoryDialogViewModel>();
        await vm.LoadAsync(_patientId, targetTest.PatientTestId, targetTest.TestName, cancellationToken);
        var window = new Views.Patients.InsertHistoryDialogWindow(vm)
        {
            Owner = System.Windows.Application.Current?.MainWindow
        };

        bool? confirmed = window.ShowDialog();
        if (confirmed == true && vm.InsertedReport is not null)
        {
            Preview = vm.InsertedReport;
            PatientFullName = vm.InsertedReport.PatientFullName;
            LabId = vm.InsertedReport.LabId;
            StatusMessage = "تم الإدراج من التاريخ.";
        }
    }

    private async Task AutoInsertAsync(CancellationToken cancellationToken)
    {
        if (CombinableTests.Count == 0)
        {
            ErrorMessage = "قائمة التحاليل مطلوبة.";
            return;
        }

        var targetTest = CombinableTests.FirstOrDefault(t => t.IsSelected) ?? CombinableTests.FirstOrDefault();
        if (targetTest is null)
        {
            return;
        }

        var confirmed = await _dialogs.ShowConfirmationAsync(
            "تأكيد الإدراج التلقائي",
            "سيتم إدراج نتائج تاريخية تلقائياً في التقرير. هل تريد المتابعة؟");
        if (!confirmed)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;

        try
        {
            var result = await _mediator.Send(
                new AutoInsertHistoryCommand(targetTest.PatientTestId), cancellationToken);

            if (result.IsSuccess && result.Value is not null)
            {
                Preview = result.Value;
                PatientFullName = result.Value.PatientFullName;
                LabId = result.Value.LabId;
                StatusMessage = "تم الإدراج التلقائي.";
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

    private void MoveUp()
    {
        var selected = CombinableTests.FirstOrDefault(t => t.IsSelected);
        if (selected is null)
        {
            return;
        }

        var index = CombinableTests.IndexOf(selected);
        if (index > 0)
        {
            CombinableTests.Move(index, index - 1);
        }
    }

    private void MoveDown()
    {
        var selected = CombinableTests.FirstOrDefault(t => t.IsSelected);
        if (selected is null)
        {
            return;
        }

        var index = CombinableTests.IndexOf(selected);
        if (index < CombinableTests.Count - 1)
        {
            CombinableTests.Move(index, index + 1);
        }
    }
}
