using TopLab.Application.Common.Authorization;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.CreateCustomGroup;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.CreatePriceList;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.CreateTestComment;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.DeleteCustomGroup;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.DeletePriceList;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.DeleteTestComment;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.RemoveCustomGroupItem;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.RemovePriceListItem;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.RenameCustomGroup;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.RenamePriceList;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.SetCustomGroupItemPrice;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.SetPriceListItemPrice;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.UpdateTestComment;
using Xunit;

namespace TopLab.Application.Tests.Features.PriceListsCommentsAndCustomGroups;

public class PriceListsCommentsAndCustomGroupsAuthorizationTests
{
    private static object CreateInstance(System.Type commandType)
    {
        return commandType switch
        {
            { } t when t == typeof(CreatePriceListCommand) => new CreatePriceListCommand("قائمة"),
            { } t when t == typeof(RenamePriceListCommand) => new RenamePriceListCommand(1, "قائمة"),
            { } t when t == typeof(DeletePriceListCommand) => new DeletePriceListCommand(1),
            { } t when t == typeof(SetPriceListItemPriceCommand) => new SetPriceListItemPriceCommand(1, 1, 10m),
            { } t when t == typeof(RemovePriceListItemCommand) => new RemovePriceListItemCommand(1, 1),
            { } t when t == typeof(CreateTestCommentCommand) => new CreateTestCommentCommand(1, "تعليق"),
            { } t when t == typeof(UpdateTestCommentCommand) => new UpdateTestCommentCommand(1, "تعليق"),
            { } t when t == typeof(DeleteTestCommentCommand) => new DeleteTestCommentCommand(1),
            { } t when t == typeof(CreateCustomGroupCommand) => new CreateCustomGroupCommand("مجموعة"),
            { } t when t == typeof(RenameCustomGroupCommand) => new RenameCustomGroupCommand(1, "مجموعة"),
            { } t when t == typeof(DeleteCustomGroupCommand) => new DeleteCustomGroupCommand(1),
            { } t when t == typeof(SetCustomGroupItemPriceCommand) => new SetCustomGroupItemPriceCommand(1, 1, 10m),
            { } t when t == typeof(RemoveCustomGroupItemCommand) => new RemoveCustomGroupItemCommand(1, 1),
            _ => throw new InvalidOperationException($"Unhandled command type {commandType.Name} in test.")
        };
    }

    [Theory]
    [InlineData(typeof(CreatePriceListCommand))]
    [InlineData(typeof(RenamePriceListCommand))]
    [InlineData(typeof(DeletePriceListCommand))]
    [InlineData(typeof(SetPriceListItemPriceCommand))]
    [InlineData(typeof(RemovePriceListItemCommand))]
    [InlineData(typeof(CreateTestCommentCommand))]
    [InlineData(typeof(UpdateTestCommentCommand))]
    [InlineData(typeof(DeleteTestCommentCommand))]
    [InlineData(typeof(CreateCustomGroupCommand))]
    [InlineData(typeof(RenameCustomGroupCommand))]
    [InlineData(typeof(DeleteCustomGroupCommand))]
    [InlineData(typeof(SetCustomGroupItemPriceCommand))]
    [InlineData(typeof(RemoveCustomGroupItemCommand))]
    public void EveryWriteCommand_Requires_EditSystemSettings(System.Type commandType)
    {
        var authorized = (IAuthorizedRequest)CreateInstance(commandType);

        Assert.Equal("EDIT_SYSTEM_SETTINGS", authorized.RequiredPermissionCode);
    }
}
