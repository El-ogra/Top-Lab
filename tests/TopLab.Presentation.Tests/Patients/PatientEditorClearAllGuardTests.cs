using System.IO;
using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientRegistration.Commands.ClearAllTests;
using TopLab.Application.Features.PatientBilling.Common;
using TopLab.Application.Features.PatientBilling.Queries.GetPatientAccount;
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
using TopLab.Presentation.ViewModels.Patients;
using Xunit;

namespace TopLab.Presentation.Tests.Patients;

/// <summary>
/// R-A04 (VG-07) — <c>CanClearAllVisitTests</c> and the stale-command short-circuit on
/// <see cref="PatientEditorViewModel"/>.
///
/// Hand-rolled nested fakes (SD-14): <c>Common/Fakes.cs</c> is NOT modified.
///
/// SD-19 honoured: no test here instantiates a WPF element. The ViewModel is driven through
/// its real load path and asserted on its public properties; the button binding is asserted
/// by reading the XAML as text, which is the established pattern
/// (<c>PriceListsPrintCommandTests.PriceLists_PrintCommand_IsReachableFromTheView</c>).
/// </summary>
public class PatientEditorClearAllGuardTests
{
    private static readonly DateTime Clock = new(2026, 6, 15, 12, 0, 0, DateTimeKind.Utc);

    private const string GuardMessage = "لا يمكن مسح التحاليل إلا عند إضافة المريض أول مرة.";

    /// <summary>
    /// Canned answers for the queries <see cref="PatientEditorViewModel"/> issues while
    /// loading a saved patient. Only those three are needed; anything else throws so a new
    /// call site cannot silently pass.
    /// </summary>
    private sealed class EditorSender : ISender
    {
        private readonly int _currentPatientId;
        private readonly IReadOnlyList<VisitHistoryDto> _history;

        public int ClearAllCallCount { get; private set; }

        public EditorSender(int currentPatientId, IReadOnlyList<VisitHistoryDto> history)
        {
            _currentPatientId = currentPatientId;
            _history = history;
        }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is GetPatientByIdQuery)
            {
                return Task.FromResult((TResponse)(object)Result<PatientDetailDto>.Success(PatientDetail(_currentPatientId)));
            }

            if (request is GetPatientVisitHistoryQuery)
            {
                return Task.FromResult((TResponse)(object)Result<IReadOnlyList<VisitHistoryDto>>.Success(_history));
            }

            if (request is GetVisitWorkSheetQuery)
            {
                return Task.FromResult((TResponse)(object)Result<VisitWorkSheetDto>
                    .Success(EmptySheet(_currentPatientId)));
            }

            if (request is GetPatientAccountQuery)
            {
                return Task.FromResult((TResponse)(object)Result<PatientAccountDto>
                    .Success(EmptyAccount(_currentPatientId)));
            }

            if (request is ClearAllTestsCommand)
            {
                ClearAllCallCount++;
                return Task.FromResult((TResponse)(object)Result<int>.Success(0));
            }

            throw new NotSupportedException($"EditorSender has no canned response for {request.GetType().Name}.");
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : IRequest
            => throw new NotSupportedException();

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class FixedDialogService : IDialogService
    {
        public Task<bool> ShowConfirmationAsync(string title, string message) => Task.FromResult(true);
        public Task ShowErrorAsync(string message) => Task.CompletedTask;
        public Task<bool> ShowSecondaryPasswordDialogAsync() => Task.FromResult(true);
        public Task<string?> PickBackupFolderAsync(string initialDirectory) => Task.FromResult<string?>(null);
        public Task<string?> PickBackupFileAsync() => Task.FromResult<string?>(null);
        public Task<string?> PickPdfSavePathAsync(string? suggestedFileName = null) => Task.FromResult<string?>(null);
    }

    private sealed class NoOpNavigation : INavigationService
    {
        public ViewModelBase? CurrentViewModel => null;

        public event Action<ViewModelBase?>? Navigated;

        public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
            => Navigated?.Invoke(null);

        public void NavigateTo(ViewModelBase viewModel)
            => Navigated?.Invoke(viewModel);
    }

