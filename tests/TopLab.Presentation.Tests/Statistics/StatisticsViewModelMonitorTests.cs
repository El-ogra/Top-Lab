using System.Globalization;
using System.IO;
using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ExternalEntities.Common;
using TopLab.Application.Features.ExternalEntities.Queries.SearchExternalEntities;
using TopLab.Application.Features.Statistics.Common;
using TopLab.Application.Features.Statistics.Queries.GetBandedResultMonitor;
using TopLab.Application.Features.Statistics.Queries.GetPatientCountStatistics;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Application.Features.UsersAndPermissions.Common;
using TopLab.Application.Features.UsersAndPermissions.Queries.GetUsers;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Common;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Queries.GetTestGroups;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Queries.SearchTestCatalog;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Common.Navigation;
using TopLab.Presentation.ViewModels.Statistics;
using Xunit;

namespace TopLab.Presentation.Tests.Statistics;

/// <summary>
/// R-F05 (VG-03) — the banded-monitor section of <see cref="StatisticsViewModel"/>.
///
/// The writer is faked (hand-rolled, no mocking library) so this proves the VIEWMODEL
/// wiring only; the real PDF is proven by <c>BandedResultMonitorPdfWriterTests</c> in the
/// Infrastructure project. <c>Common/Fakes.cs</c> is deliberately untouched (SD-14).
///
/// The binding rule checked here is OD-4b / BR-F05-17: a comma in min or max is rejected
/// with a clear Arabic message and the mediator is NEVER called, because both
/// <c>NumberStyles.Any</c> and <c>NumberStyles.Number</c> would silently read "3,5" as 35.
/// </summary>
public class StatisticsViewModelMonitorTests
{
    private sealed class RecordingMonitorPdfWriter : IBandedResultMonitorPdfWriter
    {
        private readonly TaskCompletionSource _written = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public int CallCount { get; private set; }
        public string? LastPath { get; private set; }
        public BandedResultMonitorDto? LastMonitor { get; private set; }

        public Task Written => _written.Task;

        public Task WritePdfAsync(
            string absolutePath,
            BandedResultMonitorDto monitor,
            LabPrintTextDto labText,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            LastPath = absolutePath;
            LastMonitor = monitor;
            _written.TrySetResult();
            return Task.CompletedTask;
        }
    }

    private sealed class MonitorSender : ISender
    {
        public int BandQueryCount { get; private set; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is SearchTestCatalogQuery)
            {
                var catalog = new[]
                {
                    Test(10, "FBS", "سكر صائم"),
                    Test(11, "CBC", "صورة دم كاملة")
                };
                return Task.FromResult((TResponse)(object)Result<IReadOnlyList<TestSummaryDto>>.Success(catalog));
            }

            if (request is GetPatientCountStatisticsQuery)
            {
                return Task.FromResult((TResponse)(object)Result<PatientCountStatisticsDto>
                    .Success(EmptyPatientStats()));
            }

            // The pre-existing LoadFilterItemsAsync also loads sections 2–4; these stubs keep
            // that path working so the monitor assertions are not masked by it.
            if (request is GetTestGroupsQuery)
            {
                return Task.FromResult((TResponse)(object)Result<IReadOnlyList<TestGroupDto>>
                    .Success(Array.Empty<TestGroupDto>()));
            }

            if (request is SearchExternalEntitiesQuery)
            {
                return Task.FromResult((TResponse)(object)Result<IReadOnlyList<ExternalEntityListItemDto>>
                    .Success(Array.Empty<ExternalEntityListItemDto>()));
            }

            if (request is GetUsersQuery)
            {
                return Task.FromResult((TResponse)(object)Result<IReadOnlyList<UserSummaryDto>>
                    .Success(Array.Empty<UserSummaryDto>()));
            }

