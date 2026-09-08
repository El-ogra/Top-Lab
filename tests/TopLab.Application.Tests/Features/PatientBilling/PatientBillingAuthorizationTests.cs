using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Behaviors;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientBilling.Commands.RecordCorrection;
using TopLab.Application.Features.PatientBilling.Commands.RecordExtraCharge;
using TopLab.Application.Features.PatientBilling.Commands.RecordPayment;
using TopLab.Application.Features.PatientBilling.Commands.SettleAccountInFull;
using TopLab.Application.Features.PatientBilling.Commands.VoidPaymentOperation;
using TopLab.Application.Tests.Common.Fakes;
using Xunit;

namespace TopLab.Application.Tests.Features.PatientBilling;

public class PatientBillingAuthorizationTests
{
    [Theory]
    [InlineData(typeof(RecordCorrectionCommand))]
    [InlineData(typeof(VoidPaymentOperationCommand))]
    public void GatedCommands_Require_CashDisburseDeposit(System.Type commandType)
    {
        IAuthorizedRequest authorized = commandType == typeof(RecordCorrectionCommand)
            ? new RecordCorrectionCommand(1, 10m)
            : new VoidPaymentOperationCommand(1);

        Assert.Equal("CASH_DISBURSE_DEPOSIT", authorized.RequiredPermissionCode);
    }

    [Fact]
    public void RecordPayment_IsNotGated()
    {
        Assert.IsNotAssignableFrom<IAuthorizedRequest>(new RecordPaymentCommand(1, 100m));
    }

    [Fact]
    public void RecordExtraCharge_IsNotGated()
    {
        Assert.IsNotAssignableFrom<IAuthorizedRequest>(new RecordExtraChargeCommand(1, 100m));
    }

    [Fact]
    public void SettleAccountInFull_IsNotGated()
    {
        Assert.IsNotAssignableFrom<IAuthorizedRequest>(new SettleAccountInFullCommand(1));
    }

    [Fact]
    public async Task WithoutPermission_ReturnsForbidden_WithSharedMessage()
    {
        var user = new FakeCurrentUserService { IsAbsolutePermission = false, GrantedPermissions = { } };
        var behavior = new AuthorizationBehavior<RecordCorrectionCommand, Result<int>>(user);

        var response = await behavior.Handle(
            new RecordCorrectionCommand(1, 10m),
            _ => throw new Exception("handler must not run"),
            CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Equal(ErrorType.Forbidden, response.Error!.Type);
        Assert.Equal("أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام", response.Error.Message);
    }
}
