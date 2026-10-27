using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.CultureAndAntibiotics.Common;
using TopLab.Application.Features.CultureAndAntibiotics.Queries.GetAntibiotics;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.ViewModels.Lab;
using Xunit;

namespace TopLab.Presentation.Tests.Lab;

/// <summary>
/// GAP-25 (3.9.1.6.5) — the antibiotic-catalogue print command.
///
/// The writer is faked here so this test proves the VIEWMODEL wiring only; the
/// real PDF is proven by <c>AntibioticListPdfWriterTests</c> in the Infrastructure
/// project. An empty catalogue disables the action, the printed rows are the rows
/// on screen (including any active search filter), and a failed or cancelled
/// save never reports success.
/// </summary>
public class AntibioticsPrintCommandTests
{
    private static readonly AntibioticDto Amoxicillin =
        new(1, "Amoxicillin", true, false, "AMX", "Amoxicillin trihydrate", "Amoxil");

    private static readonly AntibioticDto Ciprofloxacin =
        new(2, "Ciprofloxacin", false, true, "CIP", "Ciprofloxacin hydrochloride", "Cipro");

    private static readonly AntibioticDto Penicillin =
        new(3, "Penicillin", false, false, "PEN", "Penicillin G", "Pen-V");

    private sealed class RecordingAntibioticListPdfWriter : IAntibioticListPdfWriter
    {
        private readonly TaskCompletionSource _finished = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public RecordingAntibioticListPdfWriter(bool throwIOException = false)
        {
            ThrowIOException = throwIOException;
        }

        public bool ThrowIOException { get; }
        public int CallCount { get; private set; }
        public string? LastPath { get; private set; }
        public IReadOnlyList<AntibioticDto>? LastAntibiotics { get; private set; }
        public LabPrintTextDto? LastLabText { get; private set; }

        /// <summary>Completes once the command has actually reached the port.</summary>
        public Task Finished => _finished.Task;

        public Task WritePdfAsync(
            string absolutePath,
            IReadOnlyList<AntibioticDto> antibiotics,
            LabPrintTextDto labText,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            LastPath = absolutePath;
            LastAntibiotics = antibiotics;
            LastLabText = labText;

            try
            {
                if (ThrowIOException)
                {
                    throw new IOException("target already exists");
                }

                return Task.CompletedTask;
            }
            finally
            {
                _finished.TrySetResult();
            }
        }
    }

    private sealed class AntibioticSender : ISender
    {
        private readonly IReadOnlyList<AntibioticDto> _catalogue;

        public AntibioticSender(params AntibioticDto[] catalogue) => _catalogue = catalogue;

        public List<GetAntibioticsQuery> Queries { get; } = new();

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is GetAntibioticsQuery query)
            {
                Queries.Add(query);
                IReadOnlyList<AntibioticDto> rows = string.IsNullOrWhiteSpace(query.SearchTerm)
                    ? _catalogue
                    : _catalogue
                        .Where(a => a.Name.Contains(query.SearchTerm.Trim(), StringComparison.OrdinalIgnoreCase))
                        .ToList();

                return Task.FromResult(
                    (TResponse)(object)Result<IReadOnlyList<AntibioticDto>>.Success(rows));
            }

            throw new NotSupportedException(
                $"AntibioticSender has no canned response for {request.GetType().Name}.");
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

        public List<string?> SuggestedNames { get; } = new();

        public Task<bool> ShowConfirmationAsync(string title, string message) => Task.FromResult(true);

        public Task ShowErrorAsync(string message) => Task.CompletedTask;

        public Task<bool> ShowSecondaryPasswordDialogAsync() => Task.FromResult(true);

        public Task<string?> PickBackupFolderAsync(string initialDirectory) => Task.FromResult<string?>(null);

        public Task<string?> PickBackupFileAsync() => Task.FromResult<string?>(null);

