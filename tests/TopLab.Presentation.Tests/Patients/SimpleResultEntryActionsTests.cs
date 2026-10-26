using System.IO;
using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ReportProduction.Common;
using TopLab.Application.Features.ResultsEntry.Commands.ReviewResult;
using TopLab.Application.Features.ResultsEntry.Common;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Tests.Common;
using TopLab.Presentation.ViewModels.Patients;
using Xunit;

namespace TopLab.Presentation.Tests.Patients;

/// <summary>
/// R1 Round 4 / A4 — GAP-22 on the single-test result-entry screen.
/// Canonical reference 1.22.12.2 «طباعة», 1.22.12.3 «معاينة الطباعة», 1.22.12.4 «مراجعة».
/// <para>
/// The three actions reuse the SAME Application handlers the profile and culture screens
/// use: <see cref="IResultPrintCoordinator"/> + <see cref="IPrintedStateRecorder"/> for print,
/// <see cref="ISimpleResultPreviewBuilder"/> + <see cref="IPdfPreviewService"/> for preview,
/// and <see cref="ReviewResultCommand"/> for review. No second implementation exists.
/// </para>
/// </summary>
public class SimpleResultEntryActionsTests
{
    private sealed class RecordingSender : ISender
    {
        public List<int> ReviewedPatientTestIds { get; } = new();
        public Result? ReviewResult { get; set; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is ReviewResultCommand review)
            {
                ReviewedPatientTestIds.Add(review.PatientTestId);
                return Task.FromResult((TResponse)(object)(ReviewResult ?? Result.Success()));
            }

            throw new NotSupportedException($"RecordingSender has no canned response for {request.GetType().Name}.");
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest => throw new NotSupportedException();
        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private sealed class RecordingPrintCoordinator : IResultPrintCoordinator
    {
        public List<(int PatientTestId, ResultPrintKind Kind)> Calls { get; } = new();
        public bool Printed { get; set; } = true;
        public string? ErrorMessage { get; set; }

        public Task<ResultPrintOutcome> PrintAsync(int patientTestId, ResultPrintKind kind, CancellationToken ct = default)
        {
            Calls.Add((patientTestId, kind));
            return Task.FromResult(new ResultPrintOutcome(patientTestId, kind, Printed, ErrorMessage));
        }
    }

    private sealed class RecordingPrintedState : IPrintedStateRecorder
    {
        public List<int> RecordedPatientTestIds { get; } = new();

        public Task<Result> RecordAsync(
            IReadOnlyCollection<int> patientTestIds,
            IReadOnlyCollection<int> profileResultItemIds,
            CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success());

        public Task<Result> RecordForPatientTestAsync(int patientTestId, CancellationToken cancellationToken = default)
        {
            RecordedPatientTestIds.Add(patientTestId);
            return Task.FromResult(Result.Success());
        }
    }

    private sealed class RecordingPreviewBuilder : ISimpleResultPreviewBuilder
    {
        public List<int> Calls { get; } = new();
        public Result<string>? Result { get; set; }

        public Task<Result<string>> BuildAsync(int patientTestId, CancellationToken cancellationToken = default)
        {
            Calls.Add(patientTestId);
            return Task.FromResult(Result ?? TopLab.Application.Common.Results.Result<string>.Success("/tmp/preview.pdf"));
        }
    }

    private sealed class RecordingPdfPreview : IPdfPreviewService
    {
        public List<string> Paths { get; } = new();
        public Result<string>? Result { get; set; }

        public Task<Result<string>> PreviewAsync(string absolutePdfPath, CancellationToken cancellationToken = default)
        {
            Paths.Add(absolutePdfPath);
            return Task.FromResult(Result ?? TopLab.Application.Common.Results.Result<string>.Success(absolutePdfPath));
        }
    }

    private static ResultWorklistItemDto Item(int patientTestId = 42, bool reviewed = false) => new(
        PatientTestId: patientTestId,
        PatientId: 7,
        PatientFullName: "أحمد محمد علي",
        LabId: "LAB-1",
        TestName: "سكر صائم",
        TestCode: "FBS",
        ResultKind: 0,
        IsCultureType: false,
        IsSampleDrawn: true,
        IsTakenOutsideLab: false,
        ResultValue: "95",
        ResultFlag: null,
        IsReviewed: reviewed,
        IsPrinted: false,
        IsDelivered: false,
        AggregateStatus: 1,
        RegistrationDateUtc: new DateTime(2026, 10, 25, 9, 0, 0, DateTimeKind.Utc));

    private static (SimpleResultEntryViewModel Vm, RecordingSender Sender, RecordingPrintCoordinator Printer,
        RecordingPrintedState Printed, RecordingPreviewBuilder Builder, RecordingPdfPreview Pdf) Build()
    {
        var sender = new RecordingSender();
        var printer = new RecordingPrintCoordinator();
        var printed = new RecordingPrintedState();
        var builder = new RecordingPreviewBuilder();
        var pdf = new RecordingPdfPreview();
        var vm = new SimpleResultEntryViewModel(
            sender, new ResultErrorPresenter(), new FakeDialogService(), printer, printed, pdf, builder);
        return (vm, sender, printer, printed, builder, pdf);
    }

    private static async Task RunAsync(AsyncRelayCommand command)
    {
        command.Execute(null);
        await Task.Yield();
        await Task.Delay(80);
    }

    // ================================================================
    // Existence
    // ================================================================

    [Fact]
    public void AllThreeActionsExist()
    {
        var (vm, _, _, _, _, _) = Build();

        Assert.NotNull(vm.PrintCommand);
        Assert.NotNull(vm.PreviewCommand);
        Assert.NotNull(vm.ReviewCommand);
    }

    // ================================================================
    // Guard: no result loaded
    // ================================================================

