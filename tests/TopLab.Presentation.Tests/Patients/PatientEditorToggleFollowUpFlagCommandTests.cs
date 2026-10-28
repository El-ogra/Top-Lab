using System.IO;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientRegistration.Common;
using TopLab.Application.Features.PatientRegistration.Queries.GetPatientById;
using TopLab.Application.Features.PatientRegistration.Queries.GetPatientVisitHistory;
using TopLab.Application.Features.PatientBilling.Common;
using TopLab.Application.Features.PatientBilling.Queries.GetPatientAccount;
using TopLab.Application.Features.WorkSheets.Common;
using TopLab.Application.Features.WorkSheets.Queries.GetVisitWorkSheet;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Common.Navigation;
using TopLab.Presentation.ViewModels.Patients;
using MediatR;
using Xunit;

namespace TopLab.Presentation.Tests.Patients;

public class PatientEditorToggleFollowUpFlagCommandTests
{
    [Fact]
    public void ToggleFollowUpFlagCommand_IsConstructed()
    {
        var vm = new PatientEditorViewModel(
            new NoOpSender(),
            new NoOpNavigation(),
            new ResultErrorPresenter(),
            new FixedDialogService(),
            new StubServiceProvider());

        Assert.NotNull(vm.ToggleFollowUpFlagCommand);
    }

    [Fact]
    public void ToggleFollowUpFlagCommand_WiredToDistinctLabeledControlInTheView()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "src", "TopLab.Presentation")))
        {
            dir = dir.Parent;
        }

        Assert.NotNull(dir);
        var xaml = File.ReadAllText(Path.Combine(dir!.FullName, "src", "TopLab.Presentation", "Views", "Patients", "PatientEditorView.xaml"));

        Assert.Contains("Content=\"يحتاج متابعة\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Command=\"{Binding ToggleFollowUpFlagCommand}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("IsEnabled=\"{Binding IsEditMode}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Content=\"متابعة\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Command=\"{Binding FollowUpCommand}\"", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public async Task LoadPatient_FlagsReflected()
    {
        var clock = new DateTime(2026, 6, 15, 12, 0, 0, DateTimeKind.Utc);

        var sender = new EditorSender(clock);
        var vm = new PatientEditorViewModel(
            sender,
            new NoOpNavigation(),
            new ResultErrorPresenter(),
            new FixedDialogService(),
            new StubServiceProvider());

        await vm.LoadPatientAsync(7);

        Assert.True(vm.IsFollowUpRequired);
        Assert.NotNull(vm.ToggleFollowUpFlagCommand);
    }

    private sealed class EditorSender : ISender
    {
        private readonly DateTime _clock;

        public EditorSender(DateTime clock)
        {
            _clock = clock;
        }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is GetPatientByIdQuery)
            {
                return Task.FromResult((TResponse)(object)Result<PatientDetailDto>.Success(PatientDetail(_clock)));
            }

            if (request is GetPatientVisitHistoryQuery)
            {
                return Task.FromResult((TResponse)(object)Result<IReadOnlyList<VisitHistoryDto>>.Success(Array.Empty<VisitHistoryDto>()));
            }

            if (request is GetVisitWorkSheetQuery)
            {
                return Task.FromResult((TResponse)(object)Result<VisitWorkSheetDto>.Success(EmptySheet()));
            }

            if (request is GetPatientAccountQuery)
            {
                return Task.FromResult((TResponse)(object)Result<PatientAccountDto>.Success(new PatientAccountDto(
                    PatientId: 7,
                    PatientFullName: "Ahmed Mohamed",
                    LabId: "LAB-A",
                    AccountType: "فردي",
                    TotalCharged: 0,
                    TotalPaid: 0,
                    TotalDiscount: 0,
                    Balance: 0,
                    ChargedTests: Array.Empty<ChargedTestDto>(),
                    Operations: Array.Empty<PaymentOperationDto>())));
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

        private static PatientDetailDto PatientDetail(DateTime clock) => new(
            PatientId: 7,
            LabId: "LAB-A",
            Title: null,
            FullName: "Ahmed Mohamed",
            Sex: Sex.Male,
            AgeValue: 30,
            AgeUnit: AgeUnit.Year,
            NationalId: null,
            Address: null,
            AccountType: AccountType.Individual,
            IsVip: false,
            RegistrationDateUtc: clock.AddHours(-2),
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
            IsDeleted: false,
            CardNumber: null,
            RecentUltrasoundImaging: false,
            IsFollowUpRequired: true);

        private static VisitWorkSheetDto EmptySheet() => new(
            PatientId: 7,
            PatientFullName: "Ahmed Mohamed",
            LabId: "LAB-A",
            Sections: Array.Empty<WorkSheetSectionDto>(),
            Samples: Array.Empty<VisitWorkSheetSampleDto>(),
            TotalTests: 0,
            PrintFileExternalBarcode: false,
            PrintDateTimeOnTubeBarcode: false,
            PrintLabIdInsteadOfPatientId: false);
    }

    private sealed class NoOpSender : ISender
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException($"No canned response for {request.GetType().Name}.");

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

    private sealed class NoOpNavigation : INavigationService
    {
        public ViewModelBase? CurrentViewModel => null;
        public event Action<ViewModelBase?>? Navigated;
        public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase => Navigated?.Invoke(null);
        public void NavigateTo(ViewModelBase viewModel) => Navigated?.Invoke(viewModel);
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

    private sealed class StubServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }
}
