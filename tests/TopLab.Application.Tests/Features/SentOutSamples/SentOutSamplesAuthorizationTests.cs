using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Behaviors;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.SentOutSamples.Commands.RecordSentOutPayment;
using TopLab.Application.Features.SentOutSamples.Commands.SendSampleOut;
using TopLab.Application.Features.SentOutSamples.Commands.SettleSentOutInFull;
using TopLab.Application.Tests.Common.Fakes;
using Xunit;

namespace TopLab.Application.Tests.Features.SentOutSamples;

public class SentOutSamplesAuthorizationTests
{
    private static object CreateInstance(System.Type commandType)
    {
        return commandType switch
        {
            { } t when t == typeof(SendSampleOutCommand) => new SendSampleOutCommand(1, 1, null, null),
            { } t when t == typeof(RecordSentOutPaymentCommand) => new RecordSentOutPaymentCommand(1, 10m),
            { } t when t == typeof(SettleSentOutInFullCommand) => new SettleSentOutInFullCommand(1),
            _ => throw new InvalidOperationException($"Unhandled command type {commandType.Name} in test.")
        };
    }

    [Theory]
    [InlineData(typeof(SendSampleOutCommand))]
    [InlineData(typeof(RecordSentOutPaymentCommand))]
    [InlineData(typeof(SettleSentOutInFullCommand))]
    public void EveryWriteCommand_Requires_CashDisburseDeposit(System.Type commandType)
    {
        var authorized = (IAuthorizedRequest)CreateInstance(commandType);

        Assert.Equal("CASH_DISBURSE_DEPOSIT", authorized.RequiredPermissionCode);
    }

    [Fact]
    public async Task WithoutPermission_ReturnsForbidden_WithSharedMessage()
    {
        var user = new FakeCurrentUserService { IsAbsolutePermission = false };
        var behavior = new AuthorizationBehavior<RecordSentOutPaymentCommand, Result>(user);

        var response = await behavior.Handle(
            new RecordSentOutPaymentCommand(1, 10m),
            _ => throw new Exception("handler must not run"),
            CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Equal(ErrorType.Forbidden, response.Error!.Type);
        Assert.Equal("أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام", response.Error.Message);
    }

    [Fact]
    public async Task WithoutPermission_ReturnsForbidden_ForGenericResult()
    {
        var user = new FakeCurrentUserService { IsAbsolutePermission = false };
        var behavior = new AuthorizationBehavior<SendSampleOutCommand, Result<int>>(user);

        var response = await behavior.Handle(
            new SendSampleOutCommand(1, 1, null, null),
            _ => throw new Exception("handler must not run"),
            CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Equal(ErrorType.Forbidden, response.Error!.Type);
    }

    [Fact]
    public async Task AbsoluteUser_BypassesPermissionCheck()
    {
        var user = new FakeCurrentUserService { IsAbsolutePermission = true };
        var behavior = new AuthorizationBehavior<SettleSentOutInFullCommand, Result>(user);

        var response = await behavior.Handle(
            new SettleSentOutInFullCommand(1),
            _ => Task.FromResult(Result.Success()),
            CancellationToken.None);

        Assert.True(response.IsSuccess);
    }
}
