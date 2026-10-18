using System.Collections;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.InventoryAndAccounting.Common;
using TopLab.Application.Features.InventoryAndAccounting.Queries.GetCashDrawerInventory;
using TopLab.Application.Features.InventoryAndAccounting.Queries.GetCompanyDelegateAccounts;
using TopLab.Application.Features.InventoryAndAccounting.Queries.GetElementInventory;
using TopLab.Application.Features.InventoryAndAccounting.Queries.GetPatientSamplesDetail;
using TopLab.Application.Features.InventoryAndAccounting.Queries.ListCashMovements;
using TopLab.Domain.Common.Enums;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Tests.Common;
using TopLab.Presentation.ViewModels.Accounts;
using Xunit;

namespace TopLab.Presentation.Tests.Accounts;

/// <summary>
/// Phase 5 — receivable filter selector on the Accounts hub patient-samples tab.
/// Tests that the filter enum, default value, and query-parameter mapping work.
/// </summary>
public class AccountsHubReceivableFilterTests
{
    /// <summary>Captures the last GetPatientSamplesDetailQuery sent through the VM.</summary>
    private sealed class RecordingSender : ISender
    {
        public List<GetPatientSamplesDetailQuery> SampleQueries { get; } = new();
        public List<GetCashDrawerInventoryQuery> DrawerQueries { get; } = new();
        public List<GetElementInventoryQuery> ElementQueries { get; } = new();
        public List<GetCompanyDelegateAccountsQuery> CompanyQueries { get; } = new();
        public List<ListCashMovementsQuery> MovementQueries { get; } = new();

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            switch (request)
            {
                case GetCashDrawerInventoryQuery q:
                    DrawerQueries.Add(q);
                    return Task.FromResult((TResponse)(object)Result<CashDrawerInventoryDto>.Success(null!));
                case ListCashMovementsQuery q:
                    MovementQueries.Add(q);
                    return Task.FromResult((TResponse)(object)Result<IReadOnlyList<CashMovementDto>>.Success(Array.Empty<CashMovementDto>()));
                case GetElementInventoryQuery q:
                    ElementQueries.Add(q);
                    return Task.FromResult((TResponse)(object)Result<ElementInventoryDto>.Success(null!));
                case GetPatientSamplesDetailQuery q:
                    SampleQueries.Add(q);
                    return Task.FromResult((TResponse)(object)Result<IReadOnlyList<PatientSampleDetailDto>>.Success(Array.Empty<PatientSampleDetailDto>()));
                case GetCompanyDelegateAccountsQuery q:
                    CompanyQueries.Add(q);
                    return Task.FromResult((TResponse)(object)Result<IReadOnlyList<CompanyDelegateAccountDto>>.Success(Array.Empty<CompanyDelegateAccountDto>()));
                default:
                    throw new NotSupportedException($"RecordingSender has no canned response for {request.GetType().Name}.");
            }
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

    private sealed class EmptyServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => throw new NotSupportedException();
    }

    private static AccountsHubViewModel CreateVm(RecordingSender sender)
    {
        return new AccountsHubViewModel(
            sender,
            new ResultErrorPresenter(),
            new FakeDialogService(),
            new EmptyServiceProvider(),
            new FakeNavigationService());
    }

    // =====================================================================
    // Default filter
    // =====================================================================

    [Fact]
    public void ReceivableFilter_DefaultsToAll()
    {
        var vm = CreateVm(new RecordingSender());
        Assert.Equal(ReceivableFilter.All, vm.ReceivableFilter);
    }

    [Fact]
    public void ReceivableFilterOptions_HasFourChoices()
    {
        var vm = CreateVm(new RecordingSender());
        Assert.Equal(4, vm.ReceivableFilterOptions.Count);
        Assert.Contains(vm.ReceivableFilterOptions, o => o.Value == ReceivableFilter.All);
        Assert.Contains(vm.ReceivableFilterOptions, o => o.Value == ReceivableFilter.UnpaidOnly);
        Assert.Contains(vm.ReceivableFilterOptions, o => o.Value == ReceivableFilter.DiscountedOnly);
        Assert.Contains(vm.ReceivableFilterOptions, o => o.Value == ReceivableFilter.PaidOnly);
    }

    // =====================================================================
    // Filter → query parameter mapping
    // =====================================================================

    [Fact]
    public async Task AllFilter_SendsNoFilterFlags()
    {
        var sender = new RecordingSender();
        var vm = CreateVm(sender);
        vm.ReceivableFilter = ReceivableFilter.All;
        await vm.LoadAsync(CancellationToken.None);

        var q = Assert.Single(sender.SampleQueries);
        Assert.False(q.UnpaidOnly);
        Assert.False(q.DiscountedOnly);
        Assert.False(q.PaidOnly);
    }

    [Fact]
    public async Task UnpaidOnlyFilter_SetsOnlyUnpaidFlag()
    {
        var sender = new RecordingSender();
        var vm = CreateVm(sender);
        vm.ReceivableFilter = ReceivableFilter.UnpaidOnly;
        await vm.LoadAsync(CancellationToken.None);

        var q = Assert.Single(sender.SampleQueries);
        Assert.True(q.UnpaidOnly);
        Assert.False(q.DiscountedOnly);
        Assert.False(q.PaidOnly);
    }

    [Fact]
    public async Task DiscountedOnlyFilter_SetsOnlyDiscountedFlag()
    {
        var sender = new RecordingSender();
        var vm = CreateVm(sender);
        vm.ReceivableFilter = ReceivableFilter.DiscountedOnly;
        await vm.LoadAsync(CancellationToken.None);

        var q = Assert.Single(sender.SampleQueries);
        Assert.False(q.UnpaidOnly);
        Assert.True(q.DiscountedOnly);
        Assert.False(q.PaidOnly);
    }

    [Fact]
    public async Task PaidOnlyFilter_SetsOnlyPaidFlag()
    {
        var sender = new RecordingSender();
        var vm = CreateVm(sender);
        vm.ReceivableFilter = ReceivableFilter.PaidOnly;
        await vm.LoadAsync(CancellationToken.None);

        var q = Assert.Single(sender.SampleQueries);
        Assert.False(q.UnpaidOnly);
        Assert.False(q.DiscountedOnly);
        Assert.True(q.PaidOnly);
    }

    // =====================================================================
    // Existing view-model behaviour remains compatible
    // =====================================================================

    [Fact]
    public async Task LoadAsync_PopulatesAllTabs()
    {
        var sender = new RecordingSender();
        var vm = CreateVm(sender);
        await vm.LoadAsync(CancellationToken.None);

        Assert.Single(sender.DrawerQueries);
        Assert.Single(sender.MovementQueries);
        Assert.Single(sender.ElementQueries);
        Assert.Single(sender.SampleQueries);
        Assert.Single(sender.CompanyQueries);
    }

    [Fact]
    public void ReceivableFilter_CanBeSetBackToAll()
    {
        var vm = CreateVm(new RecordingSender());
        vm.ReceivableFilter = ReceivableFilter.UnpaidOnly;
        Assert.Equal(ReceivableFilter.UnpaidOnly, vm.ReceivableFilter);
        vm.ReceivableFilter = ReceivableFilter.All;
        Assert.Equal(ReceivableFilter.All, vm.ReceivableFilter);
    }
}