        public Task<string?> PickPdfSavePathAsync(string? suggestedFileName = null)
        {
            SuggestedNames.Add(suggestedFileName);
            return Task.FromResult(_path);
        }
    }

    private sealed class FakeLabPrintText : ILabPrintTextStore
    {
        private readonly bool _fail;

        public FakeLabPrintText(bool fail = false) => _fail = fail;

        public Task<Result<LabPrintTextDto>> GetAsync(
            LabPrintTextScope scope, CancellationToken cancellationToken = default)
            => Task.FromResult(_fail
                ? Result<LabPrintTextDto>.Failure(Error.Unexpected("lab text unavailable"))
                : Result<LabPrintTextDto>.Success(
                    new LabPrintTextDto("معمل النور", "القاهرة", "0100000000", "Arial", 11)));

        public Task<Result> SaveAsync(
            LabPrintTextScope scope, LabPrintTextDto content, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success());
    }

    private sealed class NullServices : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private static AntibioticsViewModel Screen(
        AntibioticSender sender,
        out RecordingAntibioticListPdfWriter writer,
        FixedDialogService? dialogs = null,
        string? savePath = "C:/temp/antibiotics.pdf",
        bool throwIOException = false,
        bool labTextFails = false)
    {
        writer = new RecordingAntibioticListPdfWriter(throwIOException);
        dialogs ??= new FixedDialogService(savePath);
        return new AntibioticsViewModel(
            sender,
            dialogs,
            new ResultErrorPresenter(),
            writer,
            new FakeLabPrintText(labTextFails),
            new NullServices());
    }

    private static async Task WaitUntilAsync(Func<bool> condition, string because)
    {
        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (!condition())
        {
            if (DateTime.UtcNow > deadline)
            {
                throw new TimeoutException($"Timed out waiting for: {because}");
            }

            await Task.Delay(10);
        }
    }

    // =====================================================================
    // Availability
    // =====================================================================

    [Fact]
    public void PrintCommand_IsDisabledWhenTheCatalogueIsEmpty()
    {
        var vm = Screen(new AntibioticSender(), out _);

        Assert.False(vm.PrintListCommand.CanExecute(null));
    }

    [Fact]
    public async Task PrintCommand_IsEnabledWhenTheCatalogueHasRows()
    {
        var vm = Screen(new AntibioticSender(Amoxicillin, Ciprofloxacin), out _);

        await vm.LoadAsync();

        Assert.True(vm.PrintListCommand.CanExecute(null));
    }

    [Fact]
    public async Task PrintCommand_IsDisabledWhenTheSearchReturnsNothing()
    {
        var vm = Screen(new AntibioticSender(Amoxicillin), out _);
        vm.SearchTerm = "Ciprofloxacin";

        await vm.LoadAsync();

        Assert.Empty(vm.Antibiotics);
        Assert.False(vm.PrintListCommand.CanExecute(null));
    }

    /// <summary>
    /// GAP-25-AUDIT-02: <c>Antibiotics</c> is replacement-based (<c>LoadAsync</c>
    /// assigns a new collection), so command availability must be refreshed when
    /// the property is replaced — including on the enabled-to-disabled transition.
    /// The existing tests only cover the initial empty and initial non-empty loads.
    /// This test also asserts that the <c>CanExecuteChanged</c> event is actually
    /// raised, which is what a WPF button listens to.
    /// </summary>
    [Fact]
    public async Task PrintCommand_BecomesDisabledWhenAReloadReplacesTheCatalogueWithAnEmptyOne()
    {
        var vm = Screen(new AntibioticSender(Amoxicillin, Ciprofloxacin), out _);
        await vm.LoadAsync();
        Assert.True(vm.PrintListCommand.CanExecute(null));

        var canExecuteNotifications = 0;
        vm.PrintListCommand.CanExecuteChanged += (_, _) => canExecuteNotifications++;

        vm.SearchTerm = "Cipro";
        await vm.LoadAsync();
        Assert.Single(vm.Antibiotics);
        Assert.True(vm.PrintListCommand.CanExecute(null));

        vm.SearchTerm = "no-such-antibiotic";
        await vm.LoadAsync();

        Assert.Empty(vm.Antibiotics);
        Assert.False(vm.PrintListCommand.CanExecute(null));
        Assert.True(canExecuteNotifications >= 2,
            "replacing the catalogue must raise CanExecuteChanged for the print command");
    }

    // =====================================================================
    // End-to-end wiring through the port
    // =====================================================================

    [Fact]
    public async Task PrintCommand_WritesTheOnScreenCatalogueThroughThePort()
    {
        var sender = new AntibioticSender(Amoxicillin, Ciprofloxacin, Penicillin);
        var vm = Screen(sender, out var writer);
        await vm.LoadAsync();

        vm.PrintListCommand.Execute(null);
        await writer.Finished.WaitAsync(TimeSpan.FromSeconds(10));
        await WaitUntilAsync(() => vm.StatusMessage.Length > 0, "the success message");

        Assert.Equal(1, writer.CallCount);
        Assert.Equal("C:/temp/antibiotics.pdf", writer.LastPath);
        Assert.NotNull(writer.LastAntibiotics);
        Assert.Equal(new[] { "Amoxicillin", "Ciprofloxacin", "Penicillin" },
            writer.LastAntibiotics!.Select(a => a.Name));
        Assert.Equal(new[] { "Amoxil", "Cipro", "Pen-V" },
            writer.LastAntibiotics!.Select(a => a.CommercialName));
        Assert.Equal("معمل النور", writer.LastLabText!.LabName);
        Assert.Equal(string.Empty, vm.ErrorMessage);
        Assert.Equal("تم إنشاء ملف قائمة المضادات.", vm.StatusMessage);

        // Printing is read-only: the only request was the catalogue load.
        Assert.Single(sender.Queries);
        Assert.All(sender.Queries, q => Assert.Null(q.SearchTerm));
    }

    [Fact]
    public async Task PrintCommand_PrintsTheFilteredRowsThatMatchTheScreen()
    {
        var sender = new AntibioticSender(Amoxicillin, Ciprofloxacin, Penicillin);
        var vm = Screen(sender, out var writer);
        vm.SearchTerm = "Amox";
        await vm.LoadAsync();

        vm.PrintListCommand.Execute(null);
        await writer.Finished.WaitAsync(TimeSpan.FromSeconds(10));
        await WaitUntilAsync(() => vm.StatusMessage.Length > 0, "the success message");

        Assert.Equal("Amox", Assert.Single(sender.Queries).SearchTerm);
        Assert.Equal(new[] { "Amoxicillin" }, writer.LastAntibiotics!.Select(a => a.Name));
        Assert.Equal("Amoxil", Assert.Single(writer.LastAntibiotics!).CommercialName);
    }

    [Fact]
    public async Task PrintCommand_SuggestsAnArabicPdfFileName()
    {
        var dialogs = new FixedDialogService("C:/temp/antibiotics.pdf");
        var vm = Screen(new AntibioticSender(Amoxicillin), out var writer, dialogs);
        await vm.LoadAsync();

        vm.PrintListCommand.Execute(null);
        await writer.Finished.WaitAsync(TimeSpan.FromSeconds(10));

        Assert.Equal("قائمة المضادات.pdf", Assert.Single(dialogs.SuggestedNames));
    }

    // =====================================================================
    // Cancellation and failure paths
    // =====================================================================

    [Fact]
    public async Task PrintCommand_DoesNothingWhenTheUserCancelsTheSaveDialog()
    {
        var vm = Screen(new AntibioticSender(Amoxicillin), out var writer, savePath: null);
        await vm.LoadAsync();

        vm.PrintListCommand.Execute(null);
        await Task.Delay(100);

        Assert.Equal(0, writer.CallCount);
        Assert.Equal(string.Empty, vm.StatusMessage);
        Assert.Equal(string.Empty, vm.ErrorMessage);
    }

    [Fact]
    public async Task PrintCommand_WriterFailureDoesNotReportSuccess()
    {
        var vm = Screen(new AntibioticSender(Amoxicillin), out var writer, throwIOException: true);
        await vm.LoadAsync();

        vm.PrintListCommand.Execute(null);
        await writer.Finished.WaitAsync(TimeSpan.FromSeconds(10));
        await WaitUntilAsync(() => vm.ErrorMessage.Length > 0, "the error message");

        Assert.Equal(1, writer.CallCount);
        Assert.Equal("الملف موجود مسبقًا؛ لم يتم الكتابة فوقه.", vm.ErrorMessage);
        Assert.Equal(string.Empty, vm.StatusMessage);
    }

    [Fact]
    public async Task PrintCommand_LabTextFailureDoesNotReachTheWriter()
    {
        var vm = Screen(new AntibioticSender(Amoxicillin), out var writer, labTextFails: true);
        await vm.LoadAsync();

        vm.PrintListCommand.Execute(null);
        await WaitUntilAsync(() => vm.ErrorMessage.Length > 0, "the error message");

        Assert.Equal(0, writer.CallCount);
        Assert.Equal(string.Empty, vm.StatusMessage);
        Assert.NotEqual(string.Empty, vm.ErrorMessage);
    }

    // =====================================================================
    // Reachability
    // =====================================================================

    [Fact]
    public void PrintCommand_IsReachableFromTheAntibioticsView()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "src", "TopLab.Presentation")))
        {
            dir = dir.Parent;
        }

        Assert.NotNull(dir);
        var xaml = File.ReadAllText(Path.Combine(
            dir!.FullName, "src", "TopLab.Presentation", "Views", "Lab", "AntibioticsView.xaml"));

        Assert.Contains("Command=\"{Binding PrintListCommand}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Content=\"طباعة\"", xaml, StringComparison.Ordinal);
        Assert.Contains("FlowDirection=\"RightToLeft\"", xaml, StringComparison.Ordinal);
    }
}
