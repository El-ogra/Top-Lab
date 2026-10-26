using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientBilling.Commands.RecordPayment;
using TopLab.Application.Features.PatientBilling.Common;
using TopLab.Application.Features.PatientBilling.Queries.GetPatientAccount;
using TopLab.Application.Features.PatientBilling.Queries.ListPatientPayments;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Common.Navigation;
using TopLab.Presentation.Tests.Common;
using TopLab.Presentation.ViewModels.Patients;
using Xunit;

namespace TopLab.Presentation.Tests.Patients;

/// <summary>
/// R1 Round 5 — D-01 on the PATIENT ACCOUNT screen (<see cref="PatientAccountViewModel"/>).
/// The same eight required cases as the editor tests, driven through
/// <c>AccountEntryEnterCommand</c> exactly as the Enter key drives it, so both payment-entry
/// screens are proved to behave identically.
/// </summary>
public class PatientAccountPercentageTests
{
    private sealed class AccountSender : ISender
    {
        public decimal TotalCharged { get; set; } = 1000m;
        public decimal TotalPaid { get; set; }
        public int PaymentCalls { get; private set; }
        public List<RecordPaymentCommand> Payments { get; } = new();

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            switch (request)
            {
                case GetPatientAccountQuery:
                    return Task.FromResult((TResponse)(object)Result<PatientAccountDto>.Success(new PatientAccountDto(
                        PatientId: 7,
                        PatientFullName: "أحمد محمد علي",
                        LabId: "LAB-1",
                        AccountType: "Individual",
                        TotalCharged: TotalCharged,
                        TotalPaid: TotalPaid,
                        TotalDiscount: 0m,
                        Balance: TotalCharged - TotalPaid,
                        ChargedTests: Array.Empty<ChargedTestDto>(),
                        Operations: Array.Empty<PaymentOperationDto>())));
                case ListPatientPaymentsQuery:
                    return Task.FromResult((TResponse)(object)Result<IReadOnlyList<PaymentOperationDto>>.Success(
                        Array.Empty<PaymentOperationDto>()));
                case RecordPaymentCommand payment:
                    PaymentCalls++;
                    Payments.Add(payment);
                    return Task.FromResult((TResponse)(object)Result<int>.Success(1));
                default:
                    throw new NotSupportedException($"AccountSender has no canned response for {request.GetType().Name}.");
            }
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest => throw new NotSupportedException();
        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
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

    private static async Task<(PatientAccountViewModel Vm, AccountSender Sender)> LoadedAsync(
        decimal totalCharged, decimal totalPaid)
    {
        var sender = new AccountSender { TotalCharged = totalCharged, TotalPaid = totalPaid };
        var vm = new PatientAccountViewModel(
            sender, new SilentNavigation(), new ResultErrorPresenter(),
            new FakeDialogService(), new StubServices());
        await vm.LoadAsync(7);
        return (vm, sender);
    }

    private static async Task PressEnterAsync(PatientAccountViewModel vm)
    {
        vm.AccountEntryEnterCommand.Execute(null);
        await Task.Yield();
        await Task.Delay(80);
    }

    // Case 1: TC 1000, nothing paid, amount 900, 10% -> accepted; first previews, second persists.
    [Fact]
    public async Task D01_Case1_FullSettlementByPercentage_IsAcceptedThenPersists()
    {
        var (vm, sender) = await LoadedAsync(1000m, 0m);
        vm.PaymentAmountText = "900";
        vm.PaymentPercentageText = "10";

        await PressEnterAsync(vm);

        Assert.Equal("100", vm.PaymentDiscountText);
        Assert.Equal(string.Empty, vm.ErrorMessage);
        Assert.False(string.IsNullOrWhiteSpace(vm.StatusMessage));
        Assert.Equal(0, sender.PaymentCalls);

        await PressEnterAsync(vm);

        Assert.Equal(1, sender.PaymentCalls);
        Assert.Equal(900m, sender.Payments[0].Amount);
        Assert.Equal(100m, sender.Payments[0].DiscountAmount);
    }

    // Case 2: TC 1000, nothing paid, amount 100, 50% -> refused as unavailable.
    [Fact]
    public async Task D01_Case2_PartialPaymentWithPercentage_IsRefused()
    {
        var (vm, sender) = await LoadedAsync(1000m, 0m);
        vm.PaymentAmountText = "100";
        vm.PaymentPercentageText = "50";

        await PressEnterAsync(vm);

        Assert.Equal(
            "النسبة متاحة فقط عند سداد كامل المتبقي: يجب أن يكون المدفوع + الخصم = 1000.",
            vm.ErrorMessage);
        Assert.Equal(0, sender.PaymentCalls);
    }

    // Case 3: TC 1000, nothing paid, amount 1000, 10% -> would credit 100, refused.
    [Fact]
    public async Task D01_Case3_OverSettlementWithPercentage_IsRefused()
    {
        var (vm, sender) = await LoadedAsync(1000m, 0m);
        vm.PaymentAmountText = "1000";
        vm.PaymentPercentageText = "10";

        await PressEnterAsync(vm);

        Assert.Equal(
            "النسبة متاحة فقط عند سداد كامل المتبقي: يجب أن يكون المدفوع + الخصم = 1000.",
            vm.ErrorMessage);
        Assert.Equal(0, sender.PaymentCalls);
    }

    // Case 4: TC 1000, already paid 400 (remaining 600), amount 500, 10% -> accepted.
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

    // Case 5: TC 1000, nothing paid, amount 400, 60% -> availability passes, Domain rule refuses.
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

    // Case 6: nothing remaining -> its own message.
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

    // Case 7: value discount unchanged.
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

    // Case 8: the Round-4 fix is preserved — the percentage is never silently cleared.
    [Fact]
    public async Task D01_Case8_PercentageIsNeverSilentlyCleared()
    {
        var (vm, _) = await LoadedAsync(1000m, 0m);
        vm.PaymentAmountText = "900";
        vm.PaymentPercentageText = "10";

        await PressEnterAsync(vm);

        Assert.Equal("100", vm.PaymentDiscountText);
        Assert.Null(vm.PaymentPercentageText);
        Assert.NotEqual("", vm.PaymentDiscountText);
    }
}