            if (request is GetBandedResultMonitorQuery band)
            {
                BandQueryCount++;
                var rows = new List<BandedResultRowDto>
                {
                    new(1, new DateTime(2026, 3, 5, 12, 0, 0, DateTimeKind.Utc), 42, "أحمد محمد", "Male", 30, "Year", "بدون جهة إحالة", "سكر صائم", band.MinValue.ToString(CultureInfo.InvariantCulture), "معتمد")
                };
                var dto = new BandedResultMonitorDto(
                    band.From ?? new DateOnly(2026, 3, 1),
                    band.To ?? new DateOnly(2026, 3, 31),
                    band.TestId,
                    "سكر صائم",
                    band.MinValue,
                    band.MaxValue,
                    rows,
                    rows.Count);
                return Task.FromResult((TResponse)(object)Result<BandedResultMonitorDto>.Success(dto));
            }

            throw new NotSupportedException($"MonitorSender has no canned response for {request.GetType().Name}.");
        }

        private static TestSummaryDto Test(int id, string code, string name) => new(
            Id: id,
            TestCode: code,
            Name: name,
            ReportName: name,
            TestGroupId: null,
            TestGroupName: null,
            Barcode: null,
            CompletionDurationMinutes: 60,
            IsSentOut: false,
            PatientPrice: 100m,
            LabToLabPrice: null,
            IsActive: true);

        private static PatientCountStatisticsDto EmptyPatientStats() => new(
            new DateOnly(2026, 3, 1),
            new DateOnly(2026, 3, 31),
            0,
            Array.Empty<ClassificationCountDto>(),
            Array.Empty<ClassificationCountDto>(),
            Array.Empty<ClassificationCountDto>(),
            Array.Empty<MonthlyCountDto>(),
            Array.Empty<MonthlyClassificationCountDto>(),
            Array.Empty<DayOfMonthCountDto>(),
            null);

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

    private sealed class NoOpNavigation : INavigationService
    {
        public ViewModelBase? CurrentViewModel => null;

        public event Action<ViewModelBase?>? Navigated;

        public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
            => Navigated?.Invoke(null);

        public void NavigateTo(ViewModelBase viewModel)
            => Navigated?.Invoke(viewModel);
    }

    private static StatisticsViewModel Screen(
        out RecordingMonitorPdfWriter writer,
        out MonitorSender sender,
        string? savePath = "C:/temp/band.pdf")
    {
        writer = new RecordingMonitorPdfWriter();
        sender = new MonitorSender();
        return new StatisticsViewModel(
            sender,
            new ResultErrorPresenter(),
            new NoOpNavigation(),
            new FixedDialogService(savePath),
            writer,
            new FakeLabPrintText());
    }

    private static async Task<(StatisticsViewModel Vm, RecordingMonitorPdfWriter Writer, MonitorSender Sender)> LoadedAsync(
        string? savePath = "C:/temp/band.pdf")
    {
        var vm = Screen(out var writer, out var sender, savePath);
        await vm.LoadAsync();
        return (vm, writer, sender);
    }

    /// <summary>
    /// <c>AsyncRelayCommand.Execute</c> is <c>async void</c> (RelayCommand.cs:51), so there is
    /// nothing to await. This runs the command and then yields until it has finished — the
    /// same deterministic pattern as <c>PriceListsPrintCommandTests</c>.
    /// </summary>
    private static async Task RunAsync(AsyncRelayCommand command, Task? reached = null)
    {
        command.Execute(null);

        if (reached is not null)
        {
            await reached.WaitAsync(TimeSpan.FromSeconds(10));
            return;
        }

        await Task.Yield();
        await Task.Delay(50);
    }

    [Fact]
    public async Task Picker_PopulatesFromSearchTestCatalogQuery()
    {
        var (vm, _, _) = await LoadedAsync();

        Assert.Equal(2, vm.MonitorTestItems.Count);
        Assert.Equal("سكر صائم", vm.MonitorTestItems[0].Name);
        Assert.Equal(10, vm.MonitorTestItems[0].Id);
    }

