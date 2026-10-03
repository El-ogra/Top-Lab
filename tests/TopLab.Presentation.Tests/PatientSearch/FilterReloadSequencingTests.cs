using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ExternalEntities.Common;
using TopLab.Application.Features.ExternalEntities.Queries.SearchExternalEntities;
using TopLab.Application.Features.PatientSearch.Common;
using TopLab.Application.Features.PatientSearch.Queries.GetPatientByLabId;
using TopLab.Application.Features.PatientSearch.Queries.SearchPatientsGlobal;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Common;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Queries.SearchTestCatalog;
using TopLab.Domain.Common.Enums;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Tests.Common;
using TopLab.Presentation.ViewModels.Patients;
using Xunit;

namespace TopLab.Presentation.Tests.PatientSearch;

/// <summary>
/// P-02 S3 — D-3: the filter reload is sequenced and surfaces errors.
///
/// Two independent defects are pinned here:
///   * a handler that throws used to lose the failure entirely (`SearchAsync` had no catch);
///   * a burst of filter changes could complete out of order and leave a stale result set
///     on screen.
/// </summary>
public class FilterReloadSequencingTests
{
    /// <summary>
    /// Answers each search either with a controllable delay or by throwing. The delay lets
    /// a test complete two reloads out of their natural order.
    /// </summary>
    private sealed class ScriptedSender : ISender
    {
        private readonly Func<SearchPatientsGlobalQuery, int, Task<Result<IReadOnlyList<PatientSearchHitDto>>>> _handler;

        public ScriptedSender(
            Func<SearchPatientsGlobalQuery, int, Task<Result<IReadOnlyList<PatientSearchHitDto>>>> handler)
            => _handler = handler;

        public List<SearchPatientsGlobalQuery> Searches { get; } = new();
        public int SearchCount => Searches.Count;

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            switch (request)
            {
                case SearchPatientsGlobalQuery search:
                    var index = Searches.Count;
                    Searches.Add(search);
                    return _handler(search, index).ContinueWith(
                        t => (TResponse)(object)t.Result,
                        TaskContinuationOptions.ExecuteSynchronously);

                case SearchExternalEntitiesQuery entities:
                    return Task.FromResult((TResponse)(object)Result<IReadOnlyList<ExternalEntityListItemDto>>
                        .Success(Array.Empty<ExternalEntityListItemDto>()));

                case SearchTestCatalogQuery:
                    return Task.FromResult((TResponse)(object)Result<IReadOnlyList<TestSummaryDto>>
                        .Success(Array.Empty<TestSummaryDto>()));

                case GetPatientByLabIdQuery:
                    return Task.FromResult((TResponse)(object)Result<VisitHistoryDto>
                        .Success(new VisitHistoryDto("L", "P", "ByLabCode", false, Array.Empty<VisitSummaryDto>())));

                default:
                    throw new NotSupportedException($"ScriptedSender has no canned response for {request.GetType().Name}.");
            }
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : IRequest
            => throw new NotSupportedException();

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private static PatientSearchHitDto Hit(int id)
        => new(id, "LAB", $"Patient{id}", null, "Male", 30, "Year", null,
            Array.Empty<string>(), "Individual", false, 0, DateTime.UtcNow, 0);

    private static Task<Result<IReadOnlyList<PatientSearchHitDto>>> Ok(params int[] ids)
        => Task.FromResult(Result<IReadOnlyList<PatientSearchHitDto>>.Success(ids.Select(Hit).ToList()));

    private static PatientSearchViewModel Screen(ScriptedSender sender)
        => new(sender, new ResultErrorPresenter(), new FakeNavigationService());

    // =====================================================================
    // Error surfacing
    // =====================================================================

