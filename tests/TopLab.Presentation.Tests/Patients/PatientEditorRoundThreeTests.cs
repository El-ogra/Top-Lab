using System.IO;
using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientBilling.Commands.ClearPatientAccount;
using TopLab.Application.Features.PatientBilling.Commands.RecordExtraCharge;
using TopLab.Application.Features.PatientBilling.Commands.RecordPayment;
using TopLab.Application.Features.PatientBilling.Commands.RecordRefund;
using TopLab.Application.Features.PatientBilling.Common;
using TopLab.Application.Features.PatientBilling.Queries.GetPatientAccount;
using TopLab.Application.Features.PatientEnvelope.Commands.PrintEnvelope;
using TopLab.Application.Features.PatientRegistration.Common;
using TopLab.Application.Features.PatientRegistration.Queries.GetPatientById;
using TopLab.Application.Features.PatientRegistration.Queries.GetPatientVisitHistory;
using TopLab.Application.Features.WorkSheets.Common;
using TopLab.Application.Features.WorkSheets.Queries.GetVisitWorkSheet;
using TopLab.Domain.Common.Enums;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Common.Navigation;
using TopLab.Presentation.Tests.Common;
using TopLab.Presentation.ViewModels.Patients;
using Xunit;

namespace TopLab.Presentation.Tests.Patients;

/// <summary>
/// R1 Round 3 A1/A3: the patient editor's navigation actions and its account-panel work.
/// <para>
/// The editor is driven through its real load path with a hand-rolled <see cref="ISender"/>
/// shim (the pattern <c>PatientEditorLabOrderTests</c> and
/// <c>PatientEditorClearAllGuardTests</c> already use), so nothing here instantiates a WPF
/// element (SD-19) and no private method is invoked directly.
/// </para>
/// </summary>
public class PatientEditorRoundThreeTests
{
    private const int PatientIdValue = 7;
    private static readonly DateTime Clock = new(2026, 10, 25, 9, 0, 0, DateTimeKind.Utc);

    // ================================================================
    // Sender shim
    // ================================================================

