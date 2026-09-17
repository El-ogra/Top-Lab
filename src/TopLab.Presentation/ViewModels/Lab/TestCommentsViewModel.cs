using System.Collections.ObjectModel;
using MediatR;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.CreateTestComment;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.DeleteTestComment;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.UpdateTestComment;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Common;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Queries.GetTestComments;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Common;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Queries.SearchTestCatalog;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.ErrorPresentation;

namespace TopLab.Presentation.ViewModels.Lab;

/// <summary>
/// Test-comments tab (S-02 Slice 7): grid + picker/text editor.
/// Multiple comments per test are allowed (no uniqueness constraint).
/// </summary>
public sealed class TestCommentsViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly IDialogService _dialogs;
    private readonly ResultErrorPresenter _presenter;

    private ObservableCollection<TestCommentDto> _comments = new();
    private TestCommentDto? _selectedComment;
    private ObservableCollection<TestSummaryDto> _catalogTests = new();
    private TestSummaryDto? _pickedTest;
    private string _commentText = string.Empty;
    private bool _isBusy;
    private string _errorMessage = string.Empty;
    private string _statusMessage = string.Empty;

    public TestCommentsViewModel(
        ISender mediator,
        IDialogService dialogs,
        ResultErrorPresenter presenter)
    {
        _mediator = mediator;
        _dialogs = dialogs;
        _presenter = presenter;

        LoadCommentsCommand = new AsyncRelayCommand(_ => LoadAsync());
        NewCommand = new RelayCommand(_ => ClearEditor());
        SaveCommand = new AsyncRelayCommand(_ => SaveAsync());
        DeleteCommand = new AsyncRelayCommand((p, _) => DeleteAsync(p as TestCommentDto));
    }

    public ObservableCollection<TestCommentDto> Comments { get => _comments; private set => SetProperty(ref _comments, value); }

    public TestCommentDto? SelectedComment
    {
        get => _selectedComment;
        set
        {
            if (SetProperty(ref _selectedComment, value) && value is not null)
            {
                CommentText = value.CommentText;
                PickedTest = CatalogTests.FirstOrDefault(t => t.Id == value.TestId);
            }
        }
    }

    public ObservableCollection<TestSummaryDto> CatalogTests { get => _catalogTests; private set => SetProperty(ref _catalogTests, value); }
    public TestSummaryDto? PickedTest { get => _pickedTest; set => SetProperty(ref _pickedTest, value); }
    public string CommentText { get => _commentText; set => SetProperty(ref _commentText, value); }
    public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }
    public string ErrorMessage { get => _errorMessage; private set => SetProperty(ref _errorMessage, value); }
    public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }

    public AsyncRelayCommand LoadCommentsCommand { get; }
    public RelayCommand NewCommand { get; }
    public AsyncRelayCommand SaveCommand { get; }
    public AsyncRelayCommand DeleteCommand { get; }

    public async Task LoadAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var commentsResult = await _mediator.Send(new GetTestCommentsQuery());
            if (commentsResult.IsSuccess && commentsResult.Value is not null)
            {
                Comments = new ObservableCollection<TestCommentDto>(commentsResult.Value);
            }
            else if (commentsResult.Error is not null)
            {
                ErrorMessage = _presenter.Present(commentsResult.Error);
            }

            var catalogResult = await _mediator.Send(new SearchTestCatalogQuery(null, null, IncludeInactive: false));
            if (catalogResult.IsSuccess && catalogResult.Value is not null)
            {
                CatalogTests = new ObservableCollection<TestSummaryDto>(catalogResult.Value);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ClearEditor()
    {
        SelectedComment = null;
        PickedTest = null;
        CommentText = string.Empty;
    }

    public async Task SaveAsync()
    {
        if (PickedTest is null)
        {
            ErrorMessage = "اختر التحليل.";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        try
        {
            if (SelectedComment is null)
            {
                var result = await _mediator.Send(new CreateTestCommentCommand(PickedTest.Id, CommentText));
                if (!result.IsSuccess)
                {
                    ErrorMessage = result.Error is not null ? _presenter.Present(result.Error) : ErrorMessage;
                    return;
                }

                StatusMessage = "تم إنشاء التعليق.";
            }
            else
            {
                var result = await _mediator.Send(new UpdateTestCommentCommand(SelectedComment.Id, CommentText));
                if (!result.IsSuccess)
                {
                    ErrorMessage = result.Error is not null ? _presenter.Present(result.Error) : ErrorMessage;
                    return;
                }

                StatusMessage = "تم حفظ التعليق.";
            }

            ClearEditor();
            await LoadAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task DeleteAsync(TestCommentDto? dto)
    {
        if (dto is null)
        {
            return;
        }

        bool confirm = await _dialogs.ShowConfirmationAsync(
            "حذف التعليق",
            $"سيتم حذف التعليق الخاص بالتحليل \"{dto.TestName}\" — متابعة؟");
        if (!confirm)
        {
            return;
        }

        var result = await _mediator.Send(new DeleteTestCommentCommand(dto.Id));
        if (!result.IsSuccess)
        {
            ErrorMessage = result.Error is not null ? _presenter.Present(result.Error) : ErrorMessage;
            return;
        }

        StatusMessage = "تم حذف التعليق.";
        await LoadAsync();
    }
}
