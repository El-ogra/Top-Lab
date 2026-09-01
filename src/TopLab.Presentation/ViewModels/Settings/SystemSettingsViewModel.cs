using MediatR;
using TopLab.Application.Features.SystemAndPrintSettings.Commands.SavePrinterAssignments;
using TopLab.Application.Features.SystemAndPrintSettings.Commands.UpdateDatabaseServerSettings;
using TopLab.Application.Features.SystemAndPrintSettings.Commands.UpdateSystemSettings;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Queries.CheckBackupPath;
using TopLab.Application.Features.SystemAndPrintSettings.Queries.GetDatabaseServerSettings;
using TopLab.Application.Features.SystemAndPrintSettings.Queries.GetPrinterAssignments;
using TopLab.Application.Features.SystemAndPrintSettings.Queries.GetSystemSettings;
using TopLab.Application.Common.Interfaces;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Common.Navigation;
using TopLab.Presentation.Services;
using TopLab.Domain.Common.Enums;

namespace TopLab.Presentation.ViewModels.Settings;

/// <summary>S-28: system settings — printer assignments, default account type,
/// general flags, daily backup, and the database-server connection editor.</summary>
public sealed class SystemSettingsViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly IPrinterCatalogService _printers;
    private readonly IWorkstationConnectionSettingsProvider _connection;
    private readonly IDialogService _dialogs;
    private readonly ResultErrorPresenter _presenter;
    private readonly INavigationService _navigation;

    private string _errorMessage = string.Empty;
    private string _statusMessage = string.Empty;
    private bool _isBusy;
    private bool _hasLoaded;

    private AccountType _defaultAccountType;
    private bool _saveTreatingDoctorOnlyFromEntityWindow;
    private bool _enablePatientNameSearchAssist;
    private bool _disableAutoTitleInsertion;
    private bool _printFileExternalBarcode;
    private bool _printDateTimeOnTubeBarcode;
    private bool _printLabIdInsteadOfPatientId;
    private bool _autoReviewAndComplete;
    private bool _printAccountInsteadOfDateOnReport;
    private ResultScreenAccountDisplayMode _resultScreenAccountDisplayMode;
    private bool _dailyBackupEnabled;
    private string _dailyBackupPath = string.Empty;

    private string _reportPrinter = string.Empty;
    private string _barcodePrinter = string.Empty;
    private string _envelopePrinter = string.Empty;
    private string _receiptPrinter = string.Empty;

    private string _server = string.Empty;
    private string _database = string.Empty;
    private string _login = string.Empty;
    private string _password = string.Empty;
    private bool _integratedSecurity = true;

    public SystemSettingsViewModel(
        ISender mediator,
        IPrinterCatalogService printers,
        IWorkstationConnectionSettingsProvider connection,
        IDialogService dialogs,
        ResultErrorPresenter presenter,
        INavigationService navigation)
    {
        _mediator = mediator;
        _printers = printers;
        _connection = connection;
        _dialogs = dialogs;
        _presenter = presenter;
        _navigation = navigation;

        var printerList = _printers.GetInstalledPrinters().ToList();
        if (printerList.Count == 0)
        {
            printerList.Add(string.Empty);
        }

        InstalledPrinters = printerList;

        AccountTypeOptions = new List<AccountType> { AccountType.Individual, AccountType.LabToLab, AccountType.Contracts, AccountType.Free };
        DisplayModeOptions = new List<ResultScreenAccountDisplayMode> { ResultScreenAccountDisplayMode.Hidden, ResultScreenAccountDisplayMode.Summary, ResultScreenAccountDisplayMode.Detailed };

        LoadCommand = new AsyncRelayCommand(_ => LoadAsync());
        SaveCommand = new AsyncRelayCommand(_ => SaveAsync());
        CheckBackupPathCommand = new AsyncRelayCommand(_ => CheckBackupPathAsync());
        TestDatabaseServerCommand = new AsyncRelayCommand(_ => TestDatabaseServerAsync());
        SaveDatabaseServerCommand = new AsyncRelayCommand(_ => SaveDatabaseServerAsync());
        BackToDashboardCommand = new RelayCommand(_ => _navigation.NavigateTo<SettingsDashboardViewModel>());
    }

    public IReadOnlyList<string> InstalledPrinters { get; }
    public IReadOnlyList<AccountType> AccountTypeOptions { get; }
    public IReadOnlyList<ResultScreenAccountDisplayMode> DisplayModeOptions { get; }

    public string ErrorMessage { get => _errorMessage; private set => SetProperty(ref _errorMessage, value); }
    public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }
    public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }

    public AsyncRelayCommand LoadCommand { get; }
    public AsyncRelayCommand SaveCommand { get; }
    public AsyncRelayCommand CheckBackupPathCommand { get; }
    public AsyncRelayCommand TestDatabaseServerCommand { get; }
    public AsyncRelayCommand SaveDatabaseServerCommand { get; }
    public RelayCommand BackToDashboardCommand { get; }

    public AccountType DefaultAccountType { get => _defaultAccountType; set => SetProperty(ref _defaultAccountType, value); }
    public bool SaveTreatingDoctorOnlyFromEntityWindow { get => _saveTreatingDoctorOnlyFromEntityWindow; set => SetProperty(ref _saveTreatingDoctorOnlyFromEntityWindow, value); }
    public bool EnablePatientNameSearchAssist { get => _enablePatientNameSearchAssist; set => SetProperty(ref _enablePatientNameSearchAssist, value); }
    public bool DisableAutoTitleInsertion { get => _disableAutoTitleInsertion; set => SetProperty(ref _disableAutoTitleInsertion, value); }
    public bool PrintFileExternalBarcode { get => _printFileExternalBarcode; set => SetProperty(ref _printFileExternalBarcode, value); }
    public bool PrintDateTimeOnTubeBarcode { get => _printDateTimeOnTubeBarcode; set => SetProperty(ref _printDateTimeOnTubeBarcode, value); }
    public bool PrintLabIdInsteadOfPatientId { get => _printLabIdInsteadOfPatientId; set => SetProperty(ref _printLabIdInsteadOfPatientId, value); }
    public bool AutoReviewAndComplete { get => _autoReviewAndComplete; set => SetProperty(ref _autoReviewAndComplete, value); }
    public bool PrintAccountInsteadOfDateOnReport { get => _printAccountInsteadOfDateOnReport; set => SetProperty(ref _printAccountInsteadOfDateOnReport, value); }
    public ResultScreenAccountDisplayMode ResultScreenAccountDisplayMode { get => _resultScreenAccountDisplayMode; set => SetProperty(ref _resultScreenAccountDisplayMode, value); }
    public bool DailyBackupEnabled { get => _dailyBackupEnabled; set => SetProperty(ref _dailyBackupEnabled, value); }
    public string DailyBackupPath { get => _dailyBackupPath; set => SetProperty(ref _dailyBackupPath, value); }

    public string ReportPrinter { get => _reportPrinter; set => SetProperty(ref _reportPrinter, value); }
    public string BarcodePrinter { get => _barcodePrinter; set => SetProperty(ref _barcodePrinter, value); }
    public string EnvelopePrinter { get => _envelopePrinter; set => SetProperty(ref _envelopePrinter, value); }
    public string ReceiptPrinter { get => _receiptPrinter; set => SetProperty(ref _receiptPrinter, value); }

    public string Server { get => _server; set => SetProperty(ref _server, value); }
    public string Database { get => _database; set => SetProperty(ref _database, value); }
    public string Login { get => _login; set => SetProperty(ref _login, value); }
    public string Password { get => _password; set => SetProperty(ref _password, value); }
    public bool IntegratedSecurity { get => _integratedSecurity; set => SetProperty(ref _integratedSecurity, value); }

    public async Task LoadAsync()
    {
        if (_hasLoaded)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var systems = await _mediator.Send(new GetSystemSettingsQuery());
            if (systems.IsSuccess && systems.Value is not null)
            {
                var s = systems.Value;
                DefaultAccountType = s.DefaultAccountType;
                SaveTreatingDoctorOnlyFromEntityWindow = s.SaveTreatingDoctorOnlyFromEntityWindow;
                EnablePatientNameSearchAssist = s.EnablePatientNameSearchAssist;
                DisableAutoTitleInsertion = s.DisableAutoTitleInsertion;
                PrintFileExternalBarcode = s.PrintFileExternalBarcode;
                PrintDateTimeOnTubeBarcode = s.PrintDateTimeOnTubeBarcode;
                PrintLabIdInsteadOfPatientId = s.PrintLabIdInsteadOfPatientId;
                AutoReviewAndComplete = s.AutoReviewAndComplete;
                PrintAccountInsteadOfDateOnReport = s.PrintAccountInsteadOfDateOnReport;
                ResultScreenAccountDisplayMode = s.ResultScreenAccountDisplayMode;
                DailyBackupEnabled = s.DailyBackupEnabled;
                DailyBackupPath = s.DailyBackupPath ?? string.Empty;
            }
            else if (systems.Error is not null)
            {
                ErrorMessage = _presenter.Present(systems.Error);
            }

            var printers = await _mediator.Send(new GetPrinterAssignmentsQuery());
            if (printers.IsSuccess && printers.Value is not null)
            {
                foreach (var a in printers.Value)
                {
                    switch (a.OutputType)
                    {
                        case PrinterOutputType.Reports: ReportPrinter = a.PrinterName; break;
                        case PrinterOutputType.Barcode: BarcodePrinter = a.PrinterName; break;
                        case PrinterOutputType.Envelope: EnvelopePrinter = a.PrinterName; break;
                        case PrinterOutputType.Receipt: ReceiptPrinter = a.PrinterName; break;
                    }
                }
            }
            else if (printers.Error is not null)
            {
                ErrorMessage = _presenter.Present(printers.Error);
            }

            var dbServer = await _mediator.Send(new GetDatabaseServerSettingsQuery());
            if (dbServer.IsSuccess && dbServer.Value is not null)
            {
                Server = dbServer.Value.ServerName;
                Database = dbServer.Value.DatabaseName;
                Login = dbServer.Value.Login;
                IntegratedSecurity = dbServer.Value.IntegratedSecurity;
            }

            _hasLoaded = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task SaveAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        try
        {
            var result = await _mediator.Send(new UpdateSystemSettingsCommand(
                DefaultAccountType,
                SaveTreatingDoctorOnlyFromEntityWindow,
                EnablePatientNameSearchAssist,
                DisableAutoTitleInsertion,
                PrintFileExternalBarcode,
                PrintDateTimeOnTubeBarcode,
                PrintLabIdInsteadOfPatientId,
                AutoReviewAndComplete,
                PrintAccountInsteadOfDateOnReport,
                ResultScreenAccountDisplayMode,
                DailyBackupEnabled,
                DailyBackupPath));

            if (!result.IsSuccess)
            {
                ErrorMessage = result.Error is not null ? _presenter.Present(result.Error) : ErrorMessage;
                return;
            }

            var printerAssignments = new List<PrinterAssignmentDto>
            {
                new(PrinterOutputType.Reports, ReportPrinter),
                new(PrinterOutputType.Barcode, BarcodePrinter),
                new(PrinterOutputType.Envelope, EnvelopePrinter),
                new(PrinterOutputType.Receipt, ReceiptPrinter)
            };
            var printerResult = await _mediator.Send(new SavePrinterAssignmentsCommand(printerAssignments));
            if (!printerResult.IsSuccess)
            {
                ErrorMessage = printerResult.Error is not null ? _presenter.Present(printerResult.Error) : ErrorMessage;
                return;
            }

            StatusMessage = "تم حفظ الإعدادات بنجاح.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task CheckBackupPathAsync()
    {
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        if (string.IsNullOrWhiteSpace(DailyBackupPath))
        {
            ErrorMessage = "يرجى إدخال مسار النسخ الاحتياطي أولاً.";
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _mediator.Send(new CheckBackupPathQuery(DailyBackupPath));
            if (result.IsSuccess)
            {
                StatusMessage = "مسار النسخ الاحتياطي صالح.";
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

    public async Task TestDatabaseServerAsync()
    {
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        IsBusy = true;
        try
        {
            var candidate = BuildConnectionString(Server, Database, IntegratedSecurity, Login, Password);
            var ok = await _connection.TestConnectionStringAsync(candidate);
            StatusMessage = ok ? "تم الاتصال بخادم قواعد البيانات بنجاح." : "تعذر الاتصال بخادم قواعد البيانات.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task SaveDatabaseServerAsync()
    {
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        IsBusy = true;
        try
        {
            var result = await _mediator.Send(new UpdateDatabaseServerSettingsCommand(Server, Database, IntegratedSecurity, Login, Password));
            if (result.IsSuccess)
            {
                StatusMessage = "تم حفظ إعدادات الخادم. أعد تشغيل البرنامج لتطبيق التغيير.";
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

    private static string BuildConnectionString(string server, string database, bool integratedSecurity, string login, string password)
        => integratedSecurity
            ? $"Server={server};Database={database};Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
            : $"Server={server};Database={database};User Id={login};Password={password};MultipleActiveResultSets=true;TrustServerCertificate=True";
}