    private sealed class EditorSender : ISender
    {
        public int EnvelopeCalls { get; private set; }
        public int PaymentCalls { get; private set; }
        public int ExtraChargeCalls { get; private set; }
        public int RefundCalls { get; private set; }
        public int ClearCalls { get; private set; }
        public List<RecordPaymentCommand> Payments { get; } = new();
        public Result? EnvelopeResult { get; set; }
        public Result<int>? RefundResult { get; set; }
        public Result<int>? ClearResult { get; set; }
        public decimal TotalCharged { get; set; } = 200m;
        public decimal TotalPaid { get; set; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            switch (request)
            {
                case GetPatientByIdQuery:
                    return Task.FromResult((TResponse)(object)Result<PatientDetailDto>.Success(Detail()));
                case GetPatientVisitHistoryQuery:
                    return Task.FromResult((TResponse)(object)Result<IReadOnlyList<VisitHistoryDto>>.Success(
                        new[] { Visit() }));
                case GetVisitWorkSheetQuery:
                    return Task.FromResult((TResponse)(object)Result<VisitWorkSheetDto>.Success(Sheet()));
                case GetPatientAccountQuery:
                    return Task.FromResult((TResponse)(object)Result<PatientAccountDto>.Success(Account()));
                case PrintEnvelopeCommand:
                    EnvelopeCalls++;
                    return Task.FromResult((TResponse)(object)(EnvelopeResult ?? Result.Success()));
                case RecordPaymentCommand payment:
                    PaymentCalls++;
                    Payments.Add(payment);
                    return Task.FromResult((TResponse)(object)Result<int>.Success(1));
                case RecordExtraChargeCommand:
                    ExtraChargeCalls++;
                    return Task.FromResult((TResponse)(object)Result<int>.Success(1));
                case RecordRefundCommand:
                    RefundCalls++;
                    return Task.FromResult((TResponse)(object)(RefundResult ?? Result<int>.Success(1)));
                case ClearPatientAccountCommand:
                    ClearCalls++;
                    return Task.FromResult((TResponse)(object)(ClearResult ?? Result<int>.Success(2)));
                default:
                    throw new NotSupportedException($"EditorSender has no canned response for {request.GetType().Name}.");
            }
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
            => throw new NotSupportedException();
        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        private static PatientDetailDto Detail() => new(
            PatientId: PatientIdValue, LabId: "LAB-A", Title: null, FullName: "Patient 7",
            Sex: Sex.Male, AgeValue: 30, AgeUnit: AgeUnit.Year, NationalId: null, Address: null,
            AccountType: AccountType.Individual, IsVip: false, RegistrationDateUtc: Clock.AddHours(-2),
            PickupDateUtc: null, IsFastingIndicated: false, FastingHours: null, RecentContrastImaging: false,
            Notes: null, TreatingDoctorId: null, TreatingDoctorName: null, ReferralEntityId: null,
            ReferralEntityName: null, PhoneNumbers: Array.Empty<PatientPhoneNumberDto>(),
            MedicalConditions: Array.Empty<PatientMedicalConditionDto>(), IsDeleted: false);

        private static VisitHistoryDto Visit() => new(
            PatientId: PatientIdValue, LabId: "LAB-A", RegistrationDateUtc: Clock.AddHours(-2),
            Tests: Array.Empty<PatientTestSummaryDto>());

        private static VisitWorkSheetDto Sheet() => new(
            PatientId: PatientIdValue, PatientFullName: "Patient 7", LabId: "LAB-A",
            Sections: Array.Empty<WorkSheetSectionDto>(), Samples: Array.Empty<VisitWorkSheetSampleDto>(),
            TotalTests: 0, PrintFileExternalBarcode: false, PrintDateTimeOnTubeBarcode: false,
            PrintLabIdInsteadOfPatientId: false);

        private PatientAccountDto Account() => new(
            PatientId: PatientIdValue, PatientFullName: "Patient 7", LabId: "LAB-A",
            AccountType: "Individual", TotalCharged: TotalCharged, TotalPaid: TotalPaid,
            TotalDiscount: 0m, Balance: TotalCharged - TotalPaid,
            ChargedTests: Array.Empty<ChargedTestDto>(), Operations: Array.Empty<PaymentOperationDto>());
    }

    /// <summary>Records which ViewModel the editor navigated to, including the generic overload.</summary>
    private sealed class RecordingNavigation : INavigationService
    {
        public List<Type> Targets { get; } = new();
        public ViewModelBase? CurrentViewModel { get; private set; }

        public event Action<ViewModelBase?>? Navigated
        {
            add { }
            remove { }
        }

        public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
        {
            Targets.Add(typeof(TViewModel));
            CurrentViewModel = null;
        }

        public void NavigateTo(ViewModelBase viewModel)
        {
            Targets.Add(viewModel.GetType());
            CurrentViewModel = viewModel;
        }
    }

    private sealed class DecliningDialogService : IDialogService
    {
        public int ConfirmationCalls { get; private set; }
        public Task<bool> ShowConfirmationAsync(string title, string message)
        {
            ConfirmationCalls++;
            return Task.FromResult(false);
        }
        public Task ShowErrorAsync(string message) => Task.CompletedTask;
        public Task<bool> ShowSecondaryPasswordDialogAsync() => Task.FromResult(true);
        public Task<string?> PickBackupFolderAsync(string initialDirectory) => Task.FromResult<string?>(null);
        public Task<string?> PickBackupFileAsync() => Task.FromResult<string?>(null);
        public Task<string?> PickPdfSavePathAsync(string? suggestedFileName = null) => Task.FromResult<string?>(null);
    }

    private sealed class StubServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private static async Task<(PatientEditorViewModel Vm, EditorSender Sender, RecordingNavigation Nav)> LoadedAsync(
        IDialogService? dialogs = null, EditorSender? sender = null)
    {
        sender ??= new EditorSender();
        var nav = new RecordingNavigation();
        var vm = new PatientEditorViewModel(
            sender, nav, new ResultErrorPresenter(), dialogs ?? new FakeDialogService(), new StubServiceProvider());
        await vm.LoadPatientAsync(PatientIdValue);
        return (vm, sender, nav);
    }

