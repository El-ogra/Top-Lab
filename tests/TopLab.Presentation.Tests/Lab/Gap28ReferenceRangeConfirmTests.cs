using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.CreateReferenceRange;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.UpdateReferenceRange;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Common;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Queries.GetReferenceRanges;
using TopLab.Domain.Common.Enums;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Tests.Common;
using TopLab.Presentation.ViewModels.Lab;
using Xunit;

namespace TopLab.Presentation.Tests.Lab;

/// <summary>
/// GAP-28 (Bucket A): reference-range confirm step. A confirmation dialog must
/// appear before <see cref="CreateReferenceRangeCommand"/> or
/// <see cref="UpdateReferenceRangeCommand"/> is dispatched. Cancelling must not
/// dispatch any command, must not clear the editor, and must not reload ranges.
/// </summary>
public class Gap28ReferenceRangeConfirmTests
{
    private const string CreateTitle = "تأكيد حفظ المدى المرجعي";

    private sealed class RecordingDialogService : IDialogService
    {
        private readonly List<string> _events;

        public RecordingDialogService(List<string> events, bool confirmationResult)
        {
            _events = events;
            ConfirmationResult = confirmationResult;
        }

        public bool ConfirmationResult { get; }
        public List<(string Title, string Message)> ConfirmationCalls { get; } = new();

        public Task<bool> ShowConfirmationAsync(string title, string message)
        {
            ConfirmationCalls.Add((title, message));
            _events.Add("dialog");
            return Task.FromResult(ConfirmationResult);
        }

        public Task ShowErrorAsync(string message) => Task.CompletedTask;
        public Task<bool> ShowSecondaryPasswordDialogAsync() => Task.FromResult(false);
        public Task<string?> PickBackupFolderAsync(string initialDirectory) => Task.FromResult<string?>(null);
        public Task<string?> PickBackupFileAsync() => Task.FromResult<string?>(null);
        public Task<string?> PickPdfSavePathAsync(string? suggestedFileName = null) => Task.FromResult<string?>(null);
    }

    /// <summary>
    /// Records every dispatched request in order, delegating canned responses to
    /// the shared <see cref="FakeSender"/>. The event list is shared with the
    /// dialog fake so that ordering ("dialog before dispatch") can be asserted.
    /// </summary>
    private sealed class RecordingSender : ISender
    {
        private readonly FakeSender _inner = new();
        private readonly List<string> _events;

        public RecordingSender(List<string> events)
        {
            _events = events;
        }

        public List<object> SentRequests { get; } = new();

        public RecordingSender WithResponse<TResponse>(IRequest<TResponse> request, TResponse response)
        {
            _inner.WithResponse(request, response);
            return this;
        }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            _events.Add("send:" + request.GetType().Name);
            SentRequests.Add(request);
            return _inner.Send(request, cancellationToken);
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("RecordingSender does not support non-generic Send.");

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : IRequest =>
            throw new NotSupportedException("RecordingSender does not support void Send.");

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("RecordingSender does not support streams.");

        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("RecordingSender does not support streams.");
    }

    private static RecordingSender CreateSender(List<string> events)
    {
        var sender = new RecordingSender(events);
        sender.WithResponse(
            new GetReferenceRangesQuery(1),
            Result<IReadOnlyList<ReferenceRangeDto>>.Success(Array.Empty<ReferenceRangeDto>()));
        sender.WithResponse(
            new CreateReferenceRangeCommand(1, null, AgeUnit.Year, 0, 120, 10m, 20m, null, null),
            Result<int>.Success(1));
        sender.WithResponse(
            new UpdateReferenceRangeCommand(7, null, AgeUnit.Year, 0, 120, 10m, 20m, null, null),
            Result.Success());
        return sender;
    }

    private static TestEditorViewModel CreateViewModel(RecordingSender sender, RecordingDialogService dialogs)
    {
        var vm = new TestEditorViewModel(sender, dialogs, new ResultErrorPresenter());
        vm.InitializeEdit(1);
        return vm;
    }

    [Fact]
    public async Task SaveRange_CreatePath_Confirm_ShowsDialogThenDispatchesCreateCommand()
    {
        var events = new List<string>();
        var sender = CreateSender(events);
        var dialogs = new RecordingDialogService(events, confirmationResult: true);
        var vm = CreateViewModel(sender, dialogs);

        vm.SelectedRange = null;
        vm.RangeAgeMin = 0;
        vm.RangeAgeMax = 120;
        vm.RangeMinValue = 10m;
        vm.RangeMaxValue = 20m;

        await vm.SaveRangeAsync();

        var call = Assert.Single(dialogs.ConfirmationCalls);
        Assert.Equal(CreateTitle, call.Title);
        Assert.False(string.IsNullOrWhiteSpace(call.Message));

        Assert.Single(sender.SentRequests.OfType<CreateReferenceRangeCommand>());
        Assert.Empty(sender.SentRequests.OfType<UpdateReferenceRangeCommand>());

        // Dialog is shown before the create command is dispatched.
        Assert.Equal("dialog", events[0]);
        Assert.Equal("send:" + nameof(CreateReferenceRangeCommand), events[1]);
    }

    [Fact]
    public async Task SaveRange_UpdatePath_Confirm_ShowsDialogThenDispatchesUpdateCommand()
    {
        var events = new List<string>();
        var sender = CreateSender(events);
        var dialogs = new RecordingDialogService(events, confirmationResult: true);
        var vm = CreateViewModel(sender, dialogs);

        vm.SelectedRange = new ReferenceRangeDto(7, 1, null, AgeUnit.Year, 0, 120, 10m, 20m, null, null);

        await vm.SaveRangeAsync();

        var call = Assert.Single(dialogs.ConfirmationCalls);
        Assert.Equal(CreateTitle, call.Title);

        Assert.Single(sender.SentRequests.OfType<UpdateReferenceRangeCommand>());
        Assert.Empty(sender.SentRequests.OfType<CreateReferenceRangeCommand>());

        // Dialog is shown before the update command is dispatched.
        Assert.Equal("dialog", events[0]);
        Assert.Equal("send:" + nameof(UpdateReferenceRangeCommand), events[1]);
    }

