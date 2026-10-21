using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.AnalyteProfiles.Common;
using TopLab.Application.Features.AnalyteProfiles.Queries.GetProfileDefinitions;
using TopLab.Application.Features.PatientBilling.Common;
using TopLab.Application.Features.PatientBilling.Queries.GetPatientAccount;
using TopLab.Application.Features.PatientRegistration.Common;
using TopLab.Application.Features.PatientRegistration.Commands.CreatePatient;
using TopLab.Application.Features.PatientRegistration.Commands.UpdatePatient;
using TopLab.Application.Features.PatientRegistration.Queries.GetNextLabId;
using TopLab.Application.Features.PatientRegistration.Queries.GetPatientById;
using TopLab.Application.Features.PatientRegistration.Queries.GetPatientVisitHistory;
using TopLab.Application.Features.PatientRegistration.Queries.GetRegistrationCatalog;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Common;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Queries.GetCustomGroups;
using TopLab.Application.Features.SystemAndPrintSettings.Commands.UpdatePrintResultPassword;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Common;
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
/// STAGE-01 (baseline 1.2.2.3): the medical-history block of the registration
/// screen — the fifteen questions, the pregnancy rule for female patients
/// only, the read-only sample-type summary, the card number, the visit dates
/// and the print-result password.
/// </summary>
public class PatientEditorMedicalHistoryTests
{
    private static readonly DateTime Registration = new(2026, 3, 1, 8, 0, 0, DateTimeKind.Utc);

    private static IReadOnlyList<MedicalConditionTypeDto> SeededCatalogue() => new[]
    {
        new MedicalConditionTypeDto(2, "هل يؤخذ علاج للسكر", MedicalConditionCategory.Medication),
        new MedicalConditionTypeDto(3, "هل يؤخذ علاج لسيولة الدم", MedicalConditionCategory.Medication),
        new MedicalConditionTypeDto(4, "هل يؤخذ علاج للغدد", MedicalConditionCategory.Medication),
        new MedicalConditionTypeDto(5, "هل يؤخذ علاج للفيروسات", MedicalConditionCategory.Medication),
        new MedicalConditionTypeDto(6, "هل يؤخذ مضاد حيوي", MedicalConditionCategory.Medication),
        new MedicalConditionTypeDto(7, "هل يؤخذ علاج للكبد", MedicalConditionCategory.Medication),
        new MedicalConditionTypeDto(1, "حمل", MedicalConditionCategory.Pregnancy),
        new MedicalConditionTypeDto(8, "مريض بفقر الدم", MedicalConditionCategory.Condition),
        new MedicalConditionTypeDto(9, "مريض بالتهاب المفاصل", MedicalConditionCategory.Condition),
        new MedicalConditionTypeDto(10, "مريض بالضغط الدم", MedicalConditionCategory.Condition),
        new MedicalConditionTypeDto(11, "مريض بالفشل كلوي", MedicalConditionCategory.Condition),
        new MedicalConditionTypeDto(12, "مريض بالذئبة الحمراء", MedicalConditionCategory.Condition)
    };

    private sealed class StageOneSender : ISender
    {
        public List<UpdatePatientCommand> UpdateCommands { get; } = new();

        public List<CreatePatientCommand> CreateCommands { get; } = new();

        public List<UpdatePrintResultPasswordCommand> PasswordCommands { get; } = new();

        public Sex PatientSex { get; set; } = Sex.Male;

        public string? PatientNotes { get; set; }

        public bool PrintResultPasswordEnabled { get; set; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is GetRegistrationCatalogQuery)
            {
                var dto = new RegistrationCatalogDto(
                    Array.Empty<TestSummaryDto>(),
                    Array.Empty<TestGroupDto>(),
                    Array.Empty<PatientTitleDto>(),
                    SeededCatalogue(),
                    AccountType.Individual,
                    false,
                    "Himself",
                    "Herself",
                    PrintResultPasswordEnabled);
                return Task.FromResult((TResponse)(object)Result<RegistrationCatalogDto>.Success(dto));
            }

