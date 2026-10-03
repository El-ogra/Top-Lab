using System.IO;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Common;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Queries.GetCustomGroupById;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Queries.GetCustomGroups;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.ViewModels.Lab;
using MediatR;
using Xunit;

namespace TopLab.Presentation.Tests.Lab;

/// <summary>
/// P-01 F9 (PP-03) — the custom test-group print command.
///
/// The writer is faked (hand-rolled, no mocking library) so this proves the VIEWMODEL
/// wiring only; the real PDF is proven by <c>CustomGroupPdfWriterTests</c>.
/// </summary>
public class CustomGroupsPrintCommandTests
{
    private sealed class RecordingCustomGroupPdfWriter : ICustomGroupPdfWriter
    {
        private readonly TaskCompletionSource _written = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public int CallCount { get; private set; }
        public string? LastPath { get; private set; }
        public CustomGroupDetailDto? LastGroup { get; private set; }
        public Task Written => _written.Task;

        public Task WritePdfAsync(
            string absolutePath,
            CustomGroupDetailDto group,
            LabPrintTextDto labText,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            LastPath = absolutePath;
            LastGroup = group;
            _written.TrySetResult();
            return Task.CompletedTask;
        }
    }

    private sealed class CustomGroupSender : ISender
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is GetCustomGroupByIdQuery)
            {
                var detail = new CustomGroupDetailDto(3, "باقة الفحص الشامل", new[]
                {
                    new CustomGroupItemDto(201, "تحليل دهون", "LIPID", 300m)
                });
                return Task.FromResult((TResponse)(object)Result<CustomGroupDetailDto>.Success(detail));
            }

            if (request is GetCustomGroupsQuery)
            {
                return Task.FromResult((TResponse)(object)Result<IReadOnlyList<CustomGroupSummaryDto>>
                    .Success(Array.Empty<CustomGroupSummaryDto>()));
            }

            throw new NotSupportedException($"CustomGroupSender has no canned response for {request.GetType().Name}.");
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

    private sealed class FakeLabPrintText : ILabPrintTextStore
    {
        public Task<Result<LabPrintTextDto>> GetAsync(LabPrintTextScope scope, CancellationToken cancellationToken = default)
            => Task.FromResult(Result<LabPrintTextDto>.Success(
                new LabPrintTextDto("معمل النور", "القاهرة", "0100000000", "Arial", 11)));

        public Task<Result> SaveAsync(LabPrintTextScope scope, LabPrintTextDto content, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success());
    }

    private static CustomGroupsViewModel Screen(out RecordingCustomGroupPdfWriter writer, string? savePath = "C:/temp/group.pdf")
    {
        writer = new RecordingCustomGroupPdfWriter();
        return new CustomGroupsViewModel(
            new CustomGroupSender(),
            new FixedDialogService(savePath),
            new ResultErrorPresenter(),
            writer,
            new FakeLabPrintText());
    }

    [Fact]
    public void CustomGroupsViewModel_PrintCommand_DisabledWithNoSelection()
    {
        var vm = Screen(out _);

        Assert.NotNull(vm.PrintGroupCommand);
        Assert.False(vm.PrintGroupCommand.CanExecute(null));
    }

    [Fact]
    public void CustomGroupsViewModel_PrintCommand_EnabledWithSelection()
    {
        var vm = Screen(out _);
        vm.SelectedGroup = new CustomGroupSummaryDto(3, "باقة الفحص الشامل", 1);

        Assert.True(vm.PrintGroupCommand.CanExecute(null));
    }

    [Fact]
    public void CustomGroupsViewModel_PrintCommand_DisabledAgainWhenSelectionCleared()
    {
        var vm = Screen(out _);
        vm.SelectedGroup = new CustomGroupSummaryDto(3, "باقة الفحص الشامل", 1);
        Assert.True(vm.PrintGroupCommand.CanExecute(null));

        vm.SelectedGroup = null;
        Assert.False(vm.PrintGroupCommand.CanExecute(null));
    }

    [Fact]
    public async Task CustomGroupsViewModel_PrintCommand_WritesThroughThePort()
    {
        var vm = Screen(out var writer, "C:/temp/group.pdf");
        vm.SelectedGroup = new CustomGroupSummaryDto(3, "باقة الفحص الشامل", 1);

        vm.PrintGroupCommand.Execute(null);
        await writer.Written.WaitAsync(TimeSpan.FromSeconds(10));

        Assert.Equal(1, writer.CallCount);
        Assert.Equal("C:/temp/group.pdf", writer.LastPath);
        Assert.NotNull(writer.LastGroup);
        Assert.Equal("باقة الفحص الشامل", writer.LastGroup!.Name);
    }

    [Fact]
    public async Task CustomGroupsViewModel_PrintCommand_DoesNothingWhenTheUserCancels()
    {
        var vm = Screen(out var writer, savePath: null);
        vm.SelectedGroup = new CustomGroupSummaryDto(3, "باقة الفحص الشامل", 1);

        vm.PrintGroupCommand.Execute(null);

        await Task.Yield();
        await Task.Delay(50);

        Assert.Equal(0, writer.CallCount);
    }

    [Fact]
    public void CustomGroups_PrintCommand_IsReachableFromTheView()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "src", "TopLab.Presentation")))
        {
            dir = dir.Parent;
        }

        Assert.NotNull(dir);
        var xaml = File.ReadAllText(Path.Combine(
            dir!.FullName, "src", "TopLab.Presentation", "Views", "Lab", "CustomGroupsView.xaml"));

        Assert.Contains("Command=\"{Binding PrintGroupCommand}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Content=\"طباعة\"", xaml, StringComparison.Ordinal);
        Assert.Contains("FlowDirection=\"RightToLeft\"", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void CustomGroups_UsesItsOwnPort_NotThePriceListPort()
    {
        // C-6: the two ViewModels must not reach each other's writer.
        var groupConstructor = typeof(CustomGroupsViewModel).GetConstructors().Single()
            .GetParameters()
            .Select(p => p.ParameterType)
            .ToArray();

        Assert.Contains(typeof(ICustomGroupPdfWriter), groupConstructor);
        Assert.DoesNotContain(typeof(IPriceListPdfWriter), groupConstructor);

        var priceConstructor = typeof(PriceListsViewModel).GetConstructors().Single()
            .GetParameters()
            .Select(p => p.ParameterType)
            .ToArray();

        Assert.Contains(typeof(IPriceListPdfWriter), priceConstructor);
        Assert.DoesNotContain(typeof(ICustomGroupPdfWriter), priceConstructor);
    }
}