    [Fact]
    public async Task Min_WithADot_ParsesAndReachesTheQuery()
    {
        var (vm, _, sender) = await LoadedAsync();
        vm.MonitorTest = vm.MonitorTestItems[0];
        vm.MonitorMinInput = "3.5";
        vm.MonitorMaxInput = "7";

        await RunAsync(vm.LoadBandCommand);

        Assert.Equal(1, sender.BandQueryCount);
        Assert.Equal(3.5m, vm.BandStats!.MinValue);
        Assert.Equal(7m, vm.BandStats.MaxValue);
        Assert.Equal(string.Empty, vm.ErrorMessage);
    }

    [Fact]
    public async Task Max_WithADot_ParsesCorrectly()
    {
        var (vm, _, _) = await LoadedAsync();
        vm.MonitorTest = vm.MonitorTestItems[0];
        vm.MonitorMinInput = "0";
        vm.MonitorMaxInput = "12.75";

        await RunAsync(vm.LoadBandCommand);

        Assert.Equal(12.75m, vm.BandStats!.MaxValue);
        Assert.Equal(string.Empty, vm.ErrorMessage);
    }

    // =====================================================================
    // BR-F05-17 / SD-9 — the binding rule. A comma must never be read as 35.
    // =====================================================================

    [Fact]
    public async Task Min_WithAComma_IsRejected_AndTheMediatorIsNotCalled()
    {
        var (vm, _, sender) = await LoadedAsync();
        vm.MonitorTest = vm.MonitorTestItems[0];
        vm.MonitorMinInput = "3,5";
        vm.MonitorMaxInput = "7";

        await RunAsync(vm.LoadBandCommand);

        Assert.Equal(0, sender.BandQueryCount);
        Assert.Null(vm.BandStats);
        Assert.Equal("استخدم النقطة (.) للفاصلة العشرية، والفاصلة (,) غير مقبولة.", vm.ErrorMessage);
    }

    [Fact]
    public async Task Max_WithAComma_IsRejected_AndTheMediatorIsNotCalled()
    {
        var (vm, _, sender) = await LoadedAsync();
        vm.MonitorTest = vm.MonitorTestItems[0];
        vm.MonitorMinInput = "3.5";
        vm.MonitorMaxInput = "7,25";

        await RunAsync(vm.LoadBandCommand);

        Assert.Equal(0, sender.BandQueryCount);
        Assert.Null(vm.BandStats);
        Assert.Equal("استخدم النقطة (.) للفاصلة العشرية، والفاصلة (,) غير مقبولة.", vm.ErrorMessage);
    }

    [Fact]
    public async Task NonNumericMin_SetsErrorMessage_AndTheMediatorIsNotCalled()
    {
        var (vm, _, sender) = await LoadedAsync();
        vm.MonitorTest = vm.MonitorTestItems[0];
        vm.MonitorMinInput = "abc";
        vm.MonitorMaxInput = "7";

        await RunAsync(vm.LoadBandCommand);

        Assert.Equal(0, sender.BandQueryCount);
        Assert.Null(vm.BandStats);
        Assert.Equal("أدخل قيمة رقمية صحيحة.", vm.ErrorMessage);
    }

    [Fact]
    public async Task InvertedBand_IsRejectedWithTheFrozenValidatorMessage()
    {
        var (vm, _, sender) = await LoadedAsync();
        vm.MonitorTest = vm.MonitorTestItems[0];
        vm.MonitorMinInput = "7";
        vm.MonitorMaxInput = "3";

        await RunAsync(vm.LoadBandCommand);

        Assert.Equal(0, sender.BandQueryCount);
        Assert.Equal("الحد الأدنى يجب ألا يتجاوز الحد الأقصى.", vm.ErrorMessage);
    }

    [Fact]
    public async Task NoTestSelected_IsRejected_AndTheMediatorIsNotCalled()
    {
        var (vm, _, sender) = await LoadedAsync();
        vm.MonitorMinInput = "3";
        vm.MonitorMaxInput = "7";

        await RunAsync(vm.LoadBandCommand);

        Assert.Equal(0, sender.BandQueryCount);
        Assert.Equal("اختر التحليل.", vm.ErrorMessage);
    }