    [Fact]
    public async Task Print_WithoutAResult_IsRefusedAndReachesNothing()
    {
        var (vm, _, printer, printed, _, _) = Build();

        await RunAsync(vm.PrintCommand);

        Assert.Equal("اختر نتيجة محفوظة أولًا قبل الطباعة.", vm.ErrorMessage);
        Assert.Empty(printer.Calls);
        Assert.Empty(printed.RecordedPatientTestIds);
    }

    [Fact]
    public async Task Preview_WithoutAResult_IsRefusedAndReachesNothing()
    {
        var (vm, _, _, _, builder, pdf) = Build();

        await RunAsync(vm.PreviewCommand);

        Assert.Equal("اختر نتيجة محفوظة أولًا قبل المعاينة.", vm.ErrorMessage);
        Assert.Empty(builder.Calls);
        Assert.Empty(pdf.Paths);
    }

    [Fact]
    public async Task Review_WithoutAResult_IsRefusedAndReachesNothing()
    {
        var (vm, sender, _, _, _, _) = Build();

        await RunAsync(vm.ReviewCommand);

        Assert.Equal("اختر نتيجة محفوظة أولًا قبل المراجعة.", vm.ErrorMessage);
        Assert.Empty(sender.ReviewedPatientTestIds);
    }

    // ================================================================
    // Print (1.22.12.2) — the shared coordinator path
    // ================================================================

    [Fact]
    public async Task Print_UsesTheSharedCoordinatorWithTheSimpleResultKind()
    {
        var (vm, _, printer, printed, _, _) = Build();
        await vm.LoadAsync(Item());

        await RunAsync(vm.PrintCommand);

        var call = Assert.Single(printer.Calls);
        Assert.Equal(42, call.PatientTestId);
        Assert.Equal(ResultPrintKind.SimpleResult, call.Kind);
        Assert.Equal(new[] { 42 }, printed.RecordedPatientTestIds);
        Assert.Equal("تمت الطباعة.", vm.StatusMessage);
    }

    [Fact]
    public async Task Print_WhenTheCoordinatorFails_SurfacesTheErrorAndRecordsNothing()
    {
        var (vm, _, printer, printed, _, _) = Build();
        printer.Printed = false;
        printer.ErrorMessage = "النتيجة غير معتمدة.";
        await vm.LoadAsync(Item());

        await RunAsync(vm.PrintCommand);

        Assert.Contains("تعذّرت الطباعة", vm.ErrorMessage, StringComparison.Ordinal);
        Assert.Empty(printed.RecordedPatientTestIds);
    }

    // ================================================================
    // Preview (1.22.12.3)
    // ================================================================

    [Fact]
    public async Task Preview_BuildsTheReportThenOpensIt()
    {
        var (vm, _, _, _, builder, pdf) = Build();
        await vm.LoadAsync(Item());

        await RunAsync(vm.PreviewCommand);

        Assert.Equal(new[] { 42 }, builder.Calls);
        Assert.Equal(new[] { "/tmp/preview.pdf" }, pdf.Paths);
        Assert.Equal("تم فتح المعاينة.", vm.StatusMessage);
    }

    [Fact]
    public async Task Preview_WhenTheBuildFails_SurfacesTheErrorAndOpensNothing()
    {
        var (vm, _, _, _, builder, pdf) = Build();
        builder.Result = TopLab.Application.Common.Results.Result<string>.Failure(
            TopLab.Application.Common.Results.Error.NotFound("التحليل غير موجود."));
        await vm.LoadAsync(Item());

        await RunAsync(vm.PreviewCommand);

        Assert.Equal("التحليل غير موجود.", vm.ErrorMessage);
        Assert.Empty(pdf.Paths);
    }

    // ================================================================
    // Review (1.22.12.4)
    // ================================================================

    [Fact]
    public async Task Review_DispatchesTheExistingReviewCommand()
    {
        var (vm, sender, _, _, _, _) = Build();
        await vm.LoadAsync(Item());

        await RunAsync(vm.ReviewCommand);

        Assert.Equal(new[] { 42 }, sender.ReviewedPatientTestIds);
        Assert.Equal("تم اعتماد النتيجة.", vm.StatusMessage);
    }

    [Fact]
    public async Task Review_WithAnEmptyResult_IsRefusedBeforeTheMediator()
    {
        var (vm, sender, _, _, _, _) = Build();
        var item = Item() with { ResultValue = null };
        await vm.LoadAsync(item);

        await RunAsync(vm.ReviewCommand);

        Assert.Equal("لا يمكن اعتماد نتيجة غير مدخلة.", vm.ErrorMessage);
        Assert.Empty(sender.ReviewedPatientTestIds);
    }

    [Fact]
    public async Task Review_SurfacesTheHandlersRefusal()
    {
        var (vm, sender, _, _, _, _) = Build();
        sender.ReviewResult = TopLab.Application.Common.Results.Result.Failure(
            TopLab.Application.Common.Results.Error.Conflict("النتيجة معتمدة؛ ألغِ الاعتماد أولاً."));
        await vm.LoadAsync(Item());

        await RunAsync(vm.ReviewCommand);

        Assert.Contains("معتمدة", vm.ErrorMessage, StringComparison.Ordinal);
    }

    // ================================================================
    // XAML wiring
    // ================================================================

    [Fact]
    public void View_BindsTheThreeActionsInTheActionBand()
    {
        var xaml = ReadPresentationFile(Path.Combine("Views", "Patients", "SimpleResultEntryView.xaml"));

        Assert.Contains("Command=\"{Binding PrintCommand}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Command=\"{Binding PreviewCommand}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Command=\"{Binding ReviewCommand}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("FlowDirection=\"RightToLeft\"", xaml, StringComparison.Ordinal);
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
