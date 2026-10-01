using TopLab.Application.Common.Results;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Common;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Queries.GetTestComments;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Common;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Queries.SearchTestCatalog;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Tests.Common;
using TopLab.Presentation.ViewModels.Patients;
using Xunit;

namespace TopLab.Presentation.Tests;

/// <summary>W-02 S8 (WP-13): the comment picker reads but never writes.</summary>
public class TestCommentPickerTests
{
    private static TestCommentPickerViewModel Picker(FakeSender sender) =>
        new(sender, new ResultErrorPresenter());

    [Fact]
    public async Task Picker_LoadsCommentsForTest()
    {
        var sender = new FakeSender()
            .WithResponse(
                new SearchTestCatalogQuery("GLU", null),
                Result<IReadOnlyList<TestSummaryDto>>.Success(
                    new[] { new TestSummaryDto(2, "GLU", "Glucose", "Glucose", null, null, null, 1, false, 100m, null, true) }))
            .WithResponse(
                new GetTestCommentsQuery(2),
                Result<IReadOnlyList<TestCommentDto>>.Success(
                    new[] { new TestCommentDto(1, 2, "Glucose", "صائم") }));

        var vm = Picker(sender);
        await vm.LoadByTestCodeAsync("GLU");

        Assert.Equal("Glucose", vm.TestName);
        Assert.Equal("صائم", Assert.Single(vm.Comments).CommentText);
    }

    [Fact]
    public async Task Picker_Pick_SetsPickedTextWithoutMutating()
    {
        var sender = new FakeSender()
            .WithResponse(
                new SearchTestCatalogQuery("GLU", null),
                Result<IReadOnlyList<TestSummaryDto>>.Success(
                    new[] { new TestSummaryDto(2, "GLU", "Glucose", "Glucose", null, null, null, 1, false, 100m, null, true) }))
            .WithResponse(
                new GetTestCommentsQuery(2),
                Result<IReadOnlyList<TestCommentDto>>.Success(
                    new[] { new TestCommentDto(1, 2, "Glucose", "صائم") }));

        var vm = Picker(sender);
        await vm.LoadByTestCodeAsync("GLU");
        vm.SelectedComment = vm.Comments[0];

        Assert.True(vm.Pick());
        Assert.Equal("صائم", vm.PickedCommentText);
        Assert.Single(vm.Comments);
    }

    [Fact]
    public void Picker_Pick_WithoutSelection_ShowsMessage()
    {
        var vm = Picker(new FakeSender());

        Assert.False(vm.Pick());
        Assert.Equal("اختر تعليقاً من القائمة أولاً.", vm.ErrorMessage);
        Assert.Null(vm.PickedCommentText);
    }
}