    [Fact]
    public async Task ThrowingHandler_PopulatesErrorMessage()
    {
        var sender = new ScriptedSender((_, _) =>
            throw new InvalidOperationException("database is unreachable"));

        var vm = Screen(sender);
        vm.SelectedSex = Sex.Male;
        await WaitForAsync(() => vm.ErrorMessage.Length > 0);

        // D-3: the fault is visible instead of vanishing into a discarded task.
        // The presenter maps Error.Unexpected to a generic Arabic message by design —
        // the exception text is deliberately not surfaced to the user.
        Assert.NotEmpty(vm.ErrorMessage);
        Assert.Equal("حدث خطأ غير متوقع. حاول مرة أخرى.", vm.ErrorMessage);
        Assert.DoesNotContain("database is unreachable", vm.ErrorMessage, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ThrowingHandler_PreviousListSurvives()
    {
        var sender = new ScriptedSender((_, index) =>
        {
            if (index == 0)
            {
                return Ok(1, 2, 3);
            }

            throw new InvalidOperationException("database is unreachable");
        });

        var vm = Screen(sender);

        vm.SelectedSex = Sex.Male;
        await WaitForAsync(() => vm.Items.Count > 0);
        Assert.Equal(3, vm.Items.Count);

        vm.SelectedSex = Sex.Female;
        await WaitForAsync(() => vm.ErrorMessage.Length > 0);

        // The user keeps seeing the last good result set rather than an empty grid.
        Assert.Equal(3, vm.Items.Count);
        Assert.All(vm.Items, item => Assert.Equal(Sex.Male.ToString(), item.Sex));
        Assert.NotEmpty(vm.ErrorMessage);
    }

    [Fact]
    public async Task ThrowingHandler_IsBusyIsClearedAfterwards()
    {
        var sender = new ScriptedSender((_, _) => throw new InvalidOperationException("boom"));
        var vm = Screen(sender);

        vm.SelectedSex = Sex.Male;
        await WaitForAsync(() => vm.ErrorMessage.Length > 0);

        Assert.False(vm.IsBusy);
    }

    // =====================================================================
    // Sequencing: the last requested filter set is what is displayed
    // =====================================================================

    [Fact]
    public async Task BurstOfChanges_DisplaysTheLastRequestedFilterSet()
    {
        // The FIRST search is made to finish last, so an unsequenced implementation would
        // end up displaying its result. Each response is keyed to the gender that was set.
        var firstSearchStarted = new TaskCompletionSource();
        var releaseFirstSearch = new TaskCompletionSource();

        var sender = new ScriptedSender(async (query, index) =>
        {
            if (index == 0)
            {
                firstSearchStarted.TrySetResult();
                await releaseFirstSearch.Task;
                return await Ok(1);
            }

            var gender = query.Sex!.Value;
            await Task.Yield();
            return await (gender == Sex.Female ? Ok(30, 31) : Ok(20, 21));
        });

        var vm = Screen(sender);

        vm.SelectedSex = Sex.Male;                      // search #0 — will finish last
        await firstSearchStarted.Task;

        vm.SelectedSex = Sex.Female;                    // search #1 — the newest request
        await WaitForAsync(() => vm.Items.Count > 0);

        // The newest result is already on screen.
        Assert.Equal(new[] { 30, 31 }, vm.Items.Select(i => i.PatientId).ToArray());

        // Now let the stale first search complete. Its result must be discarded.
        releaseFirstSearch.TrySetResult();
        await Task.Delay(100);

        Assert.Equal(new[] { 30, 31 }, vm.Items.Select(i => i.PatientId).ToArray());
    }

    [Fact]
    public async Task BurstOfChanges_SupersededSearchDoesNotOverwriteALaterOne()
    {
        // Two different gender filters, both completing successfully, resolved out of order.
        var firstStarted = new TaskCompletionSource();
        var releaseFirst = new TaskCompletionSource();

        var sender = new ScriptedSender(async (query, index) =>
        {
            if (index == 0)
            {
                firstStarted.TrySetResult();
                await releaseFirst.Task;
                return await Ok(100, 101);            // the STALE result set
            }

            await Task.Yield();
            return await Ok(200, 201, 202);           // the NEWEST result set
        });

        var vm = Screen(sender);

        vm.SelectedSex = Sex.Male;
        await firstStarted.Task;
        vm.SelectedSex = Sex.Female;

        await WaitForAsync(() => vm.Items.Count == 3);

        releaseFirst.TrySetResult();
        await Task.Delay(100);

        // The stale two-row set must not clobber the current three-row set.
        Assert.Equal(3, vm.Items.Count);
        Assert.Equal(new[] { 200, 201, 202 }, vm.Items.Select(i => i.PatientId).ToArray());
    }

    [Fact]
    public async Task RapidBurst_AllReloadsAreIssuedAndTheLastOneWins()
    {
        var sender = new ScriptedSender((query, _) =>
            Task.FromResult(Result<IReadOnlyList<PatientSearchHitDto>>.Success(
                new[] { Hit(query.Sex!.Value == Sex.Female ? 2 : 1) }.ToList())));

        var vm = Screen(sender);

        vm.SelectedSex = Sex.Male;
        vm.SelectedSex = Sex.Female;
        vm.SelectedSex = Sex.Male;
        vm.SelectedSex = Sex.Female;

        await WaitForAsync(() => vm.Items.Count == 1 && vm.Items[0].PatientId == 2);

        Assert.Equal(2, vm.Items[0].PatientId);
        Assert.Empty(vm.ErrorMessage);
    }

    // =====================================================================
    // Cancellation is not an error
    // =====================================================================

    [Fact]
    public async Task OperationCanceledException_DoesNotSurfaceAsAnError()
    {
        var firstStarted = new TaskCompletionSource();

        var sender = new ScriptedSender((_, index) =>
        {
            if (index == 0)
            {
                firstStarted.TrySetResult();
                return Task.FromException<Result<IReadOnlyList<PatientSearchHitDto>>>(
                    new OperationCanceledException());
            }

            return Ok(1, 2);
        });

        var vm = Screen(sender);

        vm.SelectedSex = Sex.Male;
        await firstStarted.Task;
        vm.SelectedSex = Sex.Female;

        await WaitForAsync(() => vm.Items.Count == 2);
        await Task.Delay(50);

        // Cancellation is not a fault: nothing is shown to the user.
        Assert.Empty(vm.ErrorMessage);
        Assert.False(vm.IsBusy);
    }

    // =====================================================================
    // Guards
    // =====================================================================

    private static async Task WaitForAsync(Func<bool> condition, int timeoutMs = 5000)
    {
        var deadline = Environment.TickCount64 + timeoutMs;
        while (Environment.TickCount64 < deadline)
        {
            if (condition())
            {
                return;
            }

            await Task.Delay(10);
        }

        Assert.True(condition(), "timed out waiting for the expected state");
    }
}