using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Behaviors;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientBilling.Common;
using TopLab.Application.Features.PatientBilling.Queries.GetPatientAccount;
using TopLab.Application.Features.PatientBilling.Queries.GetPatientInvoice;
using TopLab.Application.Features.PatientBilling.Queries.GetPatientReceipt;
using TopLab.Application.Features.PatientBilling.Queries.ListPatientPayments;
using TopLab.Application.Features.SentOutSamples.Common;
using TopLab.Application.Features.SentOutSamples.Queries.GetSentOutLabAccount;
using TopLab.Application.Features.SentOutSamples.Queries.GetSentOutSamples;
using TopLab.Application.Tests.Common.Fakes;

namespace TopLab.Application.Tests.Features.PatientBilling;

/// <summary>
/// R1: six financial reads must require the planted CASH_DISBURSE_DEPOSIT code
/// (absolute bypass via AuthorizationBehavior). Explicit FakeCurrentUserService state only.
/// </summary>
public sealed class FinancialReadAuthorizationTests
{
    private const string CashCode = "CASH_DISBURSE_DEPOSIT";

    private static FakeCurrentUserService NonAbsoluteNoCash() => new()
    {
        IsAuthenticated = true,
        IsAbsolutePermission = false,
        UserId = 10,
        UserName = "registrar"
    };

    private static FakeCurrentUserService Cashier() => new()
    {
        IsAuthenticated = true,
        IsAbsolutePermission = false,
        UserId = 11,
        UserName = "cashier"
    };

    private static FakeCurrentUserService Absolute() => new()
    {
        IsAuthenticated = true,
        IsAbsolutePermission = true,
        UserId = 99,
        UserName = "admin"
    };

    private static Task<TResponse> Run<TRequest, TResponse>(
        TRequest request,
        FakeCurrentUserService user,
        Func<Task<TResponse>> handler)
        where TRequest : notnull
        => new AuthorizationBehavior<TRequest, TResponse>(user)
            .Handle(request, _ => handler(), CancellationToken.None);

    public static IEnumerable<object[]> GatedQueries()
    {
        yield return new object[] { new GetPatientAccountQuery(1) };
        yield return new object[] { new ListPatientPaymentsQuery(1, 1, 50) };
        yield return new object[] { new GetPatientInvoiceQuery(1) };
        yield return new object[] { new GetPatientReceiptQuery(1) };
        yield return new object[] { new GetSentOutLabAccountQuery(1, null, null) };
        yield return new object[] { new GetSentOutSamplesQuery(null, null, null, 1, 50) };
    }

    [Theory]
    [MemberData(nameof(GatedQueries))]
    public void AllSix_Declare_PlantedCashCode(object request)
    {
        var authorized = Assert.IsAssignableFrom<IAuthorizedRequest>(request);
        Assert.Equal(CashCode, authorized.RequiredPermissionCode);
    }