    private static VisitHistoryDto Visit(int patientId, DateTime registeredUtc) => new(
        PatientId: patientId,
        LabId: "LAB-A",
        RegistrationDateUtc: registeredUtc,
        Tests: Array.Empty<PatientTestSummaryDto>());

    private static PatientDetailDto PatientDetail(int patientId) => new(
        PatientId: patientId,
        LabId: "LAB-A",
        Title: null,
        FullName: $"Patient {patientId}",
        Sex: Sex.Male,
        AgeValue: 30,
        AgeUnit: AgeUnit.Year,
        NationalId: null,
        Address: null,
        AccountType: AccountType.Individual,
        IsVip: false,
        RegistrationDateUtc: Clock.AddHours(-2),
        PickupDateUtc: null,
        IsFastingIndicated: false,
        FastingHours: null,
        RecentContrastImaging: false,
        Notes: null,
        TreatingDoctorId: null,
        TreatingDoctorName: null,
        ReferralEntityId: null,
        ReferralEntityName: null,
        PhoneNumbers: Array.Empty<PatientPhoneNumberDto>(),
        MedicalConditions: Array.Empty<PatientMedicalConditionDto>(),
        IsDeleted: false);

    private static PatientAccountDto EmptyAccount(int patientId) => new(
        PatientId: patientId,
        PatientFullName: $"Patient {patientId}",
        LabId: "LAB-A",
        AccountType: "Individual",
        TotalCharged: 0m,
        TotalPaid: 0m,
        TotalDiscount: 0m,
        Balance: 0m,
        ChargedTests: Array.Empty<ChargedTestDto>(),
        Operations: Array.Empty<PaymentOperationDto>());

    private static VisitWorkSheetDto EmptySheet(int patientId) => new(
        PatientId: patientId,
        PatientFullName: $"Patient {patientId}",
        LabId: "LAB-A",
        Sections: Array.Empty<WorkSheetSectionDto>(),
        Samples: Array.Empty<VisitWorkSheetSampleDto>(),
        TotalTests: 0,
        PrintFileExternalBarcode: false,
        PrintDateTimeOnTubeBarcode: false,
        PrintLabIdInsteadOfPatientId: false);

    /// <summary>
    /// Drives the ViewModel's real load path for a saved patient. The load is triggered by
    /// the public SaveAsync path's load branch, so no private method is invoked directly.
    /// </summary>
    private static async Task<(PatientEditorViewModel Vm, EditorSender Sender)> LoadedAsync(
        int currentPatientId,
        IReadOnlyList<VisitHistoryDto> history)
    {
        var sender = new EditorSender(currentPatientId, history);
        var vm = new PatientEditorViewModel(
            sender,
            new NoOpNavigation(),
            new ResultErrorPresenter(),
            new FixedDialogService(),
            new StubServiceProvider());

        await vm.LoadPatientAsync(currentPatientId);
        return (vm, sender);
    }

    private sealed class StubServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    /// <summary>
    /// AsyncRelayCommand.Execute is async void (RelayCommand.cs:51), so there is nothing to
    /// await; yield until the command has finished.
    /// </summary>
    private static async Task RunAsync(AsyncRelayCommand command)
    {
        command.Execute(null);
        await Task.Yield();
        await Task.Delay(50);
    }

    // =====================================================================
    // CanClearAllVisitTests
    // =====================================================================

    [Fact]
    public async Task FirstVisit_CanClearAll_IsTrue()
    {
        // Only visit in the group — the current one is the first registration.
        var (vm, _) = await LoadedAsync(2, new[] { Visit(2, Clock.AddHours(-2)) });

        Assert.True(vm.CanClearAllVisitTests);
    }

    [Fact]
    public async Task NonFirstVisit_CanClearAll_IsFalse()
    {
        // Patient 1 registered a day earlier in the same LabId group.
        var (vm, _) = await LoadedAsync(
            2,
            new[] { Visit(1, Clock.AddDays(-1)), Visit(2, Clock.AddHours(-2)) });

        Assert.False(vm.CanClearAllVisitTests);
    }