    private static PatientEditorViewModel Unsaved(IDialogService? dialogs = null, EditorSender? sender = null)
        => new(sender ?? new EditorSender(), new RecordingNavigation(), new ResultErrorPresenter(),
            dialogs ?? new FakeDialogService(), new StubServiceProvider());

    /// <summary>AsyncRelayCommand.Execute is async void, so the command is awaited by yielding.</summary>
    private static async Task RunAsync(AsyncRelayCommand command)
    {
        command.Execute(null);
        await Task.Yield();
        await Task.Delay(80);
    }

    // ================================================================
    // A1 — the patient list opens in add mode and in edit mode
    // ================================================================

    [Fact]
    public async Task PatientWindow_OpensTheList_WhenThePatientIsSaved()
    {
        var (vm, _, nav) = await LoadedAsync();

        await RunAsync(vm.PatientWindowCommand);

        Assert.Contains(typeof(PatientSearchViewModel), nav.Targets);
        Assert.Empty(vm.ErrorMessage);
    }

    [Fact]
    public void PatientWindow_OpensTheList_WhenThePatientIsNotSaved()
    {
        var vm = Unsaved();
        var nav = (RecordingNavigation)typeof(PatientEditorViewModel)
            .GetField("_navigation", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .GetValue(vm)!;

        vm.PatientWindowCommand.Execute(null);

        Assert.Contains(typeof(PatientSearchViewModel), nav.Targets);
        Assert.Empty(vm.ErrorMessage);
    }

    [Fact]
    public void PatientWindow_DoesNotAskForAConfirmation_NoSuchPatternExistsInTheEditor()
    {
        var dialogs = new DecliningDialogService();
        var vm = Unsaved(dialogs);

        vm.PatientWindowCommand.Execute(null);

        Assert.Equal(0, dialogs.ConfirmationCalls);
    }

    // ================================================================
    // A1 — the lab-results guard stays
    // ================================================================

    [Fact]
    public async Task LabResults_OnAnUnsavedPatient_ShowsTheSaveFirstMessageAndNavigatesNowhere()
    {
        var vm = Unsaved();
        var nav = (RecordingNavigation)typeof(PatientEditorViewModel)
            .GetField("_navigation", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .GetValue(vm)!;

        await RunAsync(vm.LabResultsCommand);

        Assert.Equal("احفظ بيانات المريض أولًا قبل عرض النتائج.", vm.ErrorMessage);
        Assert.Empty(nav.Targets);
    }

    [Fact]
    public async Task LabResults_OnASavedPatient_NavigatesToTheResultSheet()
    {
        var (vm, _, nav) = await LoadedAsync();

        await RunAsync(vm.LabResultsCommand);

        Assert.Contains(typeof(PatientResultSheetViewModel), nav.Targets);
    }

    // ================================================================
    // 1.2.2.6.10 — the follow-up action
    // ================================================================

    [Fact]
    public async Task FollowUp_OnAnUnsavedPatient_ShowsTheSaveFirstMessageAndNavigatesNowhere()
    {
        var vm = Unsaved();
        var nav = (RecordingNavigation)typeof(PatientEditorViewModel)
            .GetField("_navigation", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .GetValue(vm)!;

        await RunAsync(vm.FollowUpCommand);

        Assert.Equal("احفظ بيانات المريض أولًا قبل عرض المتابعة.", vm.ErrorMessage);
        Assert.Empty(nav.Targets);
    }

    [Fact]
    public async Task FollowUp_OnASavedPatient_OpensTheCaseHistoryScreen()
    {
        var (vm, _, nav) = await LoadedAsync();

        await RunAsync(vm.FollowUpCommand);

        Assert.Contains(typeof(TopLab.Presentation.ViewModels.Audit.AuditViewModel), nav.Targets);
        Assert.Empty(vm.ErrorMessage);
    }

    // ================================================================
    // A3 (2) — the envelope command
    // ================================================================

    [Fact]
    public async Task PrintEnvelope_OnASavedPatient_SendsTheCommand()
    {
        var (vm, sender, _) = await LoadedAsync();

        await RunAsync(vm.PrintEnvelopeCommand);

        Assert.Equal(1, sender.EnvelopeCalls);
        Assert.Empty(vm.ErrorMessage);
        Assert.Equal("تمت طباعة المغلّف.", vm.StatusMessage);
    }

    [Fact]
    public async Task PrintEnvelope_OnAnUnsavedPatient_ShowsTheSaveFirstMessageAndSendsNothing()
    {
        var vm = Unsaved();
        var sender = new EditorSender();
        var vm2 = Unsaved(sender: sender);

        await RunAsync(vm2.PrintEnvelopeCommand);

        Assert.Equal(0, sender.EnvelopeCalls);
        Assert.Equal("احفظ بيانات المريض أولًا قبل الطباعة.", vm2.ErrorMessage);
        Assert.NotNull(vm);
    }

    // ================================================================
    // A3 (4) / B3 — the two-press Enter state machine never writes on the first press
    // ================================================================

    [Fact]
    public async Task FirstEnterPress_DisplaysFiguresAndWritesNothing()
    {
        var (vm, sender, _) = await LoadedAsync();
        vm.PaymentAmountText = "50";
        vm.PaymentDiscountText = "10";

        await RunAsync(vm.AccountEntryEnterCommand);

        Assert.Equal(0, sender.PaymentCalls);
        Assert.Equal(0, sender.ExtraChargeCalls);
        Assert.False(string.IsNullOrWhiteSpace(vm.StatusMessage));
        Assert.Empty(vm.ErrorMessage);
    }

    [Fact]
    public async Task SecondEnterPress_WithUnchangedInputs_RecordsThroughTheSameCommandAsTheButton()
    {
        var (vm, sender, _) = await LoadedAsync();
        vm.PaymentAmountText = "50";
        vm.PaymentDiscountText = "10";

        await RunAsync(vm.AccountEntryEnterCommand);
        await RunAsync(vm.AccountEntryEnterCommand);

        Assert.Equal(1, sender.PaymentCalls);
        Assert.Equal(50m, sender.Payments[0].Amount);
        Assert.Equal(10m, sender.Payments[0].DiscountAmount);
    }

    [Fact]
    public async Task EditingAnInputAfterTheFirstPress_ResetsToTheFirstPressState()
    {
        var (vm, sender, _) = await LoadedAsync();
        vm.PaymentAmountText = "50";
        await RunAsync(vm.AccountEntryEnterCommand);

        vm.PaymentAmountText = "60";
        await RunAsync(vm.AccountEntryEnterCommand);

        Assert.Equal(0, sender.PaymentCalls);
    }

    [Fact]
    public async Task InvalidInput_ShowsAnErrorAndWritesNothing()
    {
        var (vm, sender, _) = await LoadedAsync();
        vm.PaymentAmountText = "abc";

        await RunAsync(vm.AccountEntryEnterCommand);

        Assert.Equal(0, sender.PaymentCalls);
        Assert.Equal("قيمة المدفوع غير صالحة.", vm.ErrorMessage);
    }

    // ================================================================
    // B1 — the percentage field
    // ================================================================

    [Fact]
    public async Task PercentageEnter_ConvertsAgainstTheTotalAccountAmountIntoTheValueField()
    {
        var (vm, sender, _) = await LoadedAsync();
        vm.PaymentAmountText = "100";
        vm.PaymentPercentageText = "10";

        await RunAsync(vm.AccountEntryEnterCommand);

        // 10% of the patient's TOTAL account amount (200) = 20, written into the value field.
        Assert.Equal("20", vm.PaymentDiscountText);
        Assert.Equal(0, sender.PaymentCalls);
    }

    [Fact]
    public async Task PercentageOutOfRange_ShowsTheArabicMessageAndWritesNothing()
    {
        var (vm, sender, _) = await LoadedAsync();
        vm.PaymentAmountText = "100";
        vm.PaymentPercentageText = "101";

        await RunAsync(vm.AccountEntryEnterCommand);

        Assert.Equal("نسبة الخصم يجب أن تكون أكبر من صفر ولا تتجاوز 100.", vm.ErrorMessage);
        Assert.Equal(0, sender.PaymentCalls);
    }

    [Fact]
    public async Task EditingTheValueField_ClearsThePercentage()
    {
        var (vm, _, _) = await LoadedAsync();
        vm.PaymentPercentageText = "10";

        vm.PaymentDiscountText = "5";

        Assert.Null(vm.PaymentPercentageText);
    }

    // ================================================================
    // B2 — the "+" surcharge convention routes to the existing extra-charge path
    // ================================================================

    [Fact]
    public async Task PlusPrefix_RoutesToTheExistingExtraChargeCommand()
    {
        var (vm, sender, _) = await LoadedAsync();
        vm.PaymentAmountText = "+30";

        await RunAsync(vm.RecordPaymentCommand);

        Assert.Equal(1, sender.ExtraChargeCalls);
        Assert.Equal(0, sender.PaymentCalls);
    }

    [Theory]
    [InlineData("+")]
    [InlineData("+0")]
    [InlineData("-5")]
    public async Task InvalidSurchargeEntries_AreRejectedWithAnArabicMessage(string text)
    {
        var (vm, sender, _) = await LoadedAsync();
        vm.PaymentAmountText = text;

        await RunAsync(vm.RecordPaymentCommand);

        Assert.Equal(0, sender.ExtraChargeCalls);
        Assert.Equal(0, sender.PaymentCalls);
        Assert.False(string.IsNullOrWhiteSpace(vm.ErrorMessage));
    }

    [Fact]
    public async Task PlusPrefixWithADiscount_IsRejected()
    {
        var (vm, sender, _) = await LoadedAsync();
        vm.PaymentAmountText = "+30";
        vm.PaymentDiscountText = "5";

        await RunAsync(vm.RecordPaymentCommand);

        Assert.Equal(0, sender.ExtraChargeCalls);
        Assert.Equal(0, sender.PaymentCalls);
        Assert.Equal("لا يمكن إضافة مبلغ إضافي مع خصم.", vm.ErrorMessage);
    }

    // ================================================================
    // B4 — refund
    // ================================================================

    [Fact]
    public async Task Refund_OnAnUnsavedPatient_ShowsTheSaveFirstMessage()
    {
        var vm = Unsaved(sender: new EditorSender());

        await RunAsync(vm.RefundCommand);

        Assert.Equal("احفظ بيانات المريض أولًا قبل تسجيل استرداد.", vm.ErrorMessage);
    }

    [Fact]
    public async Task Refund_WithAnInvalidAmount_IsRejectedBeforeTheMediator()
    {
        var (vm, sender, _) = await LoadedAsync();
        vm.RefundAmountText = "0";

        await RunAsync(vm.RefundCommand);

        Assert.Equal(0, sender.RefundCalls);
        Assert.Equal("قيمة المدفوع غير صالحة.", vm.ErrorMessage);
    }

    [Fact]
    public async Task Refund_WithAValidAmount_SendsTheCommandAndRefreshes()
    {
        var (vm, sender, _) = await LoadedAsync();
        vm.RefundAmountText = "25";

        await RunAsync(vm.RefundCommand);

        Assert.Equal(1, sender.RefundCalls);
        Assert.Equal("تم تسجيل الاسترداد.", vm.StatusMessage);
        Assert.Equal(string.Empty, vm.RefundAmountText);
    }

    [Fact]
    public async Task Refund_SurfacesTheHandlersValidationMessage()
    {
        var sender = new EditorSender { RefundResult = Result<int>.Failure(Error.Validation("مبلغ الاسترداد يتجاوز إجمالي المدفوع الصافي (10).")) };
        var (vm, _, _) = await LoadedAsync(sender: sender);
        vm.RefundAmountText = "50";

        await RunAsync(vm.RefundCommand);

        Assert.Contains("يتجاوز", vm.ErrorMessage, StringComparison.Ordinal);
    }

    // ================================================================
    // B5 — clear-account
    // ================================================================

    [Fact]
    public async Task ClearAccount_WhenTheConfirmationIsDeclined_DoesNothing()
    {
        var dialogs = new DecliningDialogService();
        var sender = new EditorSender();
        var (vm, _, _) = await LoadedAsync(dialogs, sender);

        await RunAsync(vm.ClearAccountCommand);

        Assert.Equal(1, dialogs.ConfirmationCalls);
        Assert.Equal(0, sender.ClearCalls);
        Assert.Empty(vm.ErrorMessage);
    }

    [Fact]
    public async Task ClearAccount_WhenConfirmed_SendsTheCommandAndRefreshes()
    {
        var (vm, sender, _) = await LoadedAsync();

        await RunAsync(vm.ClearAccountCommand);

        Assert.Equal(1, sender.ClearCalls);
        Assert.Equal("تم تصفير الحساب (2 عملية).", vm.StatusMessage);
    }

    [Fact]
    public async Task ClearAccount_OnAnUnsavedPatient_ShowsTheSaveFirstMessage()
    {
        var dialogs = new DecliningDialogService();
        var vm = Unsaved(dialogs);

        await RunAsync(vm.ClearAccountCommand);

        Assert.Equal("احفظ بيانات المريض أولًا قبل تصفير الحساب.", vm.ErrorMessage);
        Assert.Equal(0, dialogs.ConfirmationCalls);
    }

    // ================================================================
    // XAML wiring (read as text — no WPF element is instantiated)
    // ================================================================

    [Fact]
    public void View_DisablesTheGuardedActionsWhenNoPatientIsLoaded()
    {
        var xaml = ReadPresentationFile(Path.Combine("Views", "Patients", "PatientEditorView.xaml"));

        // Stage 3 §3.5 GAP-10: "actions are disabled when no patient is loaded."
        // The patient-window action is the owner-decided exception (it must open in add mode).
        Assert.Contains("Command=\"{Binding PrintEnvelopeCommand}\" IsEnabled=\"{Binding IsEditMode}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Command=\"{Binding LabResultsCommand}\" IsEnabled=\"{Binding IsEditMode}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Command=\"{Binding FollowUpCommand}\" IsEnabled=\"{Binding IsEditMode}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Command=\"{Binding PatientWindowCommand}\"", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void View_BindsTheEnterKeyAndTheNewAccountActions()
    {
        var xaml = ReadPresentationFile(Path.Combine("Views", "Patients", "PatientEditorView.xaml"));

        Assert.Contains("Key=\"Enter\" Command=\"{Binding AccountEntryEnterCommand}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Command=\"{Binding RefundCommand}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Command=\"{Binding ClearAccountCommand}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Binding PaymentPercentageText", xaml, StringComparison.Ordinal);
        Assert.Contains("Binding RefundAmountText", xaml, StringComparison.Ordinal);
        Assert.Contains("FlowDirection=\"RightToLeft\"", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void AccountView_BindsTheEnterKeyAndTheNewAccountActions()
    {
        var xaml = ReadPresentationFile(Path.Combine("Views", "Patients", "PatientAccountView.xaml"));

        Assert.Contains("Key=\"Enter\" Command=\"{Binding AccountEntryEnterCommand}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Command=\"{Binding RefundCommand}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Command=\"{Binding ClearAccountCommand}\"", xaml, StringComparison.Ordinal);
    }

    private static string ReadPresentationFile(string relativePath)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "src", "TopLab.Presentation")))
        {
            dir = dir.Parent;
        }

        Assert.NotNull(dir);
        return File.ReadAllText(Path.Combine(dir!.FullName, "src", "TopLab.Presentation", relativePath));
    }
}
