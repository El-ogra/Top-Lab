using System.Collections.ObjectModel;
using System.IO;
using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ResultDelivery.Common;
using TopLab.Application.Features.ResultDelivery.Queries.GetUndeliveredResults;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Common.Clipboard;
using TopLab.Presentation.Common.Navigation;
using TopLab.Presentation.Tests.Common;
using TopLab.Presentation.ViewModels.Patients;
using Xunit;

namespace TopLab.Presentation.Tests.Patients;

/// <summary>
/// R1 Round 3 A2: the delivery list's three new account/report buttons.
/// <para>
/// The grid's real selection binds <c>DataGrid.SelectedItem</c> to
/// <see cref="ResultDeliveryViewModel.SelectedItem"/> (verified in
/// <c>ResultDeliveryView.xaml</c>); each button passes it as <c>CommandParameter</c>.
/// With a selected row the command navigates to the right ViewModel with the patient id;
/// with no row selected the command is disabled and does nothing.
/// </para>
/// </summary>
public class ResultDeliveryRoundThreeTests
{
    private sealed class StubSender : ISender
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is GetUndeliveredResultsQuery)
            {
                return Task.FromResult((TResponse)(object)Result<IReadOnlyList<UndeliveredPatientRowDto>>
                    .Success(Array.Empty<UndeliveredPatientRowDto>()));
            }

            throw new NotSupportedException($"StubSender has no canned response for {request.GetType().Name}.");
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest => throw new NotSupportedException();
        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    /// <summary>Records the navigation target and hands back a real instance of it.</summary>
    private sealed class LiveNavigation : INavigationService
    {
        private readonly Func<ViewModelBase>? _factory;
        public List<Type> Targets { get; } = new();
        public ViewModelBase? CurrentViewModel { get; private set; }

        public LiveNavigation(Func<ViewModelBase>? factory = null) => _factory = factory;

        public event Action<ViewModelBase?>? Navigated
        {
            add { }
            remove { }
        }

        public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
        {
            Targets.Add(typeof(TViewModel));
            CurrentViewModel = _factory?.Invoke();
        }

        public void NavigateTo(ViewModelBase viewModel)
        {
            Targets.Add(viewModel.GetType());
            CurrentViewModel = viewModel;
        }
    }

    private sealed class StubServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private static UndeliveredPatientRowDto Row(int patientId) => new(
        PatientId: patientId,
        PatientFullName: $"Patient {patientId}",
        LabId: "LAB-A",
        RegistrationDateUtc: new DateTime(2026, 10, 25, 9, 0, 0, DateTimeKind.Utc),
        VisitStatus: 1,
        UndeliveredCount: 2);

    private static ResultDeliveryViewModel Vm(INavigationService navigation, IClipboardService? clipboard = null)
        => new(new StubSender(), new ResultErrorPresenter(), navigation, clipboard ?? new FakeClipboardService());

    // ================================================================
    // SelectedItem is the real grid selection
    // ================================================================

    [Fact]
    public void View_BindsTheGridSelectionToSelectedItem()
    {
        var xaml = ReadPresentationFile(Path.Combine("Views", "Patients", "ResultDeliveryView.xaml"));

        Assert.Contains("SelectedItem=\"{Binding SelectedItem}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("CommandParameter=\"{Binding SelectedItem}\"", xaml, StringComparison.Ordinal);
    }

    // ================================================================
    // No row selected: disabled and inert
    // ================================================================

    [Fact]
    public void WithNoRowSelected_TheThreeCommandsCannotExecute()
    {
        var vm = Vm(new LiveNavigation());

        Assert.False(vm.PatientAccountCommand.CanExecute(null));
        Assert.False(vm.CombinedReportCommand.CanExecute(null));
        Assert.False(vm.EmptyReportCommand.CanExecute(null));
    }

    [Fact]
    public void WithNoRowSelected_ExecutingDoesNothing()
    {
        var nav = new LiveNavigation();
        var vm = Vm(nav);

        vm.PatientAccountCommand.Execute(null);
        vm.CombinedReportCommand.Execute(null);
        vm.EmptyReportCommand.Execute(null);

        Assert.Empty(nav.Targets);
    }

    [Fact]
    public void WithARowSelected_TheThreeCommandsCanExecute()
    {
        var vm = Vm(new LiveNavigation());
        vm.SelectedItem = Row(7);

        Assert.True(vm.PatientAccountCommand.CanExecute(vm.SelectedItem));
        Assert.True(vm.CombinedReportCommand.CanExecute(vm.SelectedItem));
        Assert.True(vm.EmptyReportCommand.CanExecute(vm.SelectedItem));
    }

    [Fact]
    public void ARowWithNoPatientId_DoesNotEnableTheCommands()
    {
        var vm = Vm(new LiveNavigation());
        vm.SelectedItem = Row(0);

        Assert.False(vm.PatientAccountCommand.CanExecute(vm.SelectedItem));
    }

    // ================================================================
    // A selected row navigates to the right ViewModel with the patient id
    // ================================================================

    [Fact]
    public void PatientAccount_WithASelectedRow_NavigatesToTheAccountScreen()
    {
        var nav = new LiveNavigation(() => new PatientAccountViewModel(
            new StubSender(), new LiveNavigation(), new ResultErrorPresenter(), new FakeDialogService(), new StubServiceProvider()));
        var vm = Vm(nav);
        var row = Row(7);
        vm.SelectedItem = row;

        vm.PatientAccountCommand.Execute(row);

        Assert.Contains(typeof(PatientAccountViewModel), nav.Targets);
        Assert.IsType<PatientAccountViewModel>(nav.CurrentViewModel);
    }

    [Fact]
    public void CombinedReport_WithASelectedRow_NavigatesToTheCombinedReportScreen()
    {
        var nav = new LiveNavigation(() => new CombinedReportViewModel(
            new StubSender(), new ResultErrorPresenter(), new FakeDialogService(), new StubServiceProvider(),
            new LiveNavigation(), new UnusedPdfPreview(), new UnusedDbContext()));
        var vm = Vm(nav);
        var row = Row(9);
        vm.SelectedItem = row;

        vm.CombinedReportCommand.Execute(row);

        Assert.Contains(typeof(CombinedReportViewModel), nav.Targets);
    }

    [Fact]
    public void EmptyReport_WithASelectedRow_NavigatesToTheBlankReportScreen()
    {
        var nav = new LiveNavigation(() => new BlankReportViewModel(
            new StubSender(), new ResultErrorPresenter(), new LiveNavigation()));
        var vm = Vm(nav);
        var row = Row(11);
        vm.SelectedItem = row;

        vm.EmptyReportCommand.Execute(row);

        Assert.Contains(typeof(BlankReportViewModel), nav.Targets);
    }

    [Fact]
    public void PatientData_WithASelectedRow_NavigatesToThePatientEditor()
    {
        var nav = new LiveNavigation(() => new PatientEditorViewModel(
            new StubSender(), new LiveNavigation(), new ResultErrorPresenter(), new FakeDialogService(), new StubServiceProvider()));
        var vm = Vm(nav);
        var row = Row(13);
        vm.SelectedItem = row;

        vm.PatientDataCommand.Execute(row);

        Assert.Contains(typeof(PatientEditorViewModel), nav.Targets);
    }

    [Fact]
    public void PatientData_WithNoRowSelected_IsDisabled()
    {
        var vm = Vm(new LiveNavigation());

        Assert.False(vm.PatientDataCommand.CanExecute(null));
    }

    [Fact]
    public async Task PrintEnvelope_WithASelectedRow_SendsTheExistingEnvelopeCommand()
    {
        var sender = new EnvelopeSender();
        var vm = new ResultDeliveryViewModel(
            sender, new ResultErrorPresenter(), new LiveNavigation(), new FakeClipboardService());
        var row = Row(17);
        vm.SelectedItem = row;

        vm.PrintEnvelopeCommand.Execute(row);
        await Task.Yield();
        await Task.Delay(120);

        Assert.Equal(new[] { 17 }, sender.EnvelopePatientIds);
    }

    [Fact]
    public void PrintEnvelope_WithNoRowSelected_IsDisabled()
    {
        var vm = Vm(new LiveNavigation());

        Assert.False(vm.PrintEnvelopeCommand.CanExecute(null));
    }

    /// <summary>Sender that also answers the envelope command, so the wiring can be asserted.</summary>
    private sealed class EnvelopeSender : ISender
    {
        public List<int> EnvelopePatientIds { get; } = new();

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is GetUndeliveredResultsQuery)
            {
                return Task.FromResult((TResponse)(object)Result<IReadOnlyList<UndeliveredPatientRowDto>>
                    .Success(Array.Empty<UndeliveredPatientRowDto>()));
            }

            if (request is TopLab.Application.Features.PatientEnvelope.Commands.PrintEnvelope.PrintEnvelopeCommand envelope)
            {
                EnvelopePatientIds.Add(envelope.PatientId);
                return Task.FromResult((TResponse)(object)Result.Success());
            }

            throw new NotSupportedException($"EnvelopeSender has no canned response for {request.GetType().Name}.");
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest => throw new NotSupportedException();
        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    [Fact]
    public void Commands_IgnoreAParameterThatIsNotARow()
    {
        var nav = new LiveNavigation();
        var vm = Vm(nav);

        vm.PatientAccountCommand.Execute("not a row");
        vm.CombinedReportCommand.Execute(42);

        Assert.Empty(nav.Targets);
    }

    // ================================================================
    // Helpers
    // ================================================================

    private sealed class UnusedPdfPreview : TopLab.Application.Features.ReportProduction.Common.IPdfPreviewService
    {
        public Task<Result<string>> PreviewAsync(string absolutePdfPath, CancellationToken cancellationToken = default)
            => throw new NotSupportedException("The account/report buttons do not preview on this path.");
    }

    private sealed class UnusedDbContext : TopLab.Application.Common.Interfaces.IApplicationDbContext
    {
        public IQueryable<TEntity> Set<TEntity>() where TEntity : class => throw new NotSupportedException();
        public void Add<TEntity>(TEntity entity) where TEntity : class => throw new NotSupportedException();
        public void Update<TEntity>(TEntity entity) where TEntity : class => throw new NotSupportedException();
        public void Remove<TEntity>(TEntity entity) where TEntity : class => throw new NotSupportedException();
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<bool> CanConnectAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
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