    [Fact]
    public async Task LaterRegisteredVisit_IsNotTheFirstVisit()
    {
        // Guards an inverted comparison: the LOWER PatientId registered earlier is first.
        var (vm, _) = await LoadedAsync(
            8,
            new[] { Visit(7, Clock.AddHours(-5)), Visit(8, Clock.AddHours(-1)) });

        Assert.False(vm.CanClearAllVisitTests);
    }

    [Fact]
    public async Task TieOnRegistrationDate_IsBrokenByTheLowerPatientId()
    {
        var same = Clock.AddHours(-2);
        var (vm, _) = await LoadedAsync(2, new[] { Visit(1, same), Visit(2, same) });

        // Patient 2 has the higher id, so patient 1 is the first visit.
        Assert.False(vm.CanClearAllVisitTests);
    }

    [Fact]
    public async Task TieOnRegistrationDate_LowerPatientIdIsTheFirstVisit()
    {
        var same = Clock.AddHours(-2);
        var (vm, _) = await LoadedAsync(1, new[] { Visit(1, same), Visit(2, same) });

        Assert.True(vm.CanClearAllVisitTests);
    }

    [Fact]
    public async Task SingleVisit_CanClearAll_IsTrue_AfterReload()
    {
        // The refresh happens inside LoadVisitTestsAsync, so it also covers the add / remove /
        // clear-all paths, all of which call it. Loading proves the refresh ran.
        var (vm, _) = await LoadedAsync(5, new[] { Visit(5, Clock.AddHours(-2)) });

        Assert.Single(vm.VisitHistory);
        Assert.True(vm.CanClearAllVisitTests);
    }

    // =====================================================================
    // The stale-command short-circuit (BR-A04-5)
    // =====================================================================

    [Fact]
    public async Task ClearAll_OnNonFirstVisit_IsRefusedWithoutReachingTheMediator()
    {
        var (vm, sender) = await LoadedAsync(
            2,
            new[] { Visit(1, Clock.AddDays(-1)), Visit(2, Clock.AddHours(-2)) });

        await RunAsync(vm.ClearAllVisitTestsCommand);

        Assert.Equal(0, sender.ClearAllCallCount);
        Assert.Equal(GuardMessage, vm.ErrorMessage);
    }

    [Fact]
    public async Task ClearAll_OnFirstVisit_ReachesTheMediator()
    {
        var (vm, sender) = await LoadedAsync(2, new[] { Visit(2, Clock.AddHours(-2)) });

        await RunAsync(vm.ClearAllVisitTestsCommand);

        Assert.Equal(1, sender.ClearAllCallCount);
    }

    // =====================================================================
    // Shared literal (SD-17) and the XAML binding
    // =====================================================================

    [Fact]
    public void ViewModel_UsesTheHandlersOwnArabicLiteral()
    {
        var source = ReadPresentationFile(Path.Combine("ViewModels", "Patients", "PatientEditorViewModel.cs"));

        // The short-circuit must reference the handler's constant, not a second copy of the
        // sentence (SD-17 / VG-07).
        Assert.Contains("ClearAllTestsCommandHandler.FirstRegistrationOnlyMessage", source, StringComparison.Ordinal);
        Assert.Equal(GuardMessage, ClearAllTestsCommandHandler.FirstRegistrationOnlyMessage);
    }

    [Fact]
    public void View_BindsIsEnabled_OnTheClearAllButton()
    {
        var xaml = ReadPresentationFile(Path.Combine("Views", "Patients", "PatientEditorView.xaml"));

        Assert.Contains("Content=\"مسح الكل\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Command=\"{Binding ClearAllVisitTestsCommand}\" IsEnabled=\"{Binding CanClearAllVisitTests}\"",
            xaml, StringComparison.Ordinal);

        // RTL preserved (PresentationStructuralTests scans every Views/**/*.xaml)
        Assert.Contains("FlowDirection=\"RightToLeft\"", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void ViewModel_StillHasNoInfrastructureReference()
    {
        var source = ReadPresentationFile(Path.Combine("ViewModels", "Patients", "PatientEditorViewModel.cs"));

        Assert.DoesNotContain("TopLab.Infrastructure", source, StringComparison.Ordinal);
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