            if (request is GetProfileDefinitionsQuery)
            {
                return Task.FromResult((TResponse)(object)Result<IReadOnlyList<ProfileDefinitionDto>>.Success(
                    Array.Empty<ProfileDefinitionDto>()));
            }

            if (request is GetCustomGroupsQuery)
            {
                return Task.FromResult((TResponse)(object)Result<IReadOnlyList<CustomGroupSummaryDto>>.Success(
                    Array.Empty<CustomGroupSummaryDto>()));
            }

            if (request is GetNextLabIdQuery)
            {
                return Task.FromResult((TResponse)(object)Result<string>.Success("1"));
            }

            if (request is GetPatientByIdQuery)
            {
                return Task.FromResult((TResponse)(object)Result<PatientDetailDto>.Success(Detail()));
            }

            if (request is GetPatientVisitHistoryQuery)
            {
                return Task.FromResult((TResponse)(object)Result<IReadOnlyList<VisitHistoryDto>>.Success(
                    Array.Empty<VisitHistoryDto>()));
            }

            if (request is GetVisitWorkSheetQuery)
            {
                return Task.FromResult((TResponse)(object)Result<VisitWorkSheetDto>.Success(Sheet()));
            }

            if (request is GetPatientAccountQuery)
            {
                return Task.FromResult((TResponse)(object)Result<PatientAccountDto>.Success(Account()));
            }

            if (request is UpdatePatientCommand update)
            {
                UpdateCommands.Add(update);
                return Task.FromResult((TResponse)(object)Result<bool>.Success(true));
            }

            if (request is CreatePatientCommand create)
            {
                CreateCommands.Add(create);
                return Task.FromResult((TResponse)(object)Result<int>.Success(1));
            }

            if (request is UpdatePrintResultPasswordCommand password)
            {
                PasswordCommands.Add(password);
                return Task.FromResult((TResponse)(object)Result.Success());
            }

            throw new NotSupportedException($"StageOneSender has no canned response for {request.GetType().Name}.");
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

        private PatientDetailDto Detail() => new(
            PatientId: 1,
            LabId: "LAB-A",
            Title: null,
            FullName: "Ahmed",
            Sex: PatientSex,
            AgeValue: 30,
            AgeUnit: AgeUnit.Year,
            NationalId: null,
            Address: null,
            AccountType: AccountType.Individual,
            IsVip: false,
            RegistrationDateUtc: Registration,
            PickupDateUtc: Registration,
            IsFastingIndicated: false,
            FastingHours: null,
            RecentContrastImaging: false,
            Notes: PatientNotes,
            TreatingDoctorId: null,
            TreatingDoctorName: null,
            ReferralEntityId: null,
            ReferralEntityName: null,
            PhoneNumbers: Array.Empty<PatientPhoneNumberDto>(),
            MedicalConditions: Array.Empty<PatientMedicalConditionDto>(),
            IsDeleted: false,
            CardNumber: "CARD-1",
            RecentUltrasoundImaging: true);

        private static VisitWorkSheetDto Sheet() => new(
            PatientId: 1,
            PatientFullName: "Ahmed",
            LabId: "LAB-A",
            Sections: new[]
            {
                new WorkSheetSectionDto(1, "تحاليل", new[]
                {
                    new WorkSheetLineDto(100, 1, "Ahmed", "LAB-A", "CBC", "CBC", null, false, null, false, false, 0)
                })
            },
            Samples: new[]
            {
                new VisitWorkSheetSampleDto(100, true, false, true, false, false, false)
            },
            TotalTests: 1,
            PrintFileExternalBarcode: false,
            PrintDateTimeOnTubeBarcode: false,
            PrintLabIdInsteadOfPatientId: false);

        private static PatientAccountDto Account() => new(
            PatientId: 1,
            PatientFullName: "Ahmed",
            LabId: "LAB-A",
            AccountType: "Individual",
            TotalCharged: 0m,
            TotalPaid: 0m,
            TotalDiscount: 0m,
            Balance: 0m,
            ChargedTests: Array.Empty<ChargedTestDto>(),
            Operations: Array.Empty<PaymentOperationDto>());
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

