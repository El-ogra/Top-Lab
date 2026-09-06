using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Behaviors;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.CreateReferenceRange;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.CreateTest;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.CreateTestGroup;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.CreateWorkGroupLog;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.DeactivateTest;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.DeactivateTestGroup;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.DeleteReferenceRange;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.ReactivateTest;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.ReactivateTestGroup;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.RenameWorkGroupLog;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.SaveWorkGroupLogItems;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.UpdateReferenceRange;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.UpdateTest;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.UpdateTestGroup;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;

namespace TopLab.Application.Tests.Features.TestCatalogAndReferenceRanges;

public class TestCatalogAndReferenceRangesAuthorizationTests
{
    private static object CreateInstance(System.Type commandType)
    {
        return commandType switch
        {
            { } t when t == typeof(CreateTestCommand) => new CreateTestCommand(
                "تحليل عام", "تقرير", "إيصال", "CBC", 30, 150m, ResultKind.Simple, false, null, null, false, null, null),
            { } t when t == typeof(UpdateTestCommand) => new UpdateTestCommand(
                1, "تحليل عام", "تقرير", "إيصال", "CBC", 30, 150m, null, null, false, null, null),
            { } t when t == typeof(DeactivateTestCommand) => new DeactivateTestCommand(1),
            { } t when t == typeof(ReactivateTestCommand) => new ReactivateTestCommand(1),
            { } t when t == typeof(CreateTestGroupCommand) => new CreateTestGroupCommand("هيماتولوجيا"),
            { } t when t == typeof(UpdateTestGroupCommand) => new UpdateTestGroupCommand(1, "هيماتولوجيا"),
            { } t when t == typeof(DeactivateTestGroupCommand) => new DeactivateTestGroupCommand(1),
            { } t when t == typeof(ReactivateTestGroupCommand) => new ReactivateTestGroupCommand(1),
            { } t when t == typeof(CreateWorkGroupLogCommand) => new CreateWorkGroupLogCommand("ورشة المناعة"),
            { } t when t == typeof(RenameWorkGroupLogCommand) => new RenameWorkGroupLogCommand(1, "ورشة المناعة"),
            { } t when t == typeof(SaveWorkGroupLogItemsCommand) => new SaveWorkGroupLogItemsCommand(1, new[] { 1 }),
            { } t when t == typeof(CreateReferenceRangeCommand) => new CreateReferenceRangeCommand(1, null, AgeUnit.Year, 0, 120, 0m, 100m, null, null),
            { } t when t == typeof(UpdateReferenceRangeCommand) => new UpdateReferenceRangeCommand(1, null, AgeUnit.Year, 0, 120, 0m, 100m, null, null),
            { } t when t == typeof(DeleteReferenceRangeCommand) => new DeleteReferenceRangeCommand(1),
            _ => throw new InvalidOperationException($"Unhandled command type {commandType.Name} in test.")
        };
    }

    [Theory]
    [InlineData(typeof(CreateTestCommand))]
    [InlineData(typeof(UpdateTestCommand))]
    [InlineData(typeof(DeactivateTestCommand))]
    [InlineData(typeof(ReactivateTestCommand))]
    [InlineData(typeof(CreateTestGroupCommand))]
    [InlineData(typeof(UpdateTestGroupCommand))]
    [InlineData(typeof(DeactivateTestGroupCommand))]
    [InlineData(typeof(ReactivateTestGroupCommand))]
    [InlineData(typeof(CreateWorkGroupLogCommand))]
    [InlineData(typeof(RenameWorkGroupLogCommand))]
    [InlineData(typeof(SaveWorkGroupLogItemsCommand))]
    [InlineData(typeof(CreateReferenceRangeCommand))]
    [InlineData(typeof(UpdateReferenceRangeCommand))]
    [InlineData(typeof(DeleteReferenceRangeCommand))]
    public void EveryWriteCommand_Requires_EditSystemSettings(System.Type commandType)
    {
        var authorized = (IAuthorizedRequest)CreateInstance(commandType);

        Assert.Equal("EDIT_SYSTEM_SETTINGS", authorized.RequiredPermissionCode);
    }

    [Fact]
    public async Task WithoutEditPermission_ReturnsForbidden_WithSharedMessage()
    {
        var user = new FakeCurrentUserService { IsAbsolutePermission = false };
        var behavior = new AuthorizationBehavior<SaveWorkGroupLogItemsCommand, Result>(user);

        var response = await behavior.Handle(
            new SaveWorkGroupLogItemsCommand(1, new[] { 1 }),
            _ => throw new Exception("handler must not run"),
            CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Equal(ErrorType.Forbidden, response.Error!.Type);
        Assert.Equal("أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام", response.Error.Message);
    }

    [Fact]
    public async Task WithoutEditPermission_ReturnsForbidden_ForGenericResult()
    {
        var user = new FakeCurrentUserService { IsAbsolutePermission = false };
        var behavior = new AuthorizationBehavior<CreateTestCommand, Result<int>>(user);

        var response = await behavior.Handle(
            new CreateTestCommand("تحليل عام", "تقرير", "إيصال", "CBC", 30, 150m, ResultKind.Simple, false, null, null, false, null, null),
            _ => throw new Exception("handler must not run"),
            CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Equal(ErrorType.Forbidden, response.Error!.Type);
    }

    [Fact]
    public async Task AbsoluteUser_BypassesPermissionCheck()
    {
        var user = new FakeCurrentUserService { IsAbsolutePermission = true };
        var behavior = new AuthorizationBehavior<CreateTestCommand, Result<int>>(user);

        var response = await behavior.Handle(
            new CreateTestCommand("تحليل عام", "تقرير", "إيصال", "CBC", 30, 150m, ResultKind.Simple, false, null, null, false, null, null),
            _ => Task.FromResult(Result<int>.Success(1)),
            CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(1, response.Value);
    }
}