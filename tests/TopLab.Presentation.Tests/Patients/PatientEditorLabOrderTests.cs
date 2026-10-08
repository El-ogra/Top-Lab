using System.IO;
using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientBilling.Common;
using TopLab.Application.Features.PatientBilling.Queries.GetPatientAccount;
using TopLab.Application.Features.PatientEnvelope.Commands.PrintLabOrder;
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
/// Phase 1 REF-068: the registration-screen lab-order trigger.
/// Proves the <c>طلب تحاليل</c> button dispatches <c>PrintLabOrderCommand</c>
/// with the loaded visit id through the ViewModel's real load path, and that
/// the button is wired in the view next to the sibling print actions.
/// </summary>
public class PatientEditorLabOrderTests
{
    private sealed class LabOrderSender : ISender
    {
        private readonly int _currentPatientId;

        public List<PrintLabOrderCommand> LabOrderCommands { get; } = new();

        public Result? LabOrderResult { get; set; }

        public LabOrderSender(int currentPatientId)
        {
            _currentPatientId = currentPatientId;
        }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is GetPatientByIdQuery)
            {
                return Task.FromResult((TResponse)(object)Result<PatientDetailDto>.Success(PatientDetail(_currentPatientId)));
            }

            if (request is GetPatientVisitHistoryQuery)
            {
                return Task.FromResult((TResponse)(object)Result<IReadOnlyList<VisitHistoryDto>>.Success(
                    new[] { Visit(_currentPatientId, DateTime.UtcNow.AddHours(-2)) }));
            }

            if (request is GetVisitWorkSheetQuery)
            {
                return Task.FromResult((TResponse)(object)Result<VisitWorkSheetDto>.Success(EmptySheet(_currentPatientId)));
            }

            if (request is GetPatientAccountQuery)
            {
                return Task.FromResult((TResponse)(object)Result<PatientAccountDto>.Success(EmptyAccount(_currentPatientId)));
            }

            if (request is PrintLabOrderCommand labOrder)
            {
                LabOrderCommands.Add(labOrder);
                return Task.FromResult((TResponse)(object)(LabOrderResult ?? Result.Success()));
            }

            throw new NotSupportedException($"LabOrderSender has no canned response for {request.GetType().Name}.");
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

    private sealed class StubServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
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
        RegistrationDateUtc: DateTime.UtcNow.AddHours(-2),
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

    private static async Task<(PatientEditorViewModel Vm, LabOrderSender Sender)> LoadedAsync(int patientId)
    {
        var sender = new LabOrderSender(patientId);
        var vm = new PatientEditorViewModel(
            sender,
            new NoOpNavigation(),
            new ResultErrorPresenter(),
            new FixedDialogService(),
            new StubServiceProvider());

        await vm.LoadPatientAsync(patientId);
        return (vm, sender);
    }

    [Fact]
    public async Task PrintLabOrderCommand_DispatchesWithLoadedPatientId()
    {
        var (vm, sender) = await LoadedAsync(2);

        vm.PrintLabOrderCommand.Execute(null);
        await Task.Yield();
        await Task.Delay(100);

        var command = Assert.Single(sender.LabOrderCommands);
        Assert.Equal(2, command.PatientId);
        Assert.NotEmpty(vm.StatusMessage);
        Assert.Empty(vm.ErrorMessage);
    }

    [Fact]
    public async Task PrintLabOrderCommand_SurfacesFailure()
    {
        var (vm, sender) = await LoadedAsync(2);
        sender.LabOrderResult = Result.Failure(Error.NotFound("المريض غير موجود."));

        vm.PrintLabOrderCommand.Execute(null);
        await Task.Yield();
        await Task.Delay(100);

        Assert.NotEmpty(vm.ErrorMessage);
        Assert.Empty(vm.StatusMessage);
    }

    [Fact]
    public void LabOrderButton_IsPresentInTheView()
    {
        var xaml = File.ReadAllText(Path.Combine(
            FindSolutionRoot(), "src", "TopLab.Presentation", "Views", "Patients", "PatientEditorView.xaml"));

        Assert.Contains("Command=\"{Binding PrintLabOrderCommand}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("طلب تحاليل", xaml, StringComparison.Ordinal);

        foreach (var token in new[] { "باركود", "ورقة العمل", "الإيصال", "الفاتورة" })
        {
            Assert.Contains(token, xaml, StringComparison.Ordinal);
        }
    }

    private static string FindSolutionRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "src", "TopLab.Presentation")))
        {
            dir = dir.Parent;
        }

        Assert.NotNull(dir);
        return dir!.FullName;
    }
}