    private static (PatientEditorViewModel Vm, StageOneSender Sender) Build()
    {
        var sender = new StageOneSender();
        var vm = new PatientEditorViewModel(
            sender,
            new NoOpNavigation(),
            new ResultErrorPresenter(),
            new FixedDialogService(),
            new StubServiceProvider());
        return (vm, sender);
    }

    [Fact]
    public async Task Catalogue_IsSplitIntoTheBaselineQuestionGroups()
    {
        var (vm, _) = Build();

        await vm.LoadCatalogAsync();

        Assert.Equal(
            new[] { 2, 3, 4, 5, 6, 7 },
            vm.MedicationConditions.Where(c => c.Category == MedicalConditionCategory.Medication)
                .Select(c => c.MedicalConditionTypeId).ToArray());
        Assert.Equal(
            new[] { 8, 9, 10, 11, 12 },
            vm.ConditionToggles.Select(c => c.MedicalConditionTypeId).ToArray());
    }

    [Fact]
    public async Task PregnancyQuestion_IsHiddenForAMalePatient()
    {
        var (vm, _) = Build();

        await vm.LoadCatalogAsync();

        Assert.DoesNotContain(vm.MedicationConditions, c => c.Category == MedicalConditionCategory.Pregnancy);
    }

    [Fact]
    public async Task PregnancyQuestion_IsOfferedToAFemalePatient()
    {
        var (vm, _) = Build();
        await vm.LoadCatalogAsync();

        vm.IsMale = false;

        var pregnancy = Assert.Single(vm.MedicationConditions, c => c.Category == MedicalConditionCategory.Pregnancy);
        Assert.Equal(1, pregnancy.MedicalConditionTypeId);
    }

    [Fact]
    public async Task PregnancySelection_IsClearedWhenTheSexBecomesMale()
    {
        var (vm, _) = Build();
        await vm.LoadCatalogAsync();
        vm.IsMale = false;
        var pregnancy = Assert.Single(vm.MedicationConditions, c => c.Category == MedicalConditionCategory.Pregnancy);
        pregnancy.IsSelected = true;

        vm.IsMale = true;

        Assert.False(pregnancy.IsSelected);
        Assert.DoesNotContain(vm.MedicationConditions, c => c.Category == MedicalConditionCategory.Pregnancy);
    }

    [Fact]
    public async Task SampleTypeSummary_IsReadOnlyAndFollowsTheVisitsTests()
    {
        var (vm, _) = Build();
        await vm.LoadCatalogAsync();
        await vm.LoadPatientAsync(1);

        Assert.Equal("بول، دم", vm.SampleTypeSummary);

        // The summary is derived: it exposes no setter.
        var property = typeof(PatientEditorViewModel).GetProperty(nameof(PatientEditorViewModel.SampleTypeSummary));
        Assert.NotNull(property);
        Assert.Null(property!.SetMethod);
    }

    [Fact]
    public async Task LoadedPatient_CarriesTheCardNumberTheUltrasoundFlagAndTheDates()
    {
        var (vm, _) = Build();
        await vm.LoadCatalogAsync();

        await vm.LoadPatientAsync(1);

        Assert.Equal("CARD-1", vm.CardNumber);
        Assert.True(vm.RecentUltrasoundImaging);
        Assert.Equal("2026-03-01 08:00", vm.RegistrationDateText);
        Assert.Equal("2026-03-01 08:00", vm.PickupDateText);
    }

    [Fact]
    public async Task Save_CarriesTheCardNumberTheDatesAndTheUltrasoundFlag()
    {
        var (vm, sender) = Build();
        await vm.LoadCatalogAsync();
        await vm.LoadPatientAsync(1);

        vm.CardNumber = "CARD-2";
        vm.RegistrationDateText = "2026-03-02 07:30";
        vm.PickupDateText = "2026-03-03 10:15";
        vm.RecentUltrasoundImaging = true;
        vm.SaveCommand.Execute(null);
        await Task.Yield();
        await Task.Delay(100);

        var update = Assert.Single(sender.UpdateCommands);
        Assert.Equal("CARD-2", update.CardNumber);
        Assert.True(update.RecentUltrasoundImaging);
        Assert.Equal(new DateTime(2026, 3, 2, 7, 30, 0), update.RegistrationDateUtc);
        Assert.Equal(new DateTime(2026, 3, 3, 10, 15, 0), update.PickupDateUtc);
    }