    // =====================================================================
    // Print
    // =====================================================================

    [Fact]
    public async Task PrintCommand_ShortCircuitsWhenNoResultsAreLoaded()
    {
        var (vm, writer, _) = await LoadedAsync();

        await RunAsync(vm.PrintBandCommand);
        await Task.Delay(50);

        Assert.Equal(0, writer.CallCount);
        Assert.Equal("اعرض النتائج أولًا قبل الطباعة.", vm.ErrorMessage);
    }

    [Fact]
    public async Task PrintCommand_DoesNothingWhenTheUserCancels()
    {
        var (vm, writer, _) = await LoadedAsync(savePath: null);
        vm.MonitorTest = vm.MonitorTestItems[0];
        vm.MonitorMinInput = "3";
        vm.MonitorMaxInput = "7";
        await RunAsync(vm.LoadBandCommand);

        await RunAsync(vm.PrintBandCommand);

        Assert.Equal(0, writer.CallCount);
    }

    [Fact]
    public async Task PrintCommand_WritesExactlyOnce_WithTheLoadedBandStats()
    {
        var (vm, writer, _) = await LoadedAsync();
        vm.MonitorTest = vm.MonitorTestItems[0];
        vm.MonitorMinInput = "3.5";
        vm.MonitorMaxInput = "7";
        await RunAsync(vm.LoadBandCommand);
        var loaded = vm.BandStats!;

        await RunAsync(vm.PrintBandCommand);

        Assert.Equal(1, writer.CallCount);
        Assert.Equal("C:/temp/band.pdf", writer.LastPath);
        Assert.NotNull(writer.LastMonitor);
        Assert.Equal(loaded.TotalCount, writer.LastMonitor!.TotalCount);
        Assert.Equal(3.5m, writer.LastMonitor.MinValue);
        Assert.Equal("تم إنشاء ملف نطاق النتائج.", vm.StatusMessage);
    }

    // =====================================================================
    // Structural gates — no shipped test checks these two, so the agent must
    // =====================================================================

    [Fact]
    public void ViewModel_HasNoInfrastructureReference()
    {
        // PresentationLayeringTests.PresentationLayering_NoInfrastructureReferenceOutsideAppXaml
        // asserts an EXACT three-file offender list; a fourth entry fails a shipped test.
        var source = ReadPresentationFile(Path.Combine("ViewModels", "Statistics", "StatisticsViewModel.cs"));

        Assert.DoesNotContain("TopLab.Infrastructure", source, StringComparison.Ordinal);
    }

    [Fact]
    public void View_StaysRightToLeft_AndBindsOnlyRealProperties()
    {
        var xaml = ReadPresentationFile(Path.Combine("Views", "Statistics", "StatisticsView.xaml"));

        // PresentationStructuralTests.EveryView_IsRightToLeft scans every Views/**/*.xaml.
        Assert.Contains("FlowDirection=\"RightToLeft\"", xaml, StringComparison.Ordinal);

        // Every binding this slice added must name a real public property — WPF would
        // otherwise silently bind nothing and no shipped test would catch it.
        foreach (var name in new[]
        {
            "IsMonitorSection", "MonitorTestItems", "MonitorTest", "MonitorMinInput",
            "MonitorMaxInput", "LoadBandCommand", "PrintBandCommand", "BandStats",
            "ShowBandEmpty", "StatusMessage", "EnteredAtUtc", "PatientFullName",
            "PatientId", "PatientSex", "PatientAgeValue", "ReferralEntityName",
            "TestName", "ResultValue", "StatusText"
        })
        {
            Assert.Contains(name, xaml, StringComparison.Ordinal);
        }
    }

    private static string ReadPresentationFile(string relativePath)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "src", "TopLab.Presentation")))
        {
            dir = dir.Parent;
        }

        Assert.NotNull(dir);
        return File.ReadAllText(Path.Combine(dir!.FullName, "src", "TopLab.Presentation", relativePath));
    }
}