using System.Collections.ObjectModel;
using System.IO;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.CultureAndAntibiotics.Commands.DeleteAntibiotic;
using TopLab.Application.Features.CultureAndAntibiotics.Common;
using TopLab.Application.Features.CultureAndAntibiotics.Queries.GetAntibiotics;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Views.Lab;

namespace TopLab.Presentation.ViewModels.Lab;

/// <summary>
/// Antibiotic dictionary tab (S-02 Slice 5): search + grid with the 3
/// confirmed columns. Attached-antibiotic delete is blocked (D6).
/// </summary>
public sealed class AntibioticsViewModel : ViewModelBase
{
    internal const string AttachedRefusalFragment = "لارتباطه بمزرعة";
    internal const string AttachedRefusalMessage = "لا يمكن حذف هذا المضاد لارتباطه بمزرعة — افكك الربط أولاً من شاشة الربط";

    private readonly ISender _mediator;
    private readonly IDialogService _dialogs;
    private readonly ResultErrorPresenter _presenter;
    private readonly IAntibioticListPdfWriter _antibioticListPdfWriter;
    private readonly ILabPrintTextStore _labPrintTextStore;
    private readonly IServiceProvider _services;

    private string _searchTerm = string.Empty;
    private ObservableCollection<AntibioticDto> _antibiotics = new();
    private bool _isBusy;
    private string _errorMessage = string.Empty;
    private string _statusMessage = string.Empty;

    public AntibioticsViewModel(
        ISender mediator,
        IDialogService dialogs,
        ResultErrorPresenter presenter,
        IAntibioticListPdfWriter antibioticListPdfWriter,
        ILabPrintTextStore labPrintTextStore,
        IServiceProvider services)
    {
        _mediator = mediator;
        _dialogs = dialogs;
        _presenter = presenter;
        _antibioticListPdfWriter = antibioticListPdfWriter;
        _labPrintTextStore = labPrintTextStore;
        _services = services;

        LoadAntibioticsCommand = new AsyncRelayCommand(_ => LoadAsync());
        SearchCommand = new RelayCommand(_ => _ = LoadAsync());
        OpenCreateCommand = new AsyncRelayCommand(_ => OpenCreateAsync());
        OpenEditCommand = new AsyncRelayCommand((p, _) => OpenEditAsync(p as AntibioticDto));
        DeleteCommand = new AsyncRelayCommand((p, _) => DeleteAsync(p as AntibioticDto));
        // GAP-25 (3.9.1.6.5): enabled only when the catalogue on screen has rows —
        // an empty list is not printable.
        PrintListCommand = new AsyncRelayCommand(_ => PrintListAsync(), () => Antibiotics.Count > 0);
    }

    public string SearchTerm
    {
        get => _searchTerm;
        set => SetProperty(ref _searchTerm, value);
    }

    public ObservableCollection<AntibioticDto> Antibiotics
    {
        get => _antibiotics;
        private set
        {
            if (SetProperty(ref _antibiotics, value))
            {
                OnPropertyChanged(nameof(ShowEmpty));
                // GAP-25: keep the print command's enabled state in step with the
                // catalogue actually shown on screen.
                PrintListCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool ShowEmpty => Antibiotics.Count == 0;
    public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }
    public string ErrorMessage { get => _errorMessage; private set => SetProperty(ref _errorMessage, value); }
    public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }

    public AsyncRelayCommand LoadAntibioticsCommand { get; }
    public RelayCommand SearchCommand { get; }
    public AsyncRelayCommand OpenCreateCommand { get; }
    public AsyncRelayCommand OpenEditCommand { get; }
    public AsyncRelayCommand DeleteCommand { get; }

    /// <summary>GAP-25 (3.9.1.6.5) — print the catalogue currently on screen.</summary>
    public AsyncRelayCommand PrintListCommand { get; }

    public async Task LoadAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var result = await _mediator.Send(new GetAntibioticsQuery(
                string.IsNullOrWhiteSpace(SearchTerm) ? null : SearchTerm.Trim()));
            if (result.IsSuccess && result.Value is not null)
            {
                Antibiotics = new ObservableCollection<AntibioticDto>(result.Value);
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

    private async Task OpenCreateAsync()
    {
        var vm = _services.GetRequiredService<AntibioticEditorViewModel>();
        vm.InitializeNew();
        var window = new AntibioticEditorWindow(vm)
        {
            Owner = System.Windows.Application.Current?.MainWindow
        };
        window.ShowDialog();
        await LoadAsync();
    }

    private async Task OpenEditAsync(AntibioticDto? dto)
    {
        if (dto is null)
        {
            return;
        }

        var vm = _services.GetRequiredService<AntibioticEditorViewModel>();
        vm.InitializeEdit(dto);
        var window = new AntibioticEditorWindow(vm)
        {
            Owner = System.Windows.Application.Current?.MainWindow
        };
        window.ShowDialog();
        await LoadAsync();
    }

    private async Task DeleteAsync(AntibioticDto? dto)
    {
        if (dto is null)
        {
            return;
        }

        bool confirm = await _dialogs.ShowConfirmationAsync(
            "حذف المضاد",
            $"سيتم حذف المضاد \"{dto.Name}\" — متابعة؟");
        if (!confirm)
        {
            return;
        }

        var result = await _mediator.Send(new DeleteAntibioticCommand(dto.Id));
        if (!result.IsSuccess)
        {
            // D6: the backend Conflict refusal for attached antibiotics is
            // surfaced translated; the feature's internal DomainFailureTranslator
            // covers create/update domain exceptions only.
            ErrorMessage = result.Error is not null && result.Error.Message.Contains(AttachedRefusalFragment)
                ? AttachedRefusalMessage
                : result.Error is not null ? _presenter.Present(result.Error) : ErrorMessage;
            return;
        }

        StatusMessage = "تم حذف المضاد.";
        await LoadAsync();
    }

    /// <summary>
    /// GAP-25 (3.9.1.6.5) — write the antibiotic catalogue shown on screen to a PDF
    /// through the Application-layer port. The rows keep the catalogue order and
    /// carry each antibiotic's commercial name. A failed or cancelled save never
    /// reports success.
    /// </summary>
    private async Task PrintListAsync()
    {
        if (Antibiotics.Count == 0)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        try
        {
            var path = await _dialogs.PickPdfSavePathAsync("قائمة المضادات.pdf");
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            var labTextResult = await _labPrintTextStore.GetAsync(LabPrintTextScope.Report);
            if (!labTextResult.IsSuccess || labTextResult.Value is null)
            {
                ErrorMessage = labTextResult.Error is not null
                    ? _presenter.Present(labTextResult.Error)
                    : "تعذّر تحميل بيانات المعمل للطباعة.";
                return;
            }

            await _antibioticListPdfWriter.WritePdfAsync(path, Antibiotics.ToList(), labTextResult.Value);
            StatusMessage = "تم إنشاء ملف قائمة المضادات.";
        }
        catch (IOException)
        {
            ErrorMessage = "الملف موجود مسبقًا؛ لم يتم الكتابة فوقه.";
        }
        catch (UnauthorizedAccessException)
        {
            ErrorMessage = "لا توجد صلاحية للكتابة في المسار المحدد.";
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorMessage = _presenter.Present(Error.Unexpected(ex.Message));
        }
        finally
        {
            IsBusy = false;
        }
    }
}
