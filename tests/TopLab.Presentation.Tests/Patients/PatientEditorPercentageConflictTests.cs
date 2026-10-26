using System.IO;
using MediatR;
using TopLab.Application.Common.Results;
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
using TopLab.Presentation.Tests.Common;
using TopLab.Presentation.ViewModels.Patients;
using Xunit;

namespace TopLab.Presentation.Tests.Patients;

/// <summary>
/// R1 Round 5 — D-01 on the PATIENT EDITOR (<see cref="PatientEditorViewModel"/>).
/// <para>
/// Round 4 kept this file as a characterization probe of the pre-decision behaviour: on a partial
/// payment the percentage was computed from the total and then refused by the Domain over-discount
/// rule. The owner then chose option 4 of card D-01, so the expected messages below changed:
/// </para>
/// <list type="bullet">
/// <item><c>V1_PartialPayment_WithPercentage_ShowsTheDiscountExceedsMessage</c> is renamed and now
/// expects the D-01 availability message instead of the Domain over-discount message, because the
/// availability check runs first. The assertion was not weakened: it still requires the exact
/// message, the exact converted discount, no status line and no mediator call.</item>
/// <item><c>V2_PartialPayment_HundredPercent_IsRejected</c> likewise now expects the availability
/// message.</item>
/// <item>The two full-payment cases (5% and 100%) now expect to be REFUSED, because with nothing
/// previously paid a full payment plus any discount over-settles the account.</item>
/// </list>
/// <para>
/// Scenario V1 (unchanged input): TotalCharged = 1000, TotalPaid = 0, paid amount "100",
/// percentage "50", discount value empty. <c>AccountEntryEnterCommand</c> is driven exactly as
/// the Enter key drives it.
/// </para>
/// </summary>
public class PatientEditorPercentageConflictTests
{
    private const int PatientIdValue = 7;

    private sealed class ProbeSender : ISender
    {
        public decimal TotalCharged { get; set; } = 1000m;
        public decimal TotalPaid { get; set; }
        public int PaymentCalls { get; private set; }
        public int ExtraChargeCalls { get; private set; }
        public List<TopLab.Application.Features.PatientBilling.Commands.RecordPayment.RecordPaymentCommand> Payments { get; } = new();

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            switch (request)
            {
                case GetPatientByIdQuery:
                    return Task.FromResult((TResponse)(object)Result<PatientDetailDto>.Success(Detail()));
                case GetPatientVisitHistoryQuery:
                    return Task.FromResult((TResponse)(object)Result<IReadOnlyList<VisitHistoryDto>>.Success(
                        Array.Empty<VisitHistoryDto>()));
                case GetVisitWorkSheetQuery:
                    return Task.FromResult((TResponse)(object)Result<VisitWorkSheetDto>.Success(Sheet()));
                case GetPatientAccountQuery:
                    return Task.FromResult((TResponse)(object)Result<PatientAccountDto>.Success(Account()));
                case TopLab.Application.Features.PatientBilling.Commands.RecordPayment.RecordPaymentCommand payment:
                    PaymentCalls++;
                    Payments.Add(payment);
                    return Task.FromResult((TResponse)(object)Result<int>.Success(1));
                case TopLab.Application.Features.PatientBilling.Commands.RecordExtraCharge.RecordExtraChargeCommand:
                    ExtraChargeCalls++;
                    return Task.FromResult((TResponse)(object)Result<int>.Success(1));
                default:
                    throw new NotSupportedException($"ProbeSender has no canned response for {request.GetType().Name}.");
            }
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest => throw new NotSupportedException();
        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        private static PatientDetailDto Detail() => new(
            PatientId: PatientIdValue, LabId: "LAB-A", Title: null, FullName: "Patient 7",
            Sex: Sex.Male, AgeValue: 30, AgeUnit: AgeUnit.Year, NationalId: null, Address: null,
            AccountType: AccountType.Individual, IsVip: false,
            RegistrationDateUtc: new DateTime(2026, 10, 25, 9, 0, 0, DateTimeKind.Utc),
            PickupDateUtc: null, IsFastingIndicated: false, FastingHours: null,
            RecentContrastImaging: false, Notes: null, TreatingDoctorId: null, TreatingDoctorName: null,
            ReferralEntityId: null, ReferralEntityName: null,
            PhoneNumbers: Array.Empty<PatientPhoneNumberDto>(),
            MedicalConditions: Array.Empty<PatientMedicalConditionDto>(), IsDeleted: false);

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