    [Fact]
    public async Task Save_AnInvalidRegistrationDate_IsRefused()
    {
        var (vm, sender) = Build();
        await vm.LoadCatalogAsync();
        await vm.LoadPatientAsync(1);

        vm.RegistrationDateText = "not-a-date";
        vm.SaveCommand.Execute(null);
        await Task.Yield();
        await Task.Delay(100);

        Assert.Empty(sender.UpdateCommands);
        Assert.NotEmpty(vm.ErrorMessage);
    }

    [Fact]
    public async Task Save_CardNumberLongerThan30_IsRefused()
    {
        var (vm, sender) = Build();
        await vm.LoadCatalogAsync();
        await vm.LoadPatientAsync(1);

        vm.CardNumber = new string('C', 31);
        vm.SaveCommand.Execute(null);
        await Task.Yield();
        await Task.Delay(100);

        Assert.Empty(sender.UpdateCommands);
        Assert.NotEmpty(vm.ErrorMessage);
    }

    [Fact]
    public async Task SavePrintResultPassword_SendsThePasswordAndDropsThePlainValue()
    {
        var (vm, sender) = Build();
        await vm.LoadCatalogAsync();

        vm.PrintResultPasswordEnabled = true;
        vm.PrintResultPasswordText = "s3cret!";
        vm.SavePrintResultPasswordCommand.Execute(null);
        await Task.Yield();
        await Task.Delay(100);

        var command = Assert.Single(sender.PasswordCommands);
        Assert.True(command.Enabled);
        Assert.Equal("s3cret!", command.Password);
        Assert.Null(vm.PrintResultPasswordText);
        Assert.Empty(vm.ErrorMessage);
    }

    [Fact]
    public async Task AMalePatientNeverCarriesThePregnancyAnswer()
    {
        var (vm, sender) = Build();
        sender.PatientSex = Sex.Male;
        await vm.LoadCatalogAsync();
        await vm.LoadPatientAsync(1);

        var pregnancy = vm.Conditions.Single(c => c.Category == MedicalConditionCategory.Pregnancy);
        Assert.False(pregnancy.IsSelected);
        Assert.DoesNotContain(vm.MedicationConditions, c => c.Category == MedicalConditionCategory.Pregnancy);
    }

    [Fact]
    public async Task SaveForANewPatient_CarriesTheCardNumberTheDatesAndTheDefaultedReceiptDate()
    {
        var (vm, sender) = Build();
        await vm.LoadCatalogAsync();

        vm.FullName = "Ahmed";
        vm.CardNumber = "CARD-3";
        vm.RegistrationDateText = "2026-03-05 09:00";
        vm.RecentUltrasoundImaging = true;
        vm.SaveCommand.Execute(null);
        await Task.Yield();
        await Task.Delay(100);

        var create = Assert.Single(sender.CreateCommands);
        Assert.Equal("CARD-3", create.CardNumber);
        Assert.True(create.RecentUltrasoundImaging);
        Assert.Equal(new DateTime(2026, 3, 5, 9, 0, 0), create.RegistrationDateUtc);
        // The receipt date defaults to the registration moment.
        Assert.Equal(new DateTime(2026, 3, 5, 9, 0, 0), create.PickupDateUtc);
    }

    [Fact]
    public async Task Catalogue_CarriesThePrintResultPasswordFlag()
    {
        var (vm, sender) = Build();
        sender.PrintResultPasswordEnabled = true;

        await vm.LoadCatalogAsync();

        Assert.True(vm.PrintResultPasswordEnabled);
    }
}
