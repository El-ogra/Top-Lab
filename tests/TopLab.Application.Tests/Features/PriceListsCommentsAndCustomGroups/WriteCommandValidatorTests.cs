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

public class WriteCommandValidatorTests
{
    // ---------- Name ----------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreatePriceList_NameWhitespace_Invalid(string? name)
    {
        var result = new CreatePriceListCommandValidator().Validate(new CreatePriceListCommand(name!));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void CreatePriceList_NameOver150_Invalid()
    {
        var result = new CreatePriceListCommandValidator().Validate(new CreatePriceListCommand(new string('a', 151)));
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void RenamePriceList_NameWhitespace_Invalid(string? name)
    {
        var result = new RenamePriceListCommandValidator().Validate(new RenamePriceListCommand(1, name!));
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RenamePriceList_IdInvalid_Invalid(int id)
    {
        var result = new RenamePriceListCommandValidator().Validate(new RenamePriceListCommand(id, "X"));
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void DeletePriceList_IdInvalid_Invalid(int id)
    {
        var result = new DeletePriceListCommandValidator().Validate(new DeletePriceListCommand(id));
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateCustomGroup_NameWhitespace_Invalid(string? name)
    {
        var result = new CreateCustomGroupCommandValidator().Validate(new CreateCustomGroupCommand(name!));
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RenameCustomGroup_IdInvalid_Invalid(int id)
    {
        var result = new RenameCustomGroupCommandValidator().Validate(new RenameCustomGroupCommand(id, "X"));
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void DeleteCustomGroup_IdInvalid_Invalid(int id)
    {
        var result = new DeleteCustomGroupCommandValidator().Validate(new DeleteCustomGroupCommand(id));
        Assert.False(result.IsValid);
    }

    // ---------- CommentText ----------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateTestComment_TextWhitespace_Invalid(string? text)
    {
        var result = new CreateTestCommentCommandValidator().Validate(new CreateTestCommentCommand(1, text!));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void CreateTestComment_TextOver1000_Invalid()
    {
        var result = new CreateTestCommentCommandValidator()
            .Validate(new CreateTestCommentCommand(1, new string('a', 1001)));
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void UpdateTestComment_IdInvalid_Invalid(int id)
    {
        var result = new UpdateTestCommentCommandValidator().Validate(new UpdateTestCommentCommand(id, "X"));
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void DeleteTestComment_IdInvalid_Invalid(int id)
    {
        var result = new DeleteTestCommentCommandValidator().Validate(new DeleteTestCommentCommand(id));
        Assert.False(result.IsValid);
    }

    // ---------- Price / IDs ----------

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void SetPriceListItemPrice_ListIdInvalid_Invalid(int id)
    {
        var result = new SetPriceListItemPriceCommandValidator().Validate(new SetPriceListItemPriceCommand(id, 1, 10m));
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void SetPriceListItemPrice_TestIdInvalid_Invalid(int id)
    {
        var result = new SetPriceListItemPriceCommandValidator().Validate(new SetPriceListItemPriceCommand(1, id, 10m));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void SetPriceListItemPrice_NegativePrice_Invalid()
    {
        var result = new SetPriceListItemPriceCommandValidator().Validate(new SetPriceListItemPriceCommand(1, 1, -1m));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void SetPriceListItemPrice_ZeroPrice_Valid()
    {
        var result = new SetPriceListItemPriceCommandValidator().Validate(new SetPriceListItemPriceCommand(1, 1, 0m));
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RemovePriceListItem_PriceListIdInvalid_Invalid(int id)
    {
        var result = new RemovePriceListItemCommandValidator().Validate(new RemovePriceListItemCommand(id, 1));
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void SetCustomGroupItemPrice_GroupIdInvalid_Invalid(int id)
    {
        var result = new SetCustomGroupItemPriceCommandValidator().Validate(new SetCustomGroupItemPriceCommand(id, 1, 10m));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void SetCustomGroupItemPrice_NegativePrice_Invalid()
    {
        var result = new SetCustomGroupItemPriceCommandValidator().Validate(new SetCustomGroupItemPriceCommand(1, 1, -1m));
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RemoveCustomGroupItem_GroupIdInvalid_Invalid(int id)
    {
        var result = new RemoveCustomGroupItemCommandValidator().Validate(new RemoveCustomGroupItemCommand(id, 1));
        Assert.False(result.IsValid);
    }
}