    private sealed class SilentNavigation : INavigationService
    {
        public ViewModelBase? CurrentViewModel => null;
        public event Action<ViewModelBase?>? Navigated
        {
            add { }
            remove { }
        }
        public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase { }
        public void NavigateTo(ViewModelBase viewModel) { }
    }

    private sealed class StubServices : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private static async Task<(PatientEditorViewModel Vm, ProbeSender Sender)> LoadedAsync(
        decimal totalCharged, decimal totalPaid)
    {
        var sender = new ProbeSender { TotalCharged = totalCharged, TotalPaid = totalPaid };
        var vm = new PatientEditorViewModel(
            sender, new SilentNavigation(), new ResultErrorPresenter(),
            new FakeDialogService(), new StubServices());
        await vm.LoadPatientAsync(PatientIdValue);
        return (vm, sender);
    }

    private static async Task PressEnterAsync(PatientEditorViewModel vm)
    {
        vm.AccountEntryEnterCommand.Execute(null);
        await Task.Yield();
        await Task.Delay(80);
    }

    /// <summary>V1 (D-01) — the common partial-payment case is refused as unavailable.</summary>
    [Fact]
    public async Task V1_PartialPayment_WithPercentage_ShowsTheAvailabilityMessage()
    {
        var (vm, sender) = await LoadedAsync(totalCharged: 1000m, totalPaid: 0m);
        vm.PaymentAmountText = "100";
        vm.PaymentPercentageText = "50";

        await PressEnterAsync(vm);

        // (a) computed discount value written into the value field
        Assert.Equal("500", vm.PaymentDiscountText);
        // (c) what the user sees — D-01 availability message (it runs before the Domain rule)
        Assert.Equal(
            "النسبة متاحة فقط عند سداد كامل المتبقي: يجب أن يكون المدفوع + الخصم = 1000.",
            vm.ErrorMessage);
        // nothing is recorded, and no status line invites a second press
        Assert.Equal(0, sender.PaymentCalls);
        Assert.Equal(string.Empty, vm.StatusMessage);
    }

    /// <summary>V2 case 1 (D-01) — full payment of 1000 with 5% over-settles by 50, so refused.</summary>
    [Fact]
    public async Task V2_FullPayment_FivePercent_IsRefusedAsOverSettlement()
    {
        var (vm, sender) = await LoadedAsync(1000m, 0m);
        vm.PaymentAmountText = "1000";
        vm.PaymentPercentageText = "5";

        await PressEnterAsync(vm);

        Assert.Equal("50", vm.PaymentDiscountText);
        Assert.Equal(
            "النسبة متاحة فقط عند سداد كامل المتبقي: يجب أن يكون المدفوع + الخصم = 1000.",
            vm.ErrorMessage);
        Assert.Equal(0, sender.PaymentCalls);
    }

    /// <summary>V2 case 2 (D-01) — full payment of 1000 with 100% would credit 1000, so refused.</summary>
    [Fact]
    public async Task V2_FullPayment_HundredPercent_IsRefusedAsOverSettlement()
    {
        var (vm, sender) = await LoadedAsync(1000m, 0m);
        vm.PaymentAmountText = "1000";
        vm.PaymentPercentageText = "100";

        await PressEnterAsync(vm);

        Assert.Equal("1000", vm.PaymentDiscountText);
        Assert.Equal(
            "النسبة متاحة فقط عند سداد كامل المتبقي: يجب أن يكون المدفوع + الخصم = 1000.",
            vm.ErrorMessage);
        Assert.Equal(0, sender.PaymentCalls);
    }

    /// <summary>V2 case 3 (D-01) — total 100, payment 50, 100%: refused as unavailable.</summary>
    [Fact]
    public async Task V2_PartialPayment_HundredPercent_IsRejected()
    {
        var (vm, sender) = await LoadedAsync(100m, 0m);
        vm.PaymentAmountText = "50";
        vm.PaymentPercentageText = "100";

        await PressEnterAsync(vm);

        Assert.Equal("100", vm.PaymentDiscountText);
        Assert.Equal(
            "النسبة متاحة فقط عند سداد كامل المتبقي: يجب أن يكون المدفوع + الخصم = 100.",
            vm.ErrorMessage);
        Assert.Equal(0, sender.PaymentCalls);
    }

