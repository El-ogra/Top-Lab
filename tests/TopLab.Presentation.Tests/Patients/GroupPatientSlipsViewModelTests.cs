using System.IO;
using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ResultDelivery.Commands.PrintGroupPatientSlips;
using TopLab.Application.Features.ResultDelivery.Common;
using TopLab.Application.Features.ResultDelivery.Queries.GetUndeliveredResults;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.ViewModels.Patients;
using Xunit;

namespace TopLab.Presentation.Tests.Patients;

/// <summary>
/// Phase 6 (GAP-013): the group-patient report slip screen.
/// Covers selection-to-query parameters, writer invocation through the print
/// command, empty-selection validation, the safe default state and the
/// follow-up/delivery entry point.
/// </summary>
public class GroupPatientSlipsViewModelTests
{
    private sealed class GroupSlipSender : ISender
    {
        public List<GetUndeliveredResultsQuery> ListQueries { get; } = new();

        public List<PrintGroupPatientSlipsCommand> PrintCommands { get; } = new();

        public Result<IReadOnlyList<UndeliveredPatientRowDto>>? ListResult { get; set; }

        public Result? PrintResult { get; set; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is GetUndeliveredResultsQuery listQuery)
            {
                ListQueries.Add(listQuery);
                var response = ListResult ?? Result<IReadOnlyList<UndeliveredPatientRowDto>>.Success(
                    new List<UndeliveredPatientRowDto> { Row(1, "أحمد محمد"), Row(2, "سيد علي") });
                return Task.FromResult((TResponse)(object)response);
            }

            if (request is PrintGroupPatientSlipsCommand printCommand)
            {
                PrintCommands.Add(printCommand);
                return Task.FromResult((TResponse)(object)(PrintResult ?? Result.Success()));
            }

            throw new NotSupportedException($"GroupSlipSender has no canned response for {request.GetType().Name}.");
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

    private static UndeliveredPatientRowDto Row(int patientId, string name)
        => new(patientId, name, $"10{patientId}", new DateTime(2026, 10, 1, 9, 0, 0, DateTimeKind.Utc), 0, 1);

    private static (GroupPatientSlipsViewModel Vm, GroupSlipSender Sender) Build()
    {
        var sender = new GroupSlipSender();
        var vm = new GroupPatientSlipsViewModel(
            sender,
            new ResultErrorPresenter(),
            new NoOpNavigationService());
        return (vm, sender);
    }

    private sealed class NoOpNavigationService : TopLab.Presentation.Common.Navigation.INavigationService
    {
        public TopLab.Presentation.Common.ViewModelBase? CurrentViewModel { get; private set; }

        public event Action<TopLab.Presentation.Common.ViewModelBase?>? Navigated;

        public void NavigateTo<TViewModel>() where TViewModel : TopLab.Presentation.Common.ViewModelBase
            => Navigated?.Invoke(null);

        public void NavigateTo(TopLab.Presentation.Common.ViewModelBase viewModel)
        {
            CurrentViewModel = viewModel;
            Navigated?.Invoke(viewModel);
        }
    }

    [Fact]
    public void SafeDefaultState_NothingSelected_NoErrors_NoBusy()
    {
        var (vm, _) = Build();

        Assert.Empty(vm.Items);
        Assert.Equal(0, vm.SelectedCount);
        Assert.False(vm.HasRows);
        Assert.False(vm.IsBusy);
        Assert.Empty(vm.ErrorMessage);
        Assert.Empty(vm.StatusMessage);
        Assert.NotNull(vm.GenerateCommand);
        Assert.NotNull(vm.LoadCommand);
        Assert.NotNull(vm.SelectAllCommand);
        Assert.NotNull(vm.ClearSelectionCommand);
        Assert.NotNull(vm.BackCommand);
    }

