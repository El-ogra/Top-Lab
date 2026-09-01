using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Behaviors;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.SystemAndPrintSettings.Commands.SavePrinterAssignments;
using TopLab.Application.Features.SystemAndPrintSettings.Commands.UpdateEnvelopeSettings;
using TopLab.Application.Features.SystemAndPrintSettings.Commands.UpdateReceiptSettings;
using TopLab.Application.Features.SystemAndPrintSettings.Commands.UpdateReportSettings;
using TopLab.Application.Features.SystemAndPrintSettings.Commands.UpdateSystemSettings;
using TopLab.Application.Tests.Common.Fakes;
using Xunit;

namespace TopLab.Application.Tests.Features.SystemAndPrintSettings;

public class SystemAndPrintSettingsAuthorizationTests
{
    [Theory]
    [InlineData(typeof(UpdateSystemSettingsCommand))]
    [InlineData(typeof(UpdateReportSettingsCommand))]
    [InlineData(typeof(UpdateReceiptSettingsCommand))]
    [InlineData(typeof(UpdateEnvelopeSettingsCommand))]
    [InlineData(typeof(SavePrinterAssignmentsCommand))]
    public void EveryWriteCommand_Requires_EditSystemSettings(System.Type commandType)
    {
        object instance;
        if (commandType == typeof(UpdateSystemSettingsCommand))
        {
            instance = new UpdateSystemSettingsCommand(
                TopLab.Domain.Common.Enums.AccountType.Individual,
                false, false, false, false, false, false, false, false,
                TopLab.Domain.Common.Enums.ResultScreenAccountDisplayMode.Hidden,
                false, null);
        }
        else if (commandType == typeof(UpdateReportSettingsCommand))
        {
            instance = new UpdateReportSettingsCommand(
                1.0m, 1.0m, 2.0m,
                TopLab.Domain.Common.Enums.PaperSize.A4,
                TopLab.Domain.Common.Enums.HeaderFooterMode.None,
                false,
                TopLab.Domain.Common.Enums.HistorySortMode.ByLabCode,
                true);
        }
        else if (commandType == typeof(UpdateReceiptSettingsCommand))
        {
            instance = new UpdateReceiptSettingsCommand(
                1.0m, "L.E.", null, false,
                TopLab.Domain.Common.Enums.TestDetailDisplayMode.Show,
                false,
                TopLab.Domain.Common.Enums.HeaderFooterMode.None);
        }
        else if (commandType == typeof(UpdateEnvelopeSettingsCommand))
        {
            instance = new UpdateEnvelopeSettingsCommand(
                3.0m,
                TopLab.Domain.Common.Enums.HeaderFooterMode.None,
                false,
                [
                    new TopLab.Application.Features.SystemAndPrintSettings.Common.EnvelopePrintItemPositionDto("Name", true, 1.0m, 1.0m),
                    new TopLab.Application.Features.SystemAndPrintSettings.Common.EnvelopePrintItemPositionDto("Code", true, 1.0m, 2.0m),
                    new TopLab.Application.Features.SystemAndPrintSettings.Common.EnvelopePrintItemPositionDto("ReferralEntity", true, 1.0m, 3.0m),
                    new TopLab.Application.Features.SystemAndPrintSettings.Common.EnvelopePrintItemPositionDto("Date", true, 1.0m, 4.0m)
                ]);
        }
        else if (commandType == typeof(SavePrinterAssignmentsCommand))
        {
            instance = new SavePrinterAssignmentsCommand(
            [
                new TopLab.Application.Features.SystemAndPrintSettings.Common.PrinterAssignmentDto(
                    TopLab.Domain.Common.Enums.PrinterOutputType.Reports, "Reports"),
                new TopLab.Application.Features.SystemAndPrintSettings.Common.PrinterAssignmentDto(
                    TopLab.Domain.Common.Enums.PrinterOutputType.Barcode, "Barcode"),
                new TopLab.Application.Features.SystemAndPrintSettings.Common.PrinterAssignmentDto(
                    TopLab.Domain.Common.Enums.PrinterOutputType.Envelope, "Envelope"),
                new TopLab.Application.Features.SystemAndPrintSettings.Common.PrinterAssignmentDto(
                    TopLab.Domain.Common.Enums.PrinterOutputType.Receipt, "Receipt")
            ]);
        }
        else
        {
            throw new InvalidOperationException("Unhandled command type in test.");
        }

        var authorized = (IAuthorizedRequest)instance;
        Assert.Equal("EDIT_SYSTEM_SETTINGS", authorized.RequiredPermissionCode);
    }

    [Fact]
    public async Task WithoutEditPermission_ReturnsForbidden_WithSharedMessage()
    {
        var user = new FakeCurrentUserService { IsAbsolutePermission = false };
        var behavior = new AuthorizationBehavior<UpdateSystemSettingsCommand, Result>(user);

        var command = new UpdateSystemSettingsCommand(
            TopLab.Domain.Common.Enums.AccountType.Individual,
            false, false, false, false, false, false, false, false,
            TopLab.Domain.Common.Enums.ResultScreenAccountDisplayMode.Hidden,
            false, null);

        var response = await behavior.Handle(command, _ => throw new Exception("handler must not run"), CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Equal(ErrorType.Forbidden, response.Error!.Type);
        Assert.Equal("أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام", response.Error.Message);
    }

    [Fact]
    public async Task AbsoluteUser_BypassesPermissionCheck()
    {
        var user = new FakeCurrentUserService { IsAbsolutePermission = true };
        var behavior = new AuthorizationBehavior<UpdateSystemSettingsCommand, Result>(user);

        var command = new UpdateSystemSettingsCommand(
            TopLab.Domain.Common.Enums.AccountType.Individual,
            false, false, false, false, false, false, false, false,
            TopLab.Domain.Common.Enums.ResultScreenAccountDisplayMode.Hidden,
            false, null);

        var response = await behavior.Handle(command, _ => Task.FromResult(Result.Success()), CancellationToken.None);

        Assert.True(response.IsSuccess);
    }
}