    // ================================================================
    // D-01 — the eight required cases, on the editor screen
    // ================================================================

    /// <summary>Case 1 — TC 1000, nothing paid, amount 900, 10%: accepted, then persists on the 2nd Enter.</summary>
    [Fact]
    public async Task D01_Case1_FullSettlementByPercentage_IsAcceptedThenPersists()
    {
        var (vm, sender) = await LoadedAsync(1000m, 0m);
        vm.PaymentAmountText = "900";
        vm.PaymentPercentageText = "10";

        await PressEnterAsync(vm);

        Assert.Equal("100", vm.PaymentDiscountText);
        Assert.Equal(string.Empty, vm.ErrorMessage);
        Assert.Equal(0, sender.PaymentCalls);

        await PressEnterAsync(vm);

        Assert.Equal(1, sender.PaymentCalls);
        Assert.Equal(900m, sender.Payments[0].Amount);
        Assert.Equal(100m, sender.Payments[0].DiscountAmount);
    }

    /// <summary>Case 4 — TC 1000, already paid 400 (remaining 600), amount 500, 10%: accepted.</summary>
    [Fact]
    public async Task D01_Case4_RemainingAfterAPartPayment_IsAccepted()
    {
        var (vm, sender) = await LoadedAsync(1000m, 400m);
        vm.PaymentAmountText = "500";
        vm.PaymentPercentageText = "10";

        await PressEnterAsync(vm);

        Assert.Equal(string.Empty, vm.ErrorMessage);
        Assert.Equal("100", vm.PaymentDiscountText);
        Assert.Equal(0, sender.PaymentCalls);
    }

    /// <summary>Case 5 — TC 1000, nothing paid, amount 400, 60%: availability passes, Domain rule refuses.</summary>
    [Fact]
    public async Task D01_Case5_AvailabilityPassesThenTheDomainRuleRefuses()
    {
        var (vm, sender) = await LoadedAsync(1000m, 0m);
        vm.PaymentAmountText = "400";
        vm.PaymentPercentageText = "60";

        await PressEnterAsync(vm);

        Assert.Equal("الخصم لا يمكن أن يتجاوز مبلغ الدفع.", vm.ErrorMessage);
        Assert.Equal(0, sender.PaymentCalls);
    }

    /// <summary>Case 6 — nothing remaining: its own message.</summary>
    [Fact]
    public async Task D01_Case6_NoRemainingBalance_IsRefused()
    {
        var (vm, sender) = await LoadedAsync(1000m, 1000m);
        vm.PaymentAmountText = "100";
        vm.PaymentPercentageText = "10";

        await PressEnterAsync(vm);

        Assert.Equal("لا يوجد مبلغ متبقٍ في الحساب لتطبيق نسبة عليه.", vm.ErrorMessage);
        Assert.Equal(0, sender.PaymentCalls);
    }

    /// <summary>Case 7 — a value discount is unchanged by D-01.</summary>
    [Fact]
    public async Task D01_Case7_ValueDiscount_IsUnchanged()
    {
        var (vm, sender) = await LoadedAsync(1000m, 0m);
        vm.PaymentAmountText = "100";
        vm.PaymentDiscountText = "20";

        await PressEnterAsync(vm);

        Assert.Equal(string.Empty, vm.ErrorMessage);
        Assert.Equal(0, sender.PaymentCalls);

        await PressEnterAsync(vm);

        Assert.Equal(1, sender.PaymentCalls);
        Assert.Equal(20m, sender.Payments[0].DiscountAmount);
    }

    /// <summary>Case 8 — the Round-4 fix is preserved: the percentage is never silently cleared.</summary>
    [Fact]
    public async Task D01_Case8_PercentageIsNeverSilentlyCleared()
    {
        var (vm, _) = await LoadedAsync(1000m, 0m);
        vm.PaymentAmountText = "900";
        vm.PaymentPercentageText = "10";

        await PressEnterAsync(vm);

        Assert.Equal("100", vm.PaymentDiscountText);
        Assert.Null(vm.PaymentPercentageText);
    }
}
