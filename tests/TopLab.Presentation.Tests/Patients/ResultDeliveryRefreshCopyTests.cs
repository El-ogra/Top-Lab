using System.IO;
using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ResultDelivery.Common;
using TopLab.Application.Features.ResultDelivery.Queries.GetUndeliveredResults;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Common.Navigation;
using TopLab.Presentation.Tests.Common;
using TopLab.Presentation.ViewModels.Patients;
using Xunit;

namespace TopLab.Presentation.Tests.Patients;

/// <summary>
/// R1 Round 4 / A3: the delivery screen's refresh (1.4.1.4.2) and copy (1.4.1.4.5) actions.
/// Copy goes through <c>IClipboardService</c> — the ViewModel never touches
/// <c>System.Windows.Clipboard</c>, so the exact copied text is asserted with a fake.
/// </summary>
public class ResultDeliveryRefreshCopyTests
{
    private sealed class CountingSender : ISender
    {
        public int LoadCalls { get; private set; }
        public IReadOnlyList<UndeliveredPatientRowDto> Rows { get; set; } = Array.Empty<UndeliveredPatientRowDto>();

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is GetUndeliveredResultsQuery)
            {
                LoadCalls++;
                return Task.FromResult((TResponse)(object)Result<IReadOnlyList<UndeliveredPatientRowDto>>.Success(Rows));
            }

            throw new NotSupportedException($"CountingSender has no canned response for {request.GetType().Name}.");
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest => throw new NotSupportedException();
        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private sealed class SilentNavigation : INavigationService
    {
        public ViewModelBase? CurrentViewModel => null;
        public event Action<ViewModelBase?>? Navigated
        {
            add { }
            remove { }
        }
        public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase { }
        public void NavigateTo(ViewModelBase viewModel) { }
    }

    private static UndeliveredPatientRowDto Row(
        int patientId = 7, string? notes = null) => new(
        PatientId: patientId,
        PatientFullName: "أحمد محمد علي",
        LabId: "LAB-123",
        RegistrationDateUtc: new DateTime(2026, 10, 25, 9, 30, 0, DateTimeKind.Utc),
        VisitStatus: 3,
        UndeliveredCount: 4,
        PatientNotes: notes);

    // ================================================================
    // Refresh (1.4.1.4.2)
    // ================================================================

    [Fact]
    public async Task Refresh_ReExecutesTheCurrentLoad()
    {
        var sender = new CountingSender { Rows = new[] { Row() } };
        var vm = new ResultDeliveryViewModel(
            sender, new ResultErrorPresenter(), new SilentNavigation(), new FakeClipboardService());

        await vm.LoadAsync();
        Assert.Equal(1, sender.LoadCalls);

        vm.RefreshCommand.Execute(null);
        await Task.Yield();
        await Task.Delay(120);

        Assert.Equal(2, sender.LoadCalls);
        Assert.Single(vm.Items);
    }

    [Fact]
    public async Task Refresh_ReusesThePeriodTheUserIsViewing()
    {
        var sender = new CountingSender();
        var vm = new ResultDeliveryViewModel(
            sender, new ResultErrorPresenter(), new SilentNavigation(), new FakeClipboardService());
        vm.From = new DateOnly(2026, 10, 1);
        vm.To = new DateOnly(2026, 10, 20);

        vm.RefreshCommand.Execute(null);
        await Task.Yield();
        await Task.Delay(120);

        Assert.Equal(1, sender.LoadCalls);
        Assert.Equal(new DateOnly(2026, 10, 1), vm.From);
        Assert.Equal(new DateOnly(2026, 10, 20), vm.To);
    }

    // ================================================================
    // Copy (1.4.1.4.5)
    // ================================================================

    [Fact]
    public void Copy_PutsTheExactSummaryOnTheClipboard()
    {
        var clipboard = new FakeClipboardService();
        var vm = new ResultDeliveryViewModel(
            new CountingSender(), new ResultErrorPresenter(), new SilentNavigation(), clipboard);
        var row = Row();
        vm.SelectedItem = row;

        vm.CopyCommand.Execute(row);
        Assert.Single(clipboard.CopiedTexts);

        Assert.Equal(
            "المريض: أحمد محمد علي — Lab ID: LAB-123 — التاريخ: 2026-10-25 — الحالة: 3 — غير المسلَّم: 4",
            clipboard.LastText);
    }

    [Fact]
    public void Copy_IncludesTheNoteWhenPresent()
    {
        var clipboard = new FakeClipboardService();
        var vm = new ResultDeliveryViewModel(
            new CountingSender(), new ResultErrorPresenter(), new SilentNavigation(), clipboard);
        var row = Row(notes: "حساسية بنسلين");
        vm.SelectedItem = row;

        vm.CopyCommand.Execute(row);

        Assert.Contains("ملاحظات: حساسية بنسلين", clipboard.LastText!, StringComparison.Ordinal);
    }

    [Fact]
    public void Copy_WithNoRowSelected_IsDisabledAndCopiesNothing()
    {
        var clipboard = new FakeClipboardService();
        var vm = new ResultDeliveryViewModel(
            new CountingSender(), new ResultErrorPresenter(), new SilentNavigation(), clipboard);

        Assert.False(vm.CopyCommand.CanExecute(null));

        vm.CopyCommand.Execute(null);

        Assert.Empty(clipboard.CopiedTexts);
    }

    [Fact]
    public void Copy_WithARowSelected_IsEnabled()
    {
        var vm = new ResultDeliveryViewModel(
            new CountingSender(), new ResultErrorPresenter(), new SilentNavigation(), new FakeClipboardService());
        vm.SelectedItem = Row();

        Assert.True(vm.CopyCommand.CanExecute(vm.SelectedItem));
    }

    [Fact]
    public void Copy_IgnoresAParameterThatIsNotARow()
    {
        var clipboard = new FakeClipboardService();
        var vm = new ResultDeliveryViewModel(
            new CountingSender(), new ResultErrorPresenter(), new SilentNavigation(), clipboard);

        vm.CopyCommand.Execute("not a row");

        Assert.Empty(clipboard.CopiedTexts);
    }

    [Fact]
    public void View_BindsTheRefreshAndCopyButtons()
    {
        var xaml = ReadPresentationFile(Path.Combine("Views", "Patients", "ResultDeliveryView.xaml"));

        Assert.Contains("Command=\"{Binding RefreshCommand}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Command=\"{Binding CopyCommand}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("FlowDirection=\"RightToLeft\"", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void ViewModel_DoesNotReferenceTheWpfClipboardDirectly()
    {
        var source = ReadPresentationFile(Path.Combine("ViewModels", "Patients", "ResultDeliveryViewModel.cs"));

        Assert.DoesNotContain("System.Windows.Clipboard", source, StringComparison.Ordinal);
        Assert.Contains("IClipboardService", source, StringComparison.Ordinal);
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