    [Fact]
    public async Task NonAbsoluteWithoutCash_IsForbidden_GetPatientAccount()
    {
        var called = false;
        var result = await Run(new GetPatientAccountQuery(1), NonAbsoluteNoCash(), () =>
        {
            called = true;
            return Task.FromResult(Result<PatientAccountDto>.Success(null!));
        });
        Assert.False(called);
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Forbidden, result.Error!.Type);
    }

    [Fact]
    public async Task NonAbsoluteWithoutCash_IsForbidden_ListPatientPayments()
    {
        var called = false;
        var result = await Run(new ListPatientPaymentsQuery(1, 1, 50), NonAbsoluteNoCash(), () =>
        {
            called = true;
            return Task.FromResult(Result<IReadOnlyList<PaymentOperationDto>>.Success([]));
        });
        Assert.False(called);
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Forbidden, result.Error!.Type);
    }

    [Fact]
    public async Task NonAbsoluteWithoutCash_IsForbidden_GetPatientInvoice()
    {
        var called = false;
        var result = await Run(new GetPatientInvoiceQuery(1), NonAbsoluteNoCash(), () =>
        {
            called = true;
            return Task.FromResult(Result<InvoiceDto>.Success(null!));
        });
        Assert.False(called);
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Forbidden, result.Error!.Type);
    }

    [Fact]
    public async Task NonAbsoluteWithoutCash_IsForbidden_GetPatientReceipt()
    {
        var called = false;
        var result = await Run(new GetPatientReceiptQuery(1), NonAbsoluteNoCash(), () =>
        {
            called = true;
            return Task.FromResult(Result<ReceiptDto>.Success(null!));
        });
        Assert.False(called);
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Forbidden, result.Error!.Type);
    }

    [Fact]
    public async Task NonAbsoluteWithoutCash_IsForbidden_GetSentOutLabAccount()
    {
        var called = false;
        var result = await Run(new GetSentOutLabAccountQuery(1, null, null), NonAbsoluteNoCash(), () =>
        {
            called = true;
            return Task.FromResult(Result<SentOutLabAccountDto>.Success(null!));
        });
        Assert.False(called);
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Forbidden, result.Error!.Type);
    }

    [Fact]
    public async Task NonAbsoluteWithoutCash_IsForbidden_GetSentOutSamples()
    {
        var called = false;
        var result = await Run(new GetSentOutSamplesQuery(null, null, null, 1, 50), NonAbsoluteNoCash(), () =>
        {
            called = true;
            return Task.FromResult(Result<IReadOnlyList<SentOutSampleDto>>.Success([]));
        });
        Assert.False(called);
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Forbidden, result.Error!.Type);
    }

    [Fact]
    public async Task CashierWithCode_IsAllowed_AllSix()
    {
        var user = Cashier();
        user.GrantedPermissions.Add(CashCode);

        Assert.True((await Run(new GetPatientAccountQuery(1), user, () =>
            Task.FromResult(Result<PatientAccountDto>.Success(null!)))).IsSuccess);
        Assert.True((await Run(new ListPatientPaymentsQuery(1, 1, 50), user, () =>
            Task.FromResult(Result<IReadOnlyList<PaymentOperationDto>>.Success([])))).IsSuccess);
        Assert.True((await Run(new GetPatientInvoiceQuery(1), user, () =>
            Task.FromResult(Result<InvoiceDto>.Success(null!)))).IsSuccess);
        Assert.True((await Run(new GetPatientReceiptQuery(1), user, () =>
            Task.FromResult(Result<ReceiptDto>.Success(null!)))).IsSuccess);
        Assert.True((await Run(new GetSentOutLabAccountQuery(1, null, null), user, () =>
            Task.FromResult(Result<SentOutLabAccountDto>.Success(null!)))).IsSuccess);
        Assert.True((await Run(new GetSentOutSamplesQuery(null, null, null, 1, 50), user, () =>
            Task.FromResult(Result<IReadOnlyList<SentOutSampleDto>>.Success([])))).IsSuccess);
    }

    [Fact]
    public async Task AbsoluteWithoutCode_IsAllowed_GetPatientAccount()
    {
        var called = false;
        var result = await Run(new GetPatientAccountQuery(1), Absolute(), () =>
        {
            called = true;
            return Task.FromResult(Result<PatientAccountDto>.Success(null!));
        });
        Assert.True(called);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task TotalsScreen_Forbidden_UsesPresenterContract()
    {
        // PatientEditor.RefreshBillingAsync: if (!account.IsSuccess) ErrorMessage = _presenter.Present(account.Error!); return;
        // Forbidden maps to ResultErrorPresenter.PermissionDeniedMessage — asserted here as the ErrorType contract.
        var result = await Run(new GetPatientAccountQuery(1), NonAbsoluteNoCash(), () =>
            Task.FromResult(Result<PatientAccountDto>.Success(null!)));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Forbidden, result.Error!.Type);
    }
}