    [Fact]
    public async Task LoadAsync_DefaultPeriodIsToday_AndRowsComeFromTheDeliveryListQuery()
    {
        var (vm, sender) = Build();

        await vm.LoadAsync();

        Assert.Equal(2, vm.Items.Count);
        Assert.True(vm.HasRows);
        var query = Assert.Single(sender.ListQueries);
        Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow), query.From);
        Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow), query.To);
        Assert.Equal(1, query.Page);
    }

    [Fact]
    public async Task LoadAsync_SelectedPeriod_IsPassedToTheListQuery()
    {
        var (vm, sender) = Build();
        vm.From = new DateOnly(2026, 9, 1);
        vm.To = new DateOnly(2026, 9, 30);

        await vm.LoadAsync();

        var query = Assert.Single(sender.ListQueries);
        Assert.Equal(new DateOnly(2026, 9, 1), query.From);
        Assert.Equal(new DateOnly(2026, 9, 30), query.To);
    }

    [Fact]
    public async Task LoadAsync_ListFailure_SurfacesErrorAndShowsNoRows()
    {
        var (vm, sender) = Build();
        sender.ListResult = Result<IReadOnlyList<UndeliveredPatientRowDto>>.Failure(
            Error.Unexpected("تعذر تحميل القائمة."));

        await vm.LoadAsync();

        Assert.Empty(vm.Items);
        Assert.NotEmpty(vm.ErrorMessage);
    }

    [Fact]
    public async Task Generate_WithoutSelection_ShowsValidationMessageAndNeverPrints()
    {
        var (vm, sender) = Build();
        await vm.LoadAsync();

        vm.GenerateCommand.Execute(null);
        await Task.Yield();
        await Task.Delay(100);

        Assert.Equal("حدد مريضاً واحداً على الأقل لطباعة القصاصات.", vm.ErrorMessage);
        Assert.Empty(sender.PrintCommands);
        Assert.Empty(vm.StatusMessage);
    }

    [Fact]
    public async Task Generate_WithSelection_PassesExactlyTheSelectedIdsToThePrintCommand()
    {
        var (vm, sender) = Build();
        await vm.LoadAsync();
        vm.Items.Single(r => r.PatientId == 2).IsSelected = true;

        vm.GenerateCommand.Execute(null);
        await Task.Yield();
        await Task.Delay(100);

        var command = Assert.Single(sender.PrintCommands);
        Assert.Equal(new[] { 2 }, command.PatientIds.ToArray());
        Assert.Empty(vm.ErrorMessage);
        Assert.NotEmpty(vm.StatusMessage);
    }

    [Fact]
    public async Task Generate_WithMultipleSelection_SendsEverySelectedId()
    {
        var (vm, sender) = Build();
        await vm.LoadAsync();
        vm.SelectAllCommand.Execute(null);

        Assert.Equal(2, vm.SelectedCount);

        vm.GenerateCommand.Execute(null);
        await Task.Yield();
        await Task.Delay(100);

        var command = Assert.Single(sender.PrintCommands);
        Assert.Equal(new[] { 1, 2 }, command.PatientIds.ToArray());
    }

    [Fact]
    public async Task ClearSelection_DeselectsEveryRow()
    {
        var (vm, _) = Build();
        await vm.LoadAsync();
        vm.SelectAllCommand.Execute(null);
        Assert.Equal(2, vm.SelectedCount);

        vm.ClearSelectionCommand.Execute(null);

        Assert.Equal(0, vm.SelectedCount);
    }

    [Fact]
    public async Task Generate_PrintFailure_SurfacesErrorAndNoStatus()
    {
        var (vm, sender) = Build();
        await vm.LoadAsync();
        vm.SelectAllCommand.Execute(null);
        sender.PrintResult = Result.Failure(Error.Unexpected("تعذر طباعة قصاصات المرضى."));

        vm.GenerateCommand.Execute(null);
        await Task.Yield();
        await Task.Delay(100);

        Assert.NotEmpty(vm.ErrorMessage);
        Assert.Empty(vm.StatusMessage);
    }

    [Fact]
    public void GroupSlipsView_IsWiredIntoMainWindow_AndIsRightToLeft()
    {
        var root = FindSolutionRoot();
        var mainWindow = File.ReadAllText(Path.Combine(root, "src", "TopLab.Presentation", "MainWindow.xaml"));

        Assert.Contains("patientsVm:GroupPatientSlipsViewModel", mainWindow, StringComparison.Ordinal);
        Assert.Contains("patientsView:GroupPatientSlipsView", mainWindow, StringComparison.Ordinal);

        var view = File.ReadAllText(Path.Combine(
            root, "src", "TopLab.Presentation", "Views", "Patients", "GroupPatientSlipsView.xaml"));

        Assert.Contains("FlowDirection=\"RightToLeft\"", view, StringComparison.Ordinal);
        Assert.Contains("Command=\"{Binding GenerateCommand}\"", view, StringComparison.Ordinal);
    }

    [Fact]
    public void GroupSlips_EntryPoint_ExistsOnTheDeliveryScreen()
    {
        var root = FindSolutionRoot();

        var deliveryView = File.ReadAllText(Path.Combine(
            root, "src", "TopLab.Presentation", "Views", "Patients", "ResultDeliveryView.xaml"));
        Assert.Contains("Command=\"{Binding OpenGroupSlipsCommand}\"", deliveryView, StringComparison.Ordinal);

        var deliveryVm = File.ReadAllText(Path.Combine(
            root, "src", "TopLab.Presentation", "ViewModels", "Patients", "ResultDeliveryViewModel.cs"));
        Assert.Contains("NavigateTo<GroupPatientSlipsViewModel>", deliveryVm, StringComparison.Ordinal);

        // The screen is registered in the composition root.
        var di = File.ReadAllText(Path.Combine(root, "src", "TopLab.Presentation", "DependencyInjection.cs"));
        Assert.Contains("AddTransient<GroupPatientSlipsViewModel>", di, StringComparison.Ordinal);
    }

    private static string FindSolutionRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "TopLab.sln")))
        {
            dir = dir.Parent;
        }

        Assert.NotNull(dir);
        return dir!.FullName;
    }
}