    [Fact]
    public async Task SaveRange_CreatePath_Cancel_DispatchesNoCommand_AndRetainsEditor()
    {
        var events = new List<string>();
        var sender = CreateSender(events);
        var dialogs = new RecordingDialogService(events, confirmationResult: false);
        var vm = CreateViewModel(sender, dialogs);

        vm.SelectedRange = null;
        vm.RangeAgeMin = 5;
        vm.RangeAgeMax = 60;
        vm.RangeAgeUnit = AgeUnit.Month;
        vm.RangeSex = Sex.Male;
        vm.RangeMinValue = 10m;
        vm.RangeMaxValue = 20m;
        vm.RangeLowComment = "low";
        vm.RangeHighComment = "high";

        await vm.SaveRangeAsync();

        Assert.Single(dialogs.ConfirmationCalls);
        Assert.Empty(sender.SentRequests);
        Assert.Empty(sender.SentRequests.OfType<CreateReferenceRangeCommand>());
        Assert.Empty(sender.SentRequests.OfType<UpdateReferenceRangeCommand>());

        // Editor was not cleared.
        Assert.Null(vm.SelectedRange);
        Assert.Equal(5, vm.RangeAgeMin);
        Assert.Equal(60, vm.RangeAgeMax);
        Assert.Equal(AgeUnit.Month, vm.RangeAgeUnit);
        Assert.Equal(Sex.Male, vm.RangeSex);
        Assert.Equal(10m, vm.RangeMinValue);
        Assert.Equal(20m, vm.RangeMaxValue);
        Assert.Equal("low", vm.RangeLowComment);
        Assert.Equal("high", vm.RangeHighComment);
    }

    [Fact]
    public async Task SaveRange_UpdatePath_Cancel_DispatchesNoCommand_AndRetainsSelection()
    {
        var events = new List<string>();
        var sender = CreateSender(events);
        var dialogs = new RecordingDialogService(events, confirmationResult: false);
        var vm = CreateViewModel(sender, dialogs);

        vm.SelectedRange = new ReferenceRangeDto(7, 1, Sex.Female, AgeUnit.Year, 18, 65, 3m, 5m, "منخفض", "مرتفع");
        vm.RangeMinValue = 4m;

        await vm.SaveRangeAsync();

        Assert.Single(dialogs.ConfirmationCalls);
        Assert.Empty(sender.SentRequests);
        Assert.Empty(sender.SentRequests.OfType<UpdateReferenceRangeCommand>());

        // Editor was not cleared and the selected range is still present.
        Assert.NotNull(vm.SelectedRange);
        Assert.Equal(7, vm.SelectedRange!.Id);
        Assert.Equal(4m, vm.RangeMinValue);
        Assert.Equal("منخفض", vm.RangeLowComment);
    }

    [Fact]
    public async Task SaveRange_Cancel_DoesNotReloadRanges()
    {
        var events = new List<string>();
        var sender = CreateSender(events);
        var dialogs = new RecordingDialogService(events, confirmationResult: false);
        var vm = CreateViewModel(sender, dialogs);

        vm.SelectedRange = null;
        vm.RangeMinValue = 42m;

        await vm.SaveRangeAsync();

        Assert.DoesNotContain(sender.SentRequests, r => r is GetReferenceRangesQuery);
        Assert.Empty(sender.SentRequests);
    }

    [Fact]
    public async Task SaveRange_Confirm_UsesSameConfirmationDialogPatternAsDeleteRange()
    {
        var events = new List<string>();
        var sender = CreateSender(events);
        var dialogs = new RecordingDialogService(events, confirmationResult: false);
        var vm = CreateViewModel(sender, dialogs);

        vm.SelectedRange = new ReferenceRangeDto(7, 1, null, AgeUnit.Year, 0, 120, 10m, 20m, null, null);

        // Save (cancelled) — dialog must be requested before anything is dispatched.
        await vm.SaveRangeAsync();
        var saveCall = Assert.Single(dialogs.ConfirmationCalls);
        Assert.Equal(CreateTitle, saveCall.Title);
        Assert.EndsWith("متابعة؟", saveCall.Message);
        Assert.DoesNotContain(events, e => e.StartsWith("send:", StringComparison.Ordinal));

        // Delete (cancelled) — the pre-existing pattern used as the reference.
        dialogs.ConfirmationCalls.Clear();
        await vm.DeleteRangeAsync();
        var deleteCall = Assert.Single(dialogs.ConfirmationCalls);
        Assert.Equal("حذف المدى المرجعي", deleteCall.Title);
        Assert.EndsWith("متابعة؟", deleteCall.Message);
    }

    [Fact]
    public async Task SaveRange_EditingTestIdZero_DoesNotShowDialog()
    {
        var events = new List<string>();
        var sender = CreateSender(events);
        var dialogs = new RecordingDialogService(events, confirmationResult: true);
        var vm = new TestEditorViewModel(sender, dialogs, new ResultErrorPresenter());

        vm.SelectedRange = null;

        await vm.SaveRangeAsync();

        Assert.Empty(dialogs.ConfirmationCalls);
        Assert.Empty(sender.SentRequests);
        Assert.False(string.IsNullOrWhiteSpace(vm.ErrorMessage));
    }
}
