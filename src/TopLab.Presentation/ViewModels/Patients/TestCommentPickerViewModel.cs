using MediatR;
using System.Collections.ObjectModel;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Common;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Queries.GetTestComments;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Queries.SearchTestCatalog;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.ErrorPresentation;

namespace TopLab.Presentation.ViewModels.Patients;

/// <summary>
/// W-02 S8 (WP-13): read-only picker of the standard comments attached to a test.
/// Picking never mutates any PatientTest row — the caller decides what to do with
/// <see cref="PickedCommentText"/> (append to its own note field, or preview only).
/// </summary>
public sealed class TestCommentPickerViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly ResultErrorPresenter _presenter;

    private int _testId;
    private string _testName = string.Empty;
    private TestCommentDto? _selectedComment;
    private string? _pickedCommentText;
    private string _errorMessage = string.Empty;
    private bool _isBusy;

    public TestCommentPickerViewModel(ISender mediator, ResultErrorPresenter presenter)
    {
        _mediator = mediator;
        _presenter = presenter;
        Comments = new ObservableCollection<TestCommentDto>();
    }

    public ObservableCollection<TestCommentDto> Comments { get; }

    public string TestName { get => _testName; private set => SetProperty(ref _testName, value); }

    public TestCommentDto? SelectedComment
    {
        get => _selectedComment;
        set => SetProperty(ref _selectedComment, value);
    }

    public string? PickedCommentText
    {
        get => _pickedCommentText;
        private set => SetProperty(ref _pickedCommentText, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }

    /// <summary>
    /// Entry screens know the test by code, not by id — resolve once, then load.
    /// A single catalog query; no per-row query.
    /// </summary>
    public async Task LoadByTestCodeAsync(string testCode, CancellationToken cancellationToken = default)
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(testCode))
        {
            return;
        }

        var catalog = await _mediator.Send(
            new SearchTestCatalogQuery(testCode.Trim(), null), cancellationToken);
        if (!catalog.IsSuccess || catalog.Value is null)
        {
            if (catalog.Error is not null)
            {
                ErrorMessage = _presenter.Present(catalog.Error);
            }

            return;
        }

        var match = catalog.Value.FirstOrDefault(t => t.TestCode == testCode.Trim());
        if (match is null)
        {
            return;
        }

        await LoadAsync(match.Id, match.Name, cancellationToken);
    }

    public async Task LoadAsync(int testId, string testName, CancellationToken cancellationToken = default)
    {
        _testId = testId;
        TestName = testName;
        PickedCommentText = null;
        ErrorMessage = string.Empty;
        Comments.Clear();

        if (testId <= 0)
        {
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _mediator.Send(new GetTestCommentsQuery(testId), cancellationToken);
            if (result.IsSuccess)
            {
                foreach (var comment in result.Value!)
                {
                    Comments.Add(comment);
                }
            }
            else if (result.Error is not null)
            {
                ErrorMessage = _presenter.Present(result.Error);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    public bool Pick()
    {
        ErrorMessage = string.Empty;

        if (SelectedComment is null)
        {
            ErrorMessage = "اختر تعليقاً من القائمة أولاً.";
            return false;
        }

        PickedCommentText = SelectedComment.CommentText;
        return true;
    }
}
