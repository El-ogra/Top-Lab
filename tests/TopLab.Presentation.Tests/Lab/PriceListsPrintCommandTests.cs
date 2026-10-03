using System.IO;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Common;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Queries.GetPriceListById;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Queries.GetPriceLists;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Tests.Common;
using TopLab.Presentation.ViewModels.Lab;
using MediatR;
using Xunit;

namespace TopLab.Presentation.Tests.Lab;

/// <summary>
/// P-01 F8 (PP-03) — the price-list print command.
///
/// The writer is faked here (a hand-rolled fake, no mocking library) so this test
/// proves the VIEWMODEL wiring only; the real PDF is proven by
/// <c>PriceListPdfWriterTests</c> in the Infrastructure project.
/// </summary>
public class PriceListsPrintCommandTests
{
    private sealed class RecordingPriceListPdfWriter : IPriceListPdfWriter
    {
        private readonly TaskCompletionSource _written = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public int CallCount { get; private set; }
        public string? LastPath { get; private set; }
        public PriceListDetailDto? LastList { get; private set; }

        /// <summary>Completes once the command has actually reached the port.</summary>
        public Task Written => _written.Task;

        public Task WritePdfAsync(
            string absolutePath,
            PriceListDetailDto priceList,
            LabPrintTextDto labText,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            LastPath = absolutePath;
            LastList = priceList;
            _written.TrySetResult();
            return Task.CompletedTask;
        }
    }

    private sealed class PriceListSender : ISender
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is GetPriceListByIdQuery)
            {
                var detail = new PriceListDetailDto(7, "قائمة أسعار الدم", new[]
                {
                    new PriceListItemDto(101, "صورة دم كاملة", "CBC", 250m)
                });
                return Task.FromResult((TResponse)(object)Result<PriceListDetailDto>.Success(detail));
            }

            if (request is GetPriceListsQuery)
            {
                return Task.FromResult((TResponse)(object)Result<IReadOnlyList<PriceListSummaryDto>>
                    .Success(Array.Empty<PriceListSummaryDto>()));
            }

            throw new NotSupportedException($"PriceListSender has no canned response for {request.GetType().Name}.");
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

    private sealed class FixedDialogService : IDialogService
    {
        private readonly string? _path;
        public FixedDialogService(string? path) => _path = path;

        public Task<bool> ShowConfirmationAsync(string title, string message) => Task.FromResult(true);
        public Task ShowErrorAsync(string message) => Task.CompletedTask;
        public Task<bool> ShowSecondaryPasswordDialogAsync() => Task.FromResult(true);
        public Task<string?> PickBackupFolderAsync(string initialDirectory) => Task.FromResult<string?>(null);
        public Task<string?> PickBackupFileAsync() => Task.FromResult<string?>(null);
        public Task<string?> PickPdfSavePathAsync(string? suggestedFileName = null) => Task.FromResult(_path);
    }

    private static PriceListsViewModel Screen(out RecordingPriceListPdfWriter writer, string? savePath = "C:/temp/list.pdf")
    {
        writer = new RecordingPriceListPdfWriter();
        return new PriceListsViewModel(
            new PriceListSender(),
            new FixedDialogService(savePath),
            new ResultErrorPresenter(),
            writer,
            new FakeLabPrintText());
    }

    private sealed class FakeLabPrintText : ILabPrintTextStore
    {
        public Task<Result<LabPrintTextDto>> GetAsync(LabPrintTextScope scope, CancellationToken cancellationToken = default)
            => Task.FromResult(Result<LabPrintTextDto>.Success(
                new LabPrintTextDto("معمل النور", "القاهرة", "0100000000", "Arial", 11)));

        public Task<Result> SaveAsync(LabPrintTextScope scope, LabPrintTextDto content, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success());
    }

    [Fact]
    public void PriceListsViewModel_PrintCommand_DisabledWithNoSelection()
    {
        var vm = Screen(out _);

        // No half-wired state: nothing selected means nothing to print.
        Assert.NotNull(vm.PrintListCommand);
        Assert.False(vm.PrintListCommand.CanExecute(null));
    }

    [Fact]
    public void PriceListsViewModel_PrintCommand_EnabledWithSelection()
    {
        var vm = Screen(out _);
        vm.SelectedList = new PriceListSummaryDto(7, "قائمة أسعار الدم", 1);

        Assert.True(vm.PrintListCommand.CanExecute(null));
    }

    [Fact]
    public void PriceListsViewModel_PrintCommand_DisabledAgainWhenSelectionCleared()
    {
        var vm = Screen(out _);
        vm.SelectedList = new PriceListSummaryDto(7, "قائمة أسعار الدم", 1);
        Assert.True(vm.PrintListCommand.CanExecute(null));

        vm.SelectedList = null;
        Assert.False(vm.PrintListCommand.CanExecute(null));
    }

    [Fact]
    public async Task PriceListsViewModel_PrintCommand_WritesThroughThePort()
    {
        var vm = Screen(out var writer, "C:/temp/list.pdf");
        vm.SelectedList = new PriceListSummaryDto(7, "قائمة أسعار الدم", 1);

        // AsyncRelayCommand.Execute is async void, so the fake's TaskCompletionSource is
        // awaited instead of sleeping — a deterministic wait for the port call.
        vm.PrintListCommand.Execute(null);
        await writer.Written.WaitAsync(TimeSpan.FromSeconds(10));

        Assert.Equal(1, writer.CallCount);
        Assert.Equal("C:/temp/list.pdf", writer.LastPath);
        Assert.NotNull(writer.LastList);
        Assert.Equal("قائمة أسعار الدم", writer.LastList!.Name);
    }

    [Fact]
    public async Task PriceListsViewModel_PrintCommand_DoesNothingWhenTheUserCancels()
    {
        var vm = Screen(out var writer, savePath: null);
        vm.SelectedList = new PriceListSummaryDto(7, "قائمة أسعار الدم", 1);

        vm.PrintListCommand.Execute(null);

        // Cancelling the save dialog must not reach the writer. The command returns
        // immediately, so yielding once is enough to observe the completed path.
        await Task.Yield();
        await Task.Delay(50);

        Assert.Equal(0, writer.CallCount);
    }

    [Fact]
    public void PriceLists_PrintCommand_IsReachableFromTheView()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !Directory.Exists(System.IO.Path.Combine(dir.FullName, "src", "TopLab.Presentation")))
        {
            dir = dir.Parent;
        }

        Assert.NotNull(dir);
        var xaml = File.ReadAllText(System.IO.Path.Combine(
            dir!.FullName, "src", "TopLab.Presentation", "Views", "Lab", "PriceListsView.xaml"));

        Assert.Contains("Command=\"{Binding PrintListCommand}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Content=\"طباعة\"", xaml, StringComparison.Ordinal);
        Assert.Contains("FlowDirection=\"RightToLeft\"", xaml, StringComparison.Ordinal);
